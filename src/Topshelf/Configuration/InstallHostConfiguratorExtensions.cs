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
using Topshelf.Configuration.HostConfigurators;
using Topshelf.Runtime;

namespace Topshelf.Configuration
{
    public static class InstallHostConfiguratorExtensions
    {
        public static IHostConfigurator AfterInstall(this IHostConfigurator configurator, Action callback)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            configurator.AddConfigurator(new InstallHostConfiguratorAction("AfterInstall", x => x.AfterInstall(settings => callback())));

            return configurator;
        }

        public static IHostConfigurator AfterInstall(this IHostConfigurator configurator,
            Action<IInstallHostSettings> callback)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            configurator.AddConfigurator(new InstallHostConfiguratorAction("AfterInstall", x => x.AfterInstall(callback)));

            return configurator;
        }

        public static IHostConfigurator AfterRollback(this IHostConfigurator configurator, Action callback)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            configurator.AddConfigurator(new InstallHostConfiguratorAction("AfterRollback", x => x.AfterRollback(settings => callback())));

            return configurator;
        }

        public static IHostConfigurator AfterRollback(this IHostConfigurator configurator,
            Action<IInstallHostSettings> callback)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            configurator.AddConfigurator(new InstallHostConfiguratorAction("AfterRollback", x => x.AfterRollback(callback)));

            return configurator;
        }

        public static IHostConfigurator BeforeInstall(this IHostConfigurator configurator, Action callback)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            configurator.AddConfigurator(new InstallHostConfiguratorAction("BeforeInstall", x => x.BeforeInstall(settings => callback())));

            return configurator;
        }

        public static IHostConfigurator BeforeInstall(this IHostConfigurator configurator,
            Action<IInstallHostSettings> callback)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            configurator.AddConfigurator(new InstallHostConfiguratorAction("BeforeInstall", x => x.BeforeInstall(callback)));

            return configurator;
        }

        public static IHostConfigurator BeforeRollback(this IHostConfigurator configurator, Action callback)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            configurator.AddConfigurator(new InstallHostConfiguratorAction("BeforeRollback", x => x.BeforeRollback(settings => callback())));

            return configurator;
        }

        public static IHostConfigurator BeforeRollback(this IHostConfigurator configurator,
            Action<IInstallHostSettings> callback)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            configurator.AddConfigurator(new InstallHostConfiguratorAction("BeforeRollback", x => x.BeforeRollback(callback)));

            return configurator;
        }
    }
}
