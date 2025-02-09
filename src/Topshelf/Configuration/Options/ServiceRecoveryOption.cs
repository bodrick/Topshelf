// Copyright 2007-2014 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
using Topshelf.Configuration.HostConfigurators;
using Topshelf.Runtime.Windows;

namespace Topshelf.Configuration.Options
{
    /// <summary>
    /// Represents an option to set a service recovery options.
    /// </summary>
    /// <seealso cref="IOption" />
    public class ServiceRecoveryOption : IOption
    {
        private readonly ServiceRecoveryOptions _serviceRecoveryOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRecoveryOption"/> class.
        /// </summary>
        /// <param name="serviceRecoveryOptions">The service recovery options.</param>
        public ServiceRecoveryOption(ServiceRecoveryOptions serviceRecoveryOptions) => _serviceRecoveryOptions = serviceRecoveryOptions;

        /// <summary>
        /// Applies the option to the specified host configurator.
        /// </summary>
        /// <param name="configurator">The host configurator.</param>
        public void ApplyTo(IHostConfigurator configurator)
        {
            var recoveryHostConfigurator = new ServiceRecoveryHostConfigurator();

            foreach (var option in _serviceRecoveryOptions.Actions)
            {
                switch (option)
                {
                    case RestartServiceRecoveryAction restartServiceRecoveryAction:
                        recoveryHostConfigurator.RestartService(restartServiceRecoveryAction.Delay / 60000);
                        break;

                    case RestartSystemRecoveryAction restartSystemRecoveryAction:
                        recoveryHostConfigurator.RestartComputer(restartSystemRecoveryAction.Delay / 60000, restartSystemRecoveryAction.RestartMessage);
                        break;

                    case RunProgramRecoveryAction runProgramRecoveryAction:
                        recoveryHostConfigurator.RunProgram(runProgramRecoveryAction.Delay / 60000, runProgramRecoveryAction.Command);
                        break;
                }
            }

            if (_serviceRecoveryOptions.RecoverOnCrashOnly)
            {
                recoveryHostConfigurator.OnCrashOnly();
            }

            recoveryHostConfigurator.SetResetPeriod(_serviceRecoveryOptions.ResetPeriod);

            configurator.AddConfigurator(recoveryHostConfigurator);
        }
    }
}
