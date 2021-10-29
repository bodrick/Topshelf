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
using System.Collections.Generic;
using Topshelf.Configuration.Builders;
using Topshelf.Configuration.Configurators;
using Topshelf.Runtime;

namespace Topshelf.Configuration.ServiceConfigurators
{
    public class DelegateServiceConfigurator<T> : ServiceConfiguratorBase, IServiceConfigurator<T>, Configurator where T : class
    {
        private Func<T, IHostControl, bool> _continue;
        private Action<T, IHostControl, int> _customCommandReceived;
        private bool _customCommandReceivedConfigured;
        private ServiceFactory<T> _factory;
        private Func<T, IHostControl, bool> _pause;
        private bool _pauseConfigured;
        private Func<T, IHostControl, IPowerEventArguments, bool> _powerEvent;
        private bool _powerEventConfigured;
        private bool _sessionChangeConfigured;
        private Action<T, IHostControl, ISessionChangedArguments> _sessionChanged;
        private Action<T, IHostControl> _shutdown;
        private bool _shutdownConfigured;
        private Func<T, IHostControl, bool> _start;
        private Func<T, IHostControl, bool> _stop;

        public IServiceBuilder Build() => new DelegateServiceBuilder<T>(_factory, _start, _stop, _pause, _continue, _shutdown,
                _sessionChanged, _powerEvent, _customCommandReceived, ServiceEvents);

        public void ConstructUsing(ServiceFactory<T> serviceFactory) => _factory = serviceFactory;

        public IEnumerable<IValidateResult> Validate()
        {
            if (_factory == null)
            {
                yield return this.Failure("Factory", "must not be null");
            }

            if (_start == null)
            {
                yield return this.Failure("Start", "must not be null");
            }

            if (_stop == null)
            {
                yield return this.Failure("Stop", "must not be null");
            }

            if (_pauseConfigured && _pause != null && _continue == null)
            {
                yield return this.Failure("Continue", "must not be null if pause is specified");
            }

            if (_pauseConfigured && _pause == null && _continue != null)
            {
                yield return this.Failure("Pause", "must not be null if continue is specified");
            }

            if (_shutdownConfigured && _shutdown == null)
            {
                yield return this.Failure("Shutdown", "must not be null if shutdown is allowed");
            }

            if (_sessionChangeConfigured && _sessionChanged == null)
            {
                yield return this.Failure("SessionChange", "must not be null if session change is allowed");
            }

            if (_powerEventConfigured && _powerEvent == null)
            {
                yield return this.Failure("PowerEvent", "must not be null if power event reaction is allowed");
            }

            if (_customCommandReceivedConfigured && _customCommandReceived == null)
            {
                yield return this.Failure("CustomCommand", "must not be null if custom command is allowed");
            }
        }

        public void WhenContinued(Func<T, IHostControl, bool> @continue)
        {
            _pauseConfigured = true;
            _continue = @continue;
        }

        public void WhenCustomCommandReceived(Action<T, IHostControl, int> customCommandReceived)
        {
            _customCommandReceivedConfigured = true;
            _customCommandReceived = customCommandReceived;
        }

        public void WhenPaused(Func<T, IHostControl, bool> pause)
        {
            _pauseConfigured = true;
            _pause = pause;
        }

        public void WhenPowerEvent(Func<T, IHostControl, IPowerEventArguments, bool> powerEvent)
        {
            _powerEventConfigured = true;
            _powerEvent = powerEvent;
        }

        public void WhenSessionChanged(Action<T, IHostControl, ISessionChangedArguments> sessionChanged)
        {
            _sessionChangeConfigured = true;
            _sessionChanged = sessionChanged;
        }

        public void WhenShutdown(Action<T, IHostControl> shutdown)
        {
            _shutdownConfigured = true;
            _shutdown = shutdown;
        }

        public void WhenStarted(Func<T, IHostControl, bool> start) => _start = start;

        public void WhenStopped(Func<T, IHostControl, bool> stop) => _stop = stop;
    }
}
