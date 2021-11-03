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
using Topshelf.Configuration.ServiceConfigurators;

namespace Topshelf.Configuration
{
    public static class ServiceConfiguratorExtensions
    {
        public static IServiceConfigurator<T> ConstructUsing<T>(this IServiceConfigurator<T> configurator, Func<T> factory) where T : class
        {
            configurator.ConstructUsing(_ => factory());
            return configurator;
        }

        public static IServiceConfigurator<T> ConstructUsing<T>(this IServiceConfigurator<T> configurator, Func<string, T> factory) where T : class
        {
            configurator.ConstructUsing(_ => factory(typeof(T).Name));
            return configurator;
        }

        public static IServiceConfigurator<T> WhenContinued<T>(this IServiceConfigurator<T> configurator, Action<T> callback) where T : class
        {
            configurator.WhenContinued((service, _) =>
            {
                callback(service);
                return true;
            });

            return configurator;
        }

        public static IServiceConfigurator<T> WhenPaused<T>(this IServiceConfigurator<T> configurator, Action<T> callback) where T : class
        {
            configurator.WhenPaused((service, _) =>
            {
                callback(service);
                return true;
            });

            return configurator;
        }

        public static IServiceConfigurator<T> WhenPowerEvent<T>(this IServiceConfigurator<T> configurator, Func<T, IPowerEventArguments, bool> callback) where T : class
        {
            configurator.WhenPowerEvent((service, _, arguments) => callback(service, arguments));
            return configurator;
        }

        public static IServiceConfigurator<T> WhenSessionChanged<T>(this IServiceConfigurator<T> configurator,
            Action<T, ISessionChangedArguments> callback) where T : class
        {
            configurator.WhenSessionChanged((service, _, arguments) => callback(service, arguments));
            return configurator;
        }

        public static IServiceConfigurator<T> WhenShutdown<T>(this IServiceConfigurator<T> configurator, Action<T> callback) where T : class
        {
            configurator.WhenShutdown((service, _) => callback(service));
            return configurator;
        }

        public static IServiceConfigurator<T> WhenStarted<T>(this IServiceConfigurator<T> configurator, Action<T> callback) where T : class
        {
            configurator.WhenStarted((service, _) =>
            {
                callback(service);
                return true;
            });

            return configurator;
        }

        public static IServiceConfigurator<T> WhenStopped<T>(this IServiceConfigurator<T> configurator, Action<T> callback) where T : class
        {
            configurator.WhenStopped((service, _) =>
            {
                callback(service);
                return true;
            });

            return configurator;
        }
    }
}
