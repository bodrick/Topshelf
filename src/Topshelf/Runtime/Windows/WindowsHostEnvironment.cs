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
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using Topshelf.Configuration.HostConfigurators;
using Topshelf.Exceptions;
using Topshelf.Logging;

namespace Topshelf.Runtime.Windows
{
    public class WindowsHostEnvironment : IHostEnvironment
    {
        private readonly IHostConfigurator _hostConfigurator;
        private readonly ILogWriter _log = HostLogger.Get(typeof(WindowsHostEnvironment));

        public WindowsHostEnvironment(IHostConfigurator configurator) => _hostConfigurator = configurator;

        public string CommandLine => Configuration.CommandLineParser.CommandLine.GetUnparsedCommandLine();

        public bool IsAdministrator
        {
            get
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public bool IsRunningAsAService
        {
            get
            {
                try
                {
                    var process = GetParent(Process.GetCurrentProcess());
                    if (process?.ProcessName == "services")
                    {
                        _log.Debug("Started by the Windows services process");
                        return true;
                    }
                }
                catch (InvalidOperationException)
                {
                    // again, mono seems to fail with this, let's just return false okay?
                }

                return false;
            }
        }

        public IHost CreateServiceHost(IHostSettings settings, IServiceHandle serviceHandle) =>
            new WindowsServiceHost(this, settings, serviceHandle, _hostConfigurator);

        public void InstallService(IInstallHostSettings settings, Action<IInstallHostSettings> beforeInstall, Action afterInstall,
            Action beforeRollback, Action afterRollback)
        {
            using var installer = new HostServiceInstaller(settings);

            void BeforeInstall(InstallEventArgs _)
            {
                beforeInstall(settings);
                installer.ServiceProcessInstaller.Username = settings.Credentials.Username;
                installer.ServiceProcessInstaller.Account = settings.Credentials.Account;

                var gMSA = settings.Credentials.Username.EndsWith("$", StringComparison.OrdinalIgnoreCase);
                // Group Managed Service Account (gMSA) workaround per
                // https://connect.microsoft.com/VisualStudio/feedback/details/795196/service-process-installer-should-support-virtual-service-accounts
                if (settings.Credentials.Account == ServiceAccount.User && (gMSA || string.Equals(settings.Credentials.Username,
                    "NT SERVICE\\" + settings.ServiceName, StringComparison.OrdinalIgnoreCase)))
                {
                    _log.InfoFormat(gMSA ? "Installing as gMSA {0}." : "Installing as virtual service account",
                        settings.Credentials.Username);
                    installer.ServiceProcessInstaller.Password = string.Empty;
                    installer.ServiceProcessInstaller.GetType().GetField("haveLoginInfo", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.SetValue(installer.ServiceProcessInstaller, true);
                }
                else
                {
                    installer.ServiceProcessInstaller.Password = settings.Credentials.Password;
                }
            }

            void AfterInstall(InstallEventArgs _) => afterInstall();
            void BeforeRollback(InstallEventArgs _) => beforeRollback();
            void AfterRollback(InstallEventArgs _) => afterRollback();

            installer.InstallService(BeforeInstall, AfterInstall, BeforeRollback, AfterRollback);
        }

        public bool IsServiceInstalled(string serviceName) => Type.GetType("Mono.Runtime") == null && IsServiceListed(serviceName);

        public bool IsServiceStopped(string serviceName)
        {
            using var sc = new ServiceController(serviceName);
            return sc.Status == ServiceControllerStatus.Stopped;
        }

        public bool RunAsAdministrator()
        {
            if (Environment.OSVersion.Version.Major == 6)
            {
                var commandLine = CommandLine.Replace("--sudo", string.Empty, StringComparison.OrdinalIgnoreCase);

                var assemblyLocation = Assembly.GetEntryAssembly()?.Location;
                if (assemblyLocation == null)
                {
                    throw new TopshelfException("Unable to determine start assembly");
                }

                var startInfo = new ProcessStartInfo(assemblyLocation, commandLine)
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                };

                try
                {
                    HostLogger.Shutdown();
                    var process = Process.Start(startInfo);
                    process?.WaitForExit();
                    return true;
                }
                catch (Win32Exception ex)
                {
                    _log.Debug("Process Start Exception", ex);
                }
            }

            return false;
        }

        public void SendServiceCommand(string serviceName, int command)
        {
            using var sc = new ServiceController(serviceName);
            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.ExecuteCommand(command);
            }
            else
            {
                _log.WarnFormat("The {0} service can't be commanded now as it has the status {1}. Try again later...", serviceName,
                    sc.Status.ToString());
            }
        }

        public void StartService(string serviceName, TimeSpan startTimeOut)
        {
            using var sc = new ServiceController(serviceName);
            if (sc.Status == ServiceControllerStatus.Running)
            {
                _log.InfoFormat("The {0} service is already running.", serviceName);
                return;
            }

            if (sc.Status == ServiceControllerStatus.StartPending)
            {
                _log.InfoFormat("The {0} service is already starting.", serviceName);
                return;
            }

            if (sc.Status is ServiceControllerStatus.Stopped or ServiceControllerStatus.Paused)
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, startTimeOut);
            }
            else
            {
                // Status is StopPending, ContinuePending or PausedPending, print warning
                _log.WarnFormat("The {0} service can't be started now as it has the status {1}. Try again later...", serviceName,
                    sc.Status.ToString());
            }
        }

        public void StopService(string serviceName, TimeSpan stopTimeOut)
        {
            using var sc = new ServiceController(serviceName);
            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                _log.InfoFormat("The {0} service is not running.", serviceName);
                return;
            }

            if (sc.Status == ServiceControllerStatus.StopPending)
            {
                _log.InfoFormat("The {0} service is already stopping.", serviceName);
                return;
            }

            if (sc.Status is ServiceControllerStatus.Running or ServiceControllerStatus.Paused)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, stopTimeOut);
            }
            else
            {
                // Status is StartPending, ContinuePending or PausedPending, print warning
                _log.WarnFormat("The {0} service can't be stopped now as it has the status {1}. Try again later...", serviceName,
                    sc.Status.ToString());
            }
        }

        public void UninstallService(IHostSettings settings, Action beforeUninstall, Action afterUninstall)
        {
            using var installer = new HostServiceInstaller(settings);
            void BeforeUninstall(InstallEventArgs _) => beforeUninstall();
            void AfterUninstall(InstallEventArgs _) => afterUninstall();
            installer.UninstallService(BeforeUninstall, AfterUninstall);
        }

        private Process? GetParent(Process child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            try
            {
                var parentPid = 0;

                var hnd = Kernel32.CreateToolhelp32Snapshot(Kernel32.TH32CS_SNAPPROCESS, 0);

                if (hnd == IntPtr.Zero)
                {
                    return null;
                }

                var processInfo = new Kernel32.PROCESSENTRY32 { dwSize = (uint)Marshal.SizeOf(typeof(Kernel32.PROCESSENTRY32)) };

                if (!Kernel32.Process32First(hnd, ref processInfo))
                {
                    return null;
                }

                do
                {
                    if (child.Id == processInfo.th32ProcessID)
                    {
                        parentPid = (int)processInfo.th32ParentProcessID;
                    }
                } while (parentPid == 0 && Kernel32.Process32Next(hnd, ref processInfo));

                if (parentPid > 0)
                {
                    return Process.GetProcessById(parentPid);
                }
            }
            catch (Exception ex)
            {
                _log.Error("Unable to get parent process (ignored)", ex);
            }

            return null;
        }

        private bool IsServiceListed(string serviceName)
        {
            var result = false;

            try
            {
                result = ServiceController.GetServices().Any(service =>
                    string.Equals(service.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase));
            }
            catch (InvalidOperationException)
            {
                _log.Debug("Cannot access Service List due to permissions. Assuming the service is not installed.");
            }

            return result;
        }
    }
}
