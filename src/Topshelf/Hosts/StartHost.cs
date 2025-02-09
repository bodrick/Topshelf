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
using System.Globalization;
using System.Runtime.InteropServices;
using Topshelf.Logging;
using Topshelf.Runtime;

namespace Topshelf.Hosts
{
    public class StartHost : IHost
    {
        private readonly IHostEnvironment _environment;
        private readonly ILogWriter _log = HostLogger.Get<StartHost>();
        private readonly IHost? _parentHost;
        private readonly IHostSettings _settings;

        public StartHost(IHostEnvironment environment, IHostSettings settings, IHost? parentHost = null)
        {
            _environment = environment;
            _settings = settings;
            _parentHost = parentHost;
        }

        public TopshelfExitCode Run()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return TopshelfExitCode.NotRunningOnWindows;
            }

            if (!_environment.IsAdministrator)
            {
                if (!_environment.RunAsAdministrator())
                {
                    _log.ErrorFormat(CultureInfo.CurrentCulture, "The {0} service can only be started by an administrator", _settings.ServiceName);
                }

                return TopshelfExitCode.SudoRequired;
            }

            _parentHost?.Run();

            if (!_environment.IsServiceInstalled(_settings.ServiceName))
            {
                _log.ErrorFormat(CultureInfo.CurrentCulture, "The {0} service is not installed.", _settings.ServiceName);
                return TopshelfExitCode.ServiceNotInstalled;
            }

            _log.DebugFormat(CultureInfo.CurrentCulture, "Starting {0}", _settings.ServiceName);

            try
            {
                _environment.StartService(_settings.ServiceName, _settings.StartTimeOut);

                _log.InfoFormat(CultureInfo.CurrentCulture, "The {0} service was started.", _settings.ServiceName);
                return TopshelfExitCode.Ok;
            }
            catch (Exception ex)
            {
                _log.Error("The service failed to start.", ex);
                return TopshelfExitCode.ServiceControlRequestFailed;
            }
        }
    }
}
