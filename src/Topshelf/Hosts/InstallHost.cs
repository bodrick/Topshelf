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
using Topshelf.Configuration;
using Topshelf.Logging;
using Topshelf.Runtime;

namespace Topshelf.Hosts
{
    public class InstallHost : IHost
    {
        private static readonly ILogWriter _log = HostLogger.Get<InstallHost>();
        private readonly IHostEnvironment _environment;
        private readonly IEnumerable<Action<IInstallHostSettings>> _postActions;
        private readonly IEnumerable<Action<IInstallHostSettings>> _postRollbackActions;
        private readonly IEnumerable<Action<IInstallHostSettings>> _preActions;
        private readonly IEnumerable<Action<IInstallHostSettings>> _preRollbackActions;
        private readonly bool _sudo;

        public InstallHost(IHostEnvironment environment, IHostSettings settings, HostStartMode startMode,
            IEnumerable<string> dependencies,
            Credentials credentials, IEnumerable<Action<IInstallHostSettings>> preActions,
            IEnumerable<Action<IInstallHostSettings>> postActions,
            IEnumerable<Action<IInstallHostSettings>> preRollbackActions,
            IEnumerable<Action<IInstallHostSettings>> postRollbackActions,
            bool sudo)
        {
            _environment = environment;
            Settings = settings;

            InstallSettings = new InstallServiceSettingsImpl(settings, credentials, startMode, dependencies.ToArray());

            _preActions = preActions;
            _postActions = postActions;
            _preRollbackActions = preRollbackActions;
            _postRollbackActions = postRollbackActions;
            _sudo = sudo;
        }

        public IInstallHostSettings InstallSettings { get; }

        public IHostSettings Settings { get; }

        public TopshelfExitCode Run()
        {
            if (_environment.IsServiceInstalled(Settings.ServiceName))
            {
                _log.ErrorFormat("The {0} service is already installed.", Settings.ServiceName);
                return TopshelfExitCode.ServiceAlreadyInstalled;
            }

            if (!_environment.IsAdministrator)
            {
                if (_sudo && _environment.RunAsAdministrator())
                {
                    return TopshelfExitCode.Ok;
                }

                _log.ErrorFormat("The {0} service can only be installed as an administrator", Settings.ServiceName);
                return TopshelfExitCode.SudoRequired;
            }

            _log.DebugFormat("Attempting to install '{0}'", Settings.ServiceName);

            _environment.InstallService(InstallSettings, ExecutePreActions, ExecutePostActions, ExecutePreRollbackActions, ExecutePostRollbackActions);

            return TopshelfExitCode.Ok;
        }

        private void ExecutePostActions()
        {
            foreach (var action in _postActions)
            {
                action(InstallSettings);
            }
        }

        private void ExecutePostRollbackActions()
        {
            foreach (var action in _postRollbackActions)
            {
                action(InstallSettings);
            }
        }

        private void ExecutePreActions(IInstallHostSettings settings)
        {
            foreach (var action in _preActions)
            {
                action(InstallSettings);
            }
        }

        private void ExecutePreRollbackActions()
        {
            foreach (var action in _preRollbackActions)
            {
                action(InstallSettings);
            }
        }

        private class InstallServiceSettingsImpl : IInstallHostSettings
        {
            private readonly IHostSettings _settings;

            public InstallServiceSettingsImpl(IHostSettings settings, Credentials credentials, HostStartMode startMode, string[] dependencies)
            {
                Credentials = credentials;
                _settings = settings;
                StartMode = startMode;
                Dependencies = dependencies;
            }

            public bool CanHandleCtrlBreak => _settings.CanHandleCtrlBreak;

            /// <summary>
            /// True if the service handles power change events
            /// </summary>
            public bool CanHandlePowerEvent => _settings.CanHandlePowerEvent;

            public bool CanPauseAndContinue => _settings.CanPauseAndContinue;
            public bool CanSessionChanged => _settings.CanSessionChanged;
            public bool CanShutdown => _settings.CanShutdown;

            public Credentials Credentials { get; set; }

            public string[] Dependencies { get; }
            public string Description => _settings.Description;
            public string DisplayName => _settings.DisplayName;
            public Action<Exception> ExceptionCallback => _settings.ExceptionCallback;
            public string InstanceName => _settings.InstanceName;
            public string Name => _settings.Name;
            public string ServiceName => _settings.ServiceName;
            public HostStartMode StartMode { get; }

            public TimeSpan StartTimeOut => _settings.StartTimeOut;

            public TimeSpan StopTimeOut => _settings.StopTimeOut;
            public UnhandledExceptionPolicyCode UnhandledExceptionPolicy => _settings.UnhandledExceptionPolicy;
        }
    }
}
