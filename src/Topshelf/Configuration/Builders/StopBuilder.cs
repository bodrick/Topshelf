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
using Topshelf.Hosts;
using Topshelf.Runtime;

namespace Topshelf.Builders
{
    public class StopBuilder :
        HostBuilder
    {
        private readonly IHostEnvironment _environment;
        private readonly HostSettings _settings;

        public StopBuilder(IHostEnvironment environment, HostSettings settings)
        {
            _environment = environment;
            _settings = settings;
        }

        public IHostEnvironment Environment => _environment;

        public HostSettings Settings => _settings;

        public IHost Build(ServiceBuilder serviceBuilder) => new StopHost(_environment, _settings);

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
    }
}
