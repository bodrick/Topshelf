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
using System.ServiceProcess;
using Topshelf.Hosts;
using Topshelf.Runtime;

namespace Topshelf.Configuration.Builders
{
    public class InstallBuilder : IHostBuilder
    {
        private readonly IList<string> _dependencies;
        private readonly IList<Action<IInstallHostSettings>> _postActions;
        private readonly IList<Action<IInstallHostSettings>> _postRollbackActions;
        private readonly IList<Action<IInstallHostSettings>> _preActions;
        private readonly IList<Action<IInstallHostSettings>> _preRollbackActions;
        private Credentials _credentials;
        private HostStartMode _startMode;
        private bool _sudo;

        public InstallBuilder(IHostEnvironment environment, IHostSettings settings)
        {
            _preActions = new List<Action<IInstallHostSettings>>();
            _postActions = new List<Action<IInstallHostSettings>>();
            _preRollbackActions = new List<Action<IInstallHostSettings>>();
            _postRollbackActions = new List<Action<IInstallHostSettings>>();
            _dependencies = new List<string>();
            _startMode = HostStartMode.Automatic;
            _credentials = new Credentials("", "", ServiceAccount.LocalSystem);

            Environment = environment;
            Settings = settings;
        }

        public IHostEnvironment Environment { get; }

        public IHostSettings Settings { get; }

        public void AddDependency(string name) => _dependencies.Add(name);

        public void AfterInstall(Action<IInstallHostSettings> callback) => _postActions.Add(callback);

        public void AfterRollback(Action<IInstallHostSettings> callback) => _postRollbackActions.Add(callback);

        public void BeforeInstall(Action<IInstallHostSettings> callback) => _preActions.Add(callback);

        public void BeforeRollback(Action<IInstallHostSettings> callback) => _preRollbackActions.Add(callback);

        public IHost Build(IServiceBuilder serviceBuilder) => new InstallHost(Environment, Settings, _startMode, _dependencies.ToArray(), _credentials,
                                                        _preActions, _postActions, _preRollbackActions, _postRollbackActions, _sudo);

        public void Match<T>(Action<T> callback) where T : class, IHostBuilder
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (this is T self)
            {
                callback(self);
            }
        }

        public void RunAs(string username, string password, ServiceAccount accountType) => _credentials = new Credentials(username, password, accountType);

        public void SetStartMode(HostStartMode startMode) => _startMode = startMode;

        public void Sudo() => _sudo = true;
    }
}
