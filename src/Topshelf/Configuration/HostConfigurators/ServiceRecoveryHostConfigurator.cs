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
using System.Linq;
using Topshelf.Configuration.Builders;
using Topshelf.Configuration.Configurators;
using Topshelf.Runtime;
using Topshelf.Runtime.Windows;

namespace Topshelf.Configuration.HostConfigurators
{
    /// <summary>
    /// Implements a service recovery configurator and host builder configurator.
    /// </summary>
    /// <seealso cref="IServiceRecoveryConfigurator" />
    /// <seealso cref="IHostBuilderConfigurator" />
    public class ServiceRecoveryHostConfigurator : IServiceRecoveryConfigurator, IHostBuilderConfigurator
    {
        private ServiceRecoveryOptions _options;
        private IHostSettings _settings;

        private ServiceRecoveryOptions Options => _options ??= new ServiceRecoveryOptions();

        /// <summary>
        /// Configures the host builder.
        /// </summary>
        /// <param name="builder">The host builder.</param>
        /// <returns>The configured host builder.</returns>
        /// <exception cref="ArgumentNullException">builder</exception>
        public IHostBuilder Configure(IHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            _settings = builder.Settings;

            builder.Match<InstallBuilder>(x => x.AfterInstall(ConfigureServiceRecovery));

            return builder;
        }

        /// <summary>
        /// Specifies that the recovery actions should only be taken on a service crash. If the service exits
        /// with a non-zero exit code, it will not be restarted.
        /// </summary>
        public void OnCrashOnly() => Options.RecoverOnCrashOnly = true;

        /// <summary>
        /// Adds a restart computer recovery action with the specified delay.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <param name="message">The message.</param>
        /// <returns>ServiceRecoveryConfigurator.</returns>
        public IServiceRecoveryConfigurator RestartComputer(TimeSpan delay, string message)
        {
            Options.AddAction(new RestartSystemRecoveryAction(delay, message));

            return this;
        }

        /// <summary>
        /// Adds a restart computer recovery action with the specified delay in minutes.
        /// </summary>
        /// <param name="delayInMinutes">The delay in minutes.</param>
        /// <param name="message">The message.</param>
        /// <returns>The service recovery configurator.</returns>
        public IServiceRecoveryConfigurator RestartComputer(int delayInMinutes, string message) => RestartComputer(TimeSpan.FromMinutes(delayInMinutes), message);

        /// <summary>
        /// Adds a restart service recovery action with the specified delay.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <returns>The service recovery configurator.</returns>
        public IServiceRecoveryConfigurator RestartService(TimeSpan delay)
        {
            Options.AddAction(new RestartServiceRecoveryAction(delay));

            return this;
        }

        /// <summary>
        /// Adds a restart service recovery action with the specified delay in minutes.
        /// </summary>
        /// <param name="delayInMinutes">The delay in minutes.</param>
        /// <returns>The service recovery configurator.</returns>
        public IServiceRecoveryConfigurator RestartService(int delayInMinutes) => RestartService(TimeSpan.FromMinutes(delayInMinutes));

        /// <summary>
        /// Adds a run program recovery action with the specified delay.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <param name="command">The command to run.</param>
        /// <returns>The service recovery configurator.</returns>
        public IServiceRecoveryConfigurator RunProgram(TimeSpan delay, string command)
        {
            Options.AddAction(new RunProgramRecoveryAction(delay, command));

            return this;
        }

        /// <summary>
        /// Adds a run program recovery action with the specified delay in minutes.
        /// </summary>
        /// <param name="delayInMinutes">The delay in minutes.</param>
        /// <param name="command">The command.</param>
        /// <returns>The service recovery configurator.</returns>
        public IServiceRecoveryConfigurator RunProgram(int delayInMinutes, string command) => RunProgram(TimeSpan.FromMinutes(delayInMinutes), command);

        /// <summary>
        /// Sets the recovery reset period in days.
        /// </summary>
        /// <param name="days">The reset period in days.</param>
        public IServiceRecoveryConfigurator SetResetPeriod(int days)
        {
            Options.ResetPeriod = days;

            return this;
        }

        /// <summary>
        /// Adds a take no action recovery action.
        /// </summary>
        /// <returns>The service recovery configurator.</returns>
        public IServiceRecoveryConfigurator TakeNoAction()
        {
            Options.AddAction(new TakeNoActionAction());

            return this;
        }

        public IEnumerable<IValidateResult> Validate()
        {
            if (_options == null)
            {
                yield return this.Failure("No service recovery options were specified");
            }
            else if (!_options.Actions.Any())
            {
                yield return this.Failure("No service recovery actions were specified.");
            }
        }

        private void ConfigureServiceRecovery(IInstallHostSettings installSettings)
        {
            var controller = new WindowsServiceRecoveryController();
            controller.SetServiceRecoveryOptions(installSettings, _options);
        }
    }
}
