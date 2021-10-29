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
using System.Collections.Generic;
using Topshelf.Logging;
using Topshelf.Runtime;

namespace Topshelf.Hosts
{
    public class UninstallHost : IHost
    {
        private static readonly ILogWriter _log = HostLogger.Get<UninstallHost>();
        private readonly IHostEnvironment _environment;
        private readonly IEnumerable<Action> _postActions;
        private readonly IEnumerable<Action> _preActions;
        private readonly HostSettings _settings;
        private readonly bool _sudo;

        public UninstallHost(IHostEnvironment environment, HostSettings settings, IEnumerable<Action> preActions,
            IEnumerable<Action> postActions,
            bool sudo)
        {
            _environment = environment;
            _settings = settings;
            _preActions = preActions;
            _postActions = postActions;
            _sudo = sudo;
        }

        public TopshelfExitCode Run()
        {
            if (!_environment.IsServiceInstalled(_settings.ServiceName))
            {
                _log.ErrorFormat("The {0} service is not installed.", _settings.ServiceName);
                return TopshelfExitCode.ServiceNotInstalled;
            }

            if (!_environment.IsAdministrator)
            {
                if (_sudo && _environment.RunAsAdministrator())
                {
                    return TopshelfExitCode.Ok;
                }

                _log.ErrorFormat("The {0} service can only be uninstalled as an administrator", _settings.ServiceName);
                return TopshelfExitCode.SudoRequired;
            }

            _log.DebugFormat("Uninstalling {0}", _settings.ServiceName);

            _environment.UninstallService(_settings, ExecutePreActions, ExecutePostActions);

            return TopshelfExitCode.Ok;
        }

        private void ExecutePostActions()
        {
            foreach (var action in _postActions)
            {
                action();
            }
        }

        private void ExecutePreActions()
        {
            foreach (var action in _preActions)
            {
                action();
            }
        }
    }
}
