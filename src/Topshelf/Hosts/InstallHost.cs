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
using System.Linq;
using Topshelf.Logging;
using Topshelf.Runtime;

namespace Topshelf.Hosts
{
    public class InstallHost :
        Host
    {
        private static readonly LogWriter _log = HostLogger.Get<InstallHost>();
        private readonly HostEnvironment _environment;
        private readonly InstallHostSettings _installSettings;
        private readonly IEnumerable<Action<InstallHostSettings>> _postActions;
        private readonly IEnumerable<Action<InstallHostSettings>> _postRollbackActions;
        private readonly IEnumerable<Action<InstallHostSettings>> _preActions;
        private readonly IEnumerable<Action<InstallHostSettings>> _preRollbackActions;
        private readonly HostSettings _settings;
        private readonly bool _sudo;

        public InstallHost(HostEnvironment environment, HostSettings settings, HostStartMode startMode,
            IEnumerable<string> dependencies,
            Credentials credentials, IEnumerable<Action<InstallHostSettings>> preActions,
            IEnumerable<Action<InstallHostSettings>> postActions,
            IEnumerable<Action<InstallHostSettings>> preRollbackActions,
            IEnumerable<Action<InstallHostSettings>> postRollbackActions,
            bool sudo)
        {
            _environment = environment;
            _settings = settings;

            _installSettings = new InstallServiceSettingsImpl(settings, credentials, startMode, dependencies.ToArray());

            _preActions = preActions;
            _postActions = postActions;
            _preRollbackActions = preRollbackActions;
            _postRollbackActions = postRollbackActions;
            _sudo = sudo;
        }

        public InstallHostSettings InstallSettings => _installSettings;

        public HostSettings Settings => _settings;

        public TopshelfExitCode Run()
        {
            if (_environment.IsServiceInstalled(_settings.ServiceName))
            {
                _log.ErrorFormat("The {0} service is already installed.", _settings.ServiceName);
                return TopshelfExitCode.ServiceAlreadyInstalled;
            }

            if (!_environment.IsAdministrator)
            {
                if (_sudo)
                {
                    if (_environment.RunAsAdministrator())
                    {
                        return TopshelfExitCode.Ok;
                    }
                }

                _log.ErrorFormat("The {0} service can only be installed as an administrator", _settings.ServiceName);
                return TopshelfExitCode.SudoRequired;
            }

            _log.DebugFormat("Attempting to install '{0}'", _settings.ServiceName);

            _environment.InstallService(_installSettings, ExecutePreActions, ExecutePostActions, ExecutePreRollbackActions, ExecutePostRollbackActions);

            return TopshelfExitCode.Ok;
        }

        private void ExecutePostActions()
        {
            foreach (var action in _postActions)
            {
                action(_installSettings);
            }
        }

        private void ExecutePostRollbackActions()
        {
            foreach (var action in _postRollbackActions)
            {
                action(_installSettings);
            }
        }

        private void ExecutePreActions(InstallHostSettings settings)
        {
            foreach (var action in _preActions)
            {
                action(_installSettings);
            }
        }

        private void ExecutePreRollbackActions()
        {
            foreach (var action in _preRollbackActions)
            {
                action(_installSettings);
            }
        }

        private class InstallServiceSettingsImpl :
            InstallHostSettings
        {
            private readonly string[] _dependencies;
            private readonly HostSettings _settings;
            private readonly HostStartMode _startMode;
            private Credentials _credentials;

            public InstallServiceSettingsImpl(HostSettings settings, Credentials credentials, HostStartMode startMode,
                string[] dependencies)
            {
                _credentials = credentials;
                _settings = settings;
                _startMode = startMode;
                _dependencies = dependencies;
            }

            public bool CanHandleCtrlBreak => _settings.CanHandleCtrlBreak;

            /// <summary>
            /// True if the service handles power change events
            /// </summary>
            public bool CanHandlePowerEvent => _settings.CanHandlePowerEvent;

            public bool CanPauseAndContinue => _settings.CanPauseAndContinue;
            public bool CanSessionChanged => _settings.CanSessionChanged;
            public bool CanShutdown => _settings.CanShutdown;

            public Credentials Credentials
            {
                get => _credentials;
                set => _credentials = value;
            }

            public string[] Dependencies => _dependencies;
            public string Description => _settings.Description;
            public string DisplayName => _settings.DisplayName;
            public Action<Exception> ExceptionCallback => _settings.ExceptionCallback;
            public string InstanceName => _settings.InstanceName;
            public string Name => _settings.Name;
            public string ServiceName => _settings.ServiceName;
            public HostStartMode StartMode => _startMode;

            public TimeSpan StartTimeOut => _settings.StartTimeOut;

            public TimeSpan StopTimeOut => _settings.StopTimeOut;
            public UnhandledExceptionPolicyCode UnhandledExceptionPolicy => _settings.UnhandledExceptionPolicy;
        }
    }
}
