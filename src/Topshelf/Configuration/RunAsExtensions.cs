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
using System.ServiceProcess;
using Topshelf.Configuration.HostConfigurators;

namespace Topshelf.Configuration
{
    public static class RunAsExtensions
    {
        public static IHostConfigurator RunAs(this IHostConfigurator configurator, string username, string password)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            var runAsConfigurator = new RunAsUserHostConfigurator(username, password);

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }

        public static IHostConfigurator RunAsLocalService(this IHostConfigurator configurator)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            var runAsConfigurator = new RunAsServiceAccountHostConfigurator(ServiceAccount.LocalService);

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }

        public static IHostConfigurator RunAsLocalSystem(this IHostConfigurator configurator)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            var runAsConfigurator = new RunAsServiceAccountHostConfigurator(ServiceAccount.LocalSystem);

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }

        public static IHostConfigurator RunAsNetworkService(this IHostConfigurator configurator)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            var runAsConfigurator = new RunAsServiceAccountHostConfigurator(ServiceAccount.NetworkService);

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }

        public static IHostConfigurator RunAsPrompt(this IHostConfigurator configurator)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            var runAsConfigurator = new RunAsServiceAccountHostConfigurator(ServiceAccount.User);

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }

        public static IHostConfigurator RunAsVirtualServiceAccount(this IHostConfigurator configurator)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            var runAsConfigurator = new RunAsVirtualAccountHostConfigurator();

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }
    }
}
