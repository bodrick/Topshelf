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
using Topshelf.Hosts;
using Topshelf.Runtime;

namespace Topshelf.Builders
{
    public class UninstallBuilder :
        HostBuilder
    {
        private readonly HostEnvironment _environment;
        private readonly IList<Action> _postActions;
        private readonly IList<Action> _preActions;
        private readonly HostSettings _settings;
        private bool _sudo;

        public UninstallBuilder(HostEnvironment environment, HostSettings settings)
        {
            _preActions = new List<Action>();
            _postActions = new List<Action>();

            _environment = environment;
            _settings = settings;
        }

        public HostEnvironment Environment => _environment;

        public HostSettings Settings => _settings;

        public void AfterUninstall(Action callback) => _postActions.Add(callback);

        public void BeforeUninstall(Action callback) => _preActions.Add(callback);

        public Host Build(ServiceBuilder serviceBuilder) => new UninstallHost(_environment, _settings, _preActions, _postActions, _sudo);

        public void Match<T>(Action<T> callback)
            where T : class, HostBuilder
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            var self = this as T;
            if (self != null)
            {
                callback(self);
            }
        }

        public void Sudo() => _sudo = true;
    }
}
