// Copyright 2007-2014 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
using Topshelf.Logging;
using Topshelf.Runtime;

namespace Topshelf.Hosts
{
    public class CommandHost : IHost
    {
        private readonly int _command;
        private readonly IHostEnvironment _environment;
        private readonly ILogWriter _log = HostLogger.Get<StartHost>();
        private readonly IHostSettings _settings;

        public CommandHost(IHostEnvironment environment, IHostSettings settings, int command)
        {
            _environment = environment;
            _settings = settings;
            _command = command;
        }

        public TopshelfExitCode Run()
        {
            if (!_environment.IsServiceInstalled(_settings.ServiceName))
            {
                _log.ErrorFormat("The {0} service is not installed.", _settings.ServiceName);
                return TopshelfExitCode.ServiceNotInstalled;
            }

            if (_environment.IsServiceStopped(_settings.ServiceName))
            {
                _log.ErrorFormat("The {0} service is not running.", _settings.ServiceName);
                return TopshelfExitCode.ServiceNotRunning;
            }

            _log.DebugFormat("Sending command {0} to {1}", _command, _settings.ServiceName);

            try
            {
                _environment.SendServiceCommand(_settings.ServiceName, _command);

                _log.InfoFormat("The command {0} was sent to the {1} service.", _command, _settings.ServiceName);
                return TopshelfExitCode.Ok;
            }
            catch (Exception ex)
            {
                _log.Error("The command could not be sent to the service.", ex);
                return TopshelfExitCode.ServiceControlRequestFailed;
            }
        }
    }
}
