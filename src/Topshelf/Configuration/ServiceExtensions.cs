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
using Topshelf.Configuration.Builders;
using Topshelf.Configuration.Configurators;
using Topshelf.Configuration.HostConfigurators;
using Topshelf.Configuration.ServiceConfigurators;
using Topshelf.Exceptions;
using Topshelf.Runtime;

namespace Topshelf.Configuration
{
    public static class ServiceExtensions
    {
        public static ServiceBuilderFactory CreateServiceBuilderFactory<TService>(
            Func<IHostSettings, TService> serviceFactory,
            Action<IServiceConfigurator> callback)
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

            IServiceBuilder ServiceBuilderFactory(IHostSettings x)
            {
                var configurationResult = ValidateConfigurationResult.CompileResults(serviceConfigurator.Validate());
                if (configurationResult.Results.Any())
                {
                    throw new HostConfigurationException("The service was not properly configured");
                }

                return serviceConfigurator.Build();
            }

            return ServiceBuilderFactory;
        }

        public static ServiceBuilderFactory CreateServiceBuilderFactory<TService>(Action<IServiceConfigurator<TService>> callback)
            where TService : class
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var serviceConfigurator = new DelegateServiceConfigurator<TService>();

            callback(serviceConfigurator);

            IServiceBuilder ServiceBuilderFactory(IHostSettings x)
            {
                var configurationResult = ValidateConfigurationResult.CompileResults(serviceConfigurator.Validate());
                if (configurationResult.Results.Any())
                {
                    throw new HostConfigurationException("The service was not properly configured");
                }

                return serviceConfigurator.Build();
            }

            return ServiceBuilderFactory;
        }

        public static IHostConfigurator Service<TService>(this IHostConfigurator configurator,
                            Func<IHostSettings, TService> serviceFactory, Action<IServiceConfigurator> callback)
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

        public static IHostConfigurator Service<T>(this IHostConfigurator configurator)
            where T : class, IServiceControl, new() => Service(configurator, x => new T(), x => { });

        public static IHostConfigurator Service<T>(this IHostConfigurator configurator, Func<T> serviceFactory)
            where T : class, IServiceControl => Service(configurator, x => serviceFactory(), x => { });

        public static IHostConfigurator Service<T>(this IHostConfigurator configurator, Func<T> serviceFactory,
            Action<IServiceConfigurator> callback)
            where T : class, IServiceControl => Service(configurator, x => serviceFactory(), callback);

        public static IHostConfigurator Service<T>(this IHostConfigurator configurator,
            Func<IHostSettings, T> serviceFactory)
            where T : class, IServiceControl => Service(configurator, serviceFactory, x => { });

        public static IHostConfigurator Service<TService>(this IHostConfigurator configurator,
            Action<IServiceConfigurator<TService>> callback)
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
