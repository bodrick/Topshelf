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
using System.Collections;
using System.Configuration.Install;
using Microsoft.Win32;
using Topshelf.Logging;

namespace Topshelf.Runtime.Windows
{
    public class HostInstaller : Installer
    {
        private static readonly ILogWriter _log = HostLogger.Get<HostInstaller>();
        private readonly string _arguments;
        private readonly Installer[] _installers;
        private readonly IHostSettings _settings;

        public HostInstaller(IHostSettings settings, string arguments, Installer[] installers)
        {
            _installers = installers;
            _arguments = arguments;
            _settings = settings;
        }

        public override void Install(IDictionary stateSaver)
        {
            Installers.AddRange(_installers);

            if (_log.IsInfoEnabled)
            {
                _log.InfoFormat("Installing {0} service", _settings.DisplayName);
            }

            base.Install(stateSaver);

            if (_log.IsDebugEnabled)
            {
                _log.Debug("Opening Registry");
            }

            using (var system = Registry.LocalMachine.OpenSubKey("System"))
            using (var currentControlSet = system.OpenSubKey("CurrentControlSet"))
            using (var services = currentControlSet.OpenSubKey("Services"))
            using (var service = services.OpenSubKey(_settings.ServiceName, true))
            {
                service.SetValue("Description", _settings.Description);

                var imagePath = (string)service.GetValue("ImagePath");

                _log.DebugFormat("Service path: {0}", imagePath);

                imagePath += _arguments;

                _log.DebugFormat("Image path: {0}", imagePath);

                service.SetValue("ImagePath", imagePath);
            }

            if (_log.IsDebugEnabled)
            {
                _log.Debug("Closing Registry");
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            Installers.AddRange(_installers);
            if (_log.IsInfoEnabled)
            {
                _log.InfoFormat("Uninstalling {0} service", _settings.Name);
            }

            base.Uninstall(savedState);
        }
    }
}
