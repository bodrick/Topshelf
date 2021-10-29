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
using Topshelf.Runtime;

namespace Topshelf.Configuration.ServiceConfigurators
{
    public class ControlServiceConfigurator<T> : ServiceConfiguratorBase, IServiceConfigurator, Configurator where T : class, IServiceControl
    {
        private readonly Func<IHostSettings, T> _serviceFactory;

        public ControlServiceConfigurator(Func<IHostSettings, T> serviceFactory) => _serviceFactory = serviceFactory;

        public IServiceBuilder Build() => new ControlServiceBuilder<T>(_serviceFactory, ServiceEvents);

        public IEnumerable<IValidateResult> Validate()
        {
            yield break;
        }
    }
}
