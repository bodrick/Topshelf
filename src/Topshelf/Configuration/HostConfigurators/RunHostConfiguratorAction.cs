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
using Topshelf.Configuration.Builders;
using Topshelf.Configuration.Configurators;

namespace Topshelf.Configuration.HostConfigurators
{
    public class RunHostConfiguratorAction : IHostBuilderConfigurator
    {
        private readonly Action<RunBuilder> _callback;
        private readonly string _key;

        public RunHostConfiguratorAction(string key, Action<RunBuilder> callback)
        {
            _key = key;
            _callback = callback;
        }

        public IHostBuilder Configure(IHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Match<RunBuilder>(x => _callback(x));

            return builder;
        }

        public IEnumerable<IValidateResult> Validate()
        {
            if (_callback == null)
            {
                yield return this.Failure(_key, "must not be null");
            }
        }
    }
}
