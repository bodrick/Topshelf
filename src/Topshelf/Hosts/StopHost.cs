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
    public class StopHost : IHost
    {
        private static readonly ILogWriter Log = HostLogger.Get<StopHost>();
        private readonly IHostEnvironment _environment;
        private readonly IHostSettings _settings;

        public StopHost(IHostEnvironment environment, IHostSettings settings)
        {
            _environment = environment;
            _settings = settings;
        }

        public TopshelfExitCode Run()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return TopshelfExitCode.NotRunningOnWindows;
            }

            if (!_environment.IsServiceInstalled(_settings.ServiceName))
            {
                var message = $"The {_settings.ServiceName} service is not installed.";
                Log.Error(message);

                return TopshelfExitCode.ServiceNotInstalled;
            }

            if (!_environment.IsAdministrator)
            {
                if (!_environment.RunAsAdministrator())
                {
                    Log.ErrorFormat(CultureInfo.CurrentCulture, "The {0} service can only be stopped by an administrator", _settings.ServiceName);
                }

                return TopshelfExitCode.SudoRequired;
            }

            Log.DebugFormat(CultureInfo.CurrentCulture, "Stopping {0}", _settings.ServiceName);

            try
            {
                _environment.StopService(_settings.ServiceName, _settings.StopTimeOut);

                Log.InfoFormat(CultureInfo.CurrentCulture, "The {0} service was stopped.", _settings.ServiceName);
                return TopshelfExitCode.Ok;
            }
            catch (Exception ex)
            {
                Log.Error("The service failed to stop.", ex);
                return TopshelfExitCode.ServiceControlRequestFailed;
            }
        }
    }
}
