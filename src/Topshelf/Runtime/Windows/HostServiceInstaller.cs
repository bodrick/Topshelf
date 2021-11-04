// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using Topshelf.Exceptions;

namespace Topshelf.Runtime.Windows
{
    public sealed class HostServiceInstaller : IDisposable
    {
        private readonly Installer _installer;
        private readonly TransactedInstaller _transactedInstaller;
        private bool _disposed;

        public HostServiceInstaller(IInstallHostSettings settings)
        {
            _installer = CreateInstaller(settings);
            _transactedInstaller = CreateTransactedInstaller(_installer);
        }

        public HostServiceInstaller(IHostSettings settings)
        {
            _installer = CreateInstaller(settings);
            _transactedInstaller = CreateTransactedInstaller(_installer);
        }

        public ServiceProcessInstaller ServiceProcessInstaller => (ServiceProcessInstaller)_installer.Installers[1];

        public void Dispose() => Dispose(true);

        public void InstallService(Action<InstallEventArgs> beforeInstall, Action<InstallEventArgs> afterInstall,
            Action<InstallEventArgs> beforeRollback, Action<InstallEventArgs> afterRollback)
        {
            if (beforeInstall != null)
            {
                _installer.BeforeInstall += (_, args) => beforeInstall(args);
            }

            if (afterInstall != null)
            {
                _installer.AfterInstall += (_, args) => afterInstall(args);
            }

            if (beforeRollback != null)
            {
                _installer.BeforeRollback += (_, args) => beforeRollback(args);
            }

            if (afterRollback != null)
            {
                _installer.AfterRollback += (_, args) => afterRollback(args);
            }

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            _transactedInstaller.Install(new Hashtable());
        }

        public void UninstallService(Action<InstallEventArgs> beforeUninstall, Action<InstallEventArgs> afterUninstall)
        {
            if (beforeUninstall != null)
            {
                _installer.BeforeUninstall += (_, args) => beforeUninstall(args);
            }

            if (afterUninstall != null)
            {
                _installer.AfterUninstall += (_, args) => afterUninstall(args);
            }

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            _transactedInstaller.Uninstall(null);
        }

        private static ServiceInstaller ConfigureServiceInstaller(IHostSettings settings, string[] dependencies, HostStartMode startMode)
        {
            var installer = new ServiceInstaller
            {
                ServiceName = settings.ServiceName,
                Description = settings.Description,
                DisplayName = settings.DisplayName,
                ServicesDependedOn = dependencies
            };

            SetStartMode(installer, startMode);
            return installer;
        }

        private static ServiceProcessInstaller ConfigureServiceProcessInstaller(ServiceAccount account, string username, string password) =>
            new() { Username = username, Password = password, Account = account, };

        private static Installer CreateHostInstaller(IHostSettings settings, Installer[] installers)
        {
            var arguments = " ";
            if (!string.IsNullOrEmpty(settings.InstanceName))
            {
                arguments += $" -instance \"{settings.InstanceName}\"";
            }

            if (!string.IsNullOrEmpty(settings.DisplayName))
            {
                arguments += $" -displayname \"{settings.DisplayName}\"";
            }

            if (!string.IsNullOrEmpty(settings.Name))
            {
                arguments += $" -servicename \"{settings.Name}\"";
            }

            return new HostInstaller(settings, arguments, installers);
        }

        private static Installer CreateInstaller(IInstallHostSettings settings)
        {
            var installers = new Installer[]
            {
                ConfigureServiceInstaller(settings, settings.Dependencies, settings.StartMode), ConfigureServiceProcessInstaller(
                    settings.Credentials.Account, settings.Credentials.Username,
                    settings.Credentials.Password)
            };

            //DO not auto create EventLog Source while install service
            //MSDN: When the installation is performed, it automatically creates an EventLogInstaller to install the event log source associated with the ServiceBase derived class. The Log property for this source is set by the ServiceInstaller constructor to the computer's Application log. When you set the ServiceName of the ServiceInstaller (which should be identical to the ServiceBase..::.ServiceName of the service), the Source is automatically set to the same value. In an installation failure, the source's installation is rolled-back along with previously installed services.
            //MSDN: from EventLog.CreateEventSource Method (String, String) : an ArgumentException thrown when The first 8 characters of logName match the first 8 characters of an existing event log name.
            RemoveEventLogInstallers(installers);

            return CreateHostInstaller(settings, installers);
        }

        private static Installer CreateInstaller(IHostSettings settings)
        {
            var installers = new Installer[]
            {
                ConfigureServiceInstaller(settings, Array.Empty<string>(), HostStartMode.Automatic),
                ConfigureServiceProcessInstaller(ServiceAccount.LocalService, string.Empty, string.Empty),
            };

            RemoveEventLogInstallers(installers);

            return CreateHostInstaller(settings, installers);
        }

        private static TransactedInstaller CreateTransactedInstaller(Installer installer)
        {
            var transactedInstaller = new TransactedInstaller();

            transactedInstaller.Installers.Add(installer);

            var assembly = Assembly.GetEntryAssembly();

            var currentProcess = Process.GetCurrentProcess();

            if (assembly == null)
            {
                throw new TopshelfException("Assembly.GetEntryAssembly() is null for some reason.");
            }

            if (currentProcess == null)
            {
                throw new TopshelfException("Process.GetCurrentProcess() is null for some reason.");
            }

            if (currentProcess.MainModule == null)
            {
                throw new TopshelfException("Process.GetCurrentProcess().MainModule is null for some reason.");
            }

            var path =
                IsDotnetExe(currentProcess)
                    ? $"/assemblypath={currentProcess.MainModule.FileName} \"{assembly.Location}\""
                    : $"/assemblypath={currentProcess.MainModule.FileName}";

            string[] commandLine = { path };

            transactedInstaller.Context = new InstallContext(null, commandLine);

            return transactedInstaller;
        }

        private static bool IsDotnetExe(Process process) =>
            process.MainModule?.ModuleName?.Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase) == true;

        private static void RemoveEventLogInstallers(IEnumerable<Installer> installers)
        {
            foreach (var installer in installers)
            {
                foreach (var eventLogInstaller in installer.Installers.OfType<EventLogInstaller>().ToArray())
                {
                    installer.Installers.Remove(eventLogInstaller);
                }
            }
        }

        private static void SetStartMode(ServiceInstaller installer, HostStartMode startMode)
        {
            switch (startMode)
            {
                case HostStartMode.Automatic:
                    installer.StartType = ServiceStartMode.Automatic;
                    break;

                case HostStartMode.Manual:
                    installer.StartType = ServiceStartMode.Manual;
                    break;

                case HostStartMode.Disabled:
                    installer.StartType = ServiceStartMode.Disabled;
                    break;

                case HostStartMode.AutomaticDelayed:
                    installer.StartType = ServiceStartMode.Automatic;
                    installer.DelayedAutoStart = true;
                    break;
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                try
                {
                    _transactedInstaller.Dispose();
                }
                finally
                {
                    _installer.Dispose();
                }
            }

            _disposed = true;
        }
    }
}
