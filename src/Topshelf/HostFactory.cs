// Copyright 2007-2013 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
using System.Globalization;
using Topshelf.Configuration.Configurators;
using Topshelf.Configuration.HostConfigurators;
using Topshelf.Logging;

namespace Topshelf
{
    /// <summary>
    ///   Configure and run a service host using the HostFactory
    /// </summary>
    public static class HostFactory
    {
        /// <summary>
        ///   Configures a new service host
        /// </summary>
        /// <param name="configureCallback"> Configuration method to call </param>
        /// <returns> A Topshelf service host, ready to run </returns>
        /// <exception cref="ArgumentNullException"><paramref name="configureCallback"/> is <c>null</c>.</exception>
        public static IHost New(Action<IHostConfigurator> configureCallback)
        {
            try
            {
                var configurator = new HostConfigurator();

                var declaringType = configureCallback.Method.DeclaringType;
                if (declaringType != null)
                {
                    var defaultServiceName = declaringType.Namespace;
                    if (!string.IsNullOrEmpty(defaultServiceName))
                    {
                        configurator.SetServiceName(defaultServiceName);
                    }
                }

                configureCallback(configurator);

                configurator.ApplyCommandLine();

                var result = ValidateConfigurationResult.CompileResults(configurator.Validate());

                if (result.Message.Length > 0)
                {
                    HostLogger.Get(typeof(HostFactory)).InfoFormat(CultureInfo.CurrentCulture, "Configuration Result:{0}{1}", Environment.NewLine, result.Message);
                }

                return configurator.CreateHost();
            }
            catch (Exception ex)
            {
                HostLogger.Get(typeof(HostFactory)).Error("An exception occurred creating the host", ex);
                HostLogger.Shutdown();
                throw;
            }
        }

        /// <summary>
        ///   Configures and runs a new service host, handling any exceptions and writing them to the log.
        /// </summary>
        /// <param name="configureCallback"> Configuration method to call </param>
        /// <returns> Returns the exit code of the process that should be returned by your application's main method </returns>
        public static TopshelfExitCode Run(Action<IHostConfigurator> configureCallback)
        {
            try
            {
                return New(configureCallback).Run();
            }
            catch (Exception ex)
            {
                HostLogger.Get(typeof(HostFactory)).Error("The service terminated abnormally", ex);
                HostLogger.Shutdown();
                return TopshelfExitCode.AbnormalExit;
            }
        }
    }
}
