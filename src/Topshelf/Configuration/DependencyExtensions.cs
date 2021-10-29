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
using Topshelf.Configuration.Constants;
using Topshelf.Configuration.HostConfigurators;

namespace Topshelf.Configuration
{
    public static class DependencyExtensions
    {
        public static IHostConfigurator AddDependency(this IHostConfigurator configurator, string name)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var dependencyConfigurator = new DependencyHostConfigurator(name);

            configurator.AddConfigurator(dependencyConfigurator);

            return configurator;
        }

        public static IHostConfigurator DependsOn(this IHostConfigurator configurator, string name) => AddDependency(configurator, name);

        public static IHostConfigurator DependsOnEventLog(this IHostConfigurator configurator) => AddDependency(configurator, KnownServiceNames.EventLog);

        public static IHostConfigurator DependsOnIis(this IHostConfigurator configurator) => AddDependency(configurator, KnownServiceNames.IIS);

        public static IHostConfigurator DependsOnMsmq(this IHostConfigurator configurator) => AddDependency(configurator, KnownServiceNames.Msmq);

        public static IHostConfigurator DependsOnMsSql(this IHostConfigurator configurator) => AddDependency(configurator, KnownServiceNames.SqlServer);
    }
}
