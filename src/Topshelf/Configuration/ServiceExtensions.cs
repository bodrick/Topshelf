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
using System.Linq;
using Topshelf.Builders;
using Topshelf.Configurators;
using Topshelf.HostConfigurators;
using Topshelf.Runtime;
using Topshelf.ServiceConfigurators;

namespace Topshelf
{
    public static class ServiceExtensions
    {
        public static ServiceBuilderFactory CreateServiceBuilderFactory<TService>(
            Func<HostSettings, TService> serviceFactory,
            Action<ServiceConfigurator> callback)
            where TService : class, IServiceControl
        {
            if (serviceFactory == null)
            {
                throw new ArgumentNullException(nameof(serviceFactory));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var serviceConfigurator = new ControlServiceConfigurator<TService>(serviceFactory);

            callback(serviceConfigurator);

            ServiceBuilder ServiceBuilderFactory(HostSettings x)
            {
                var configurationResult = ValidateConfigurationResult.CompileResults(serviceConfigurator.Validate());
                if (configurationResult.Results.Any())
                {
                    throw new HostConfigurationException("The service was not properly configured");
                }

                var serviceBuilder = serviceConfigurator.Build();

                return serviceBuilder;
            }

            return ServiceBuilderFactory;
        }

        public static ServiceBuilderFactory CreateServiceBuilderFactory<TService>(Action<ServiceConfigurator<TService>> callback)
            where TService : class
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var serviceConfigurator = new DelegateServiceConfigurator<TService>();

            callback(serviceConfigurator);

            ServiceBuilder ServiceBuilderFactory(HostSettings x)
            {
                var configurationResult = ValidateConfigurationResult.CompileResults(serviceConfigurator.Validate());
                if (configurationResult.Results.Any())
                {
                    throw new HostConfigurationException("The service was not properly configured");
                }

                var serviceBuilder = serviceConfigurator.Build();

                return serviceBuilder;
            }

            return ServiceBuilderFactory;
        }

        public static HostConfigurator Service<TService>(this HostConfigurator configurator,
                            Func<HostSettings, TService> serviceFactory, Action<ServiceConfigurator> callback)
            where TService : class, IServiceControl
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            var serviceBuilderFactory = CreateServiceBuilderFactory(serviceFactory, callback);

            configurator.UseServiceBuilder(serviceBuilderFactory);

            return configurator;
        }

        public static HostConfigurator Service<T>(this HostConfigurator configurator)
            where T : class, IServiceControl, new() => Service(configurator, x => new T(), x => { });

        public static HostConfigurator Service<T>(this HostConfigurator configurator, Func<T> serviceFactory)
            where T : class, IServiceControl => Service(configurator, x => serviceFactory(), x => { });

        public static HostConfigurator Service<T>(this HostConfigurator configurator, Func<T> serviceFactory,
            Action<ServiceConfigurator> callback)
            where T : class, IServiceControl => Service(configurator, x => serviceFactory(), callback);

        public static HostConfigurator Service<T>(this HostConfigurator configurator,
            Func<HostSettings, T> serviceFactory)
            where T : class, IServiceControl => Service(configurator, serviceFactory, x => { });

        public static HostConfigurator Service<TService>(this HostConfigurator configurator,
            Action<ServiceConfigurator<TService>> callback)
            where TService : class
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            var serviceBuilderFactory = CreateServiceBuilderFactory(callback);

            configurator.UseServiceBuilder(serviceBuilderFactory);

            return configurator;
        }
    }
}
