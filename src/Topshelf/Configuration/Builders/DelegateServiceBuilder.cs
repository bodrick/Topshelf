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
using Topshelf.Exceptions;
using Topshelf.Runtime;

namespace Topshelf.Configuration.Builders
{
    public class DelegateServiceBuilder<T> : IServiceBuilder where T : class
    {
        private readonly Func<T, IHostControl, bool> _continue;
        private readonly Action<T, IHostControl, int> _customCommand;
        private readonly Func<T, IHostControl, bool> _pause;
        private readonly Func<T, IHostControl, IPowerEventArguments, bool> _powerEvent;
        private readonly IServiceEvents _serviceEvents;
        private readonly ServiceFactory<T> _serviceFactory;
        private readonly Action<T, IHostControl, ISessionChangedArguments> _sessionChanged;
        private readonly Action<T, IHostControl> _shutdown;
        private readonly Func<T, IHostControl, bool> _start;
        private readonly Func<T, IHostControl, bool> _stop;

        public DelegateServiceBuilder(ServiceFactory<T> serviceFactory, Func<T, IHostControl, bool> start,
            Func<T, IHostControl, bool> stop, Func<T, IHostControl, bool> pause, Func<T, IHostControl, bool> @continue,
            Action<T, IHostControl> shutdown, Action<T, IHostControl, ISessionChangedArguments> sessionChanged,
            Func<T, IHostControl, IPowerEventArguments, bool> powerEvent,
            Action<T, IHostControl, int> customCommand, IServiceEvents serviceEvents)
        {
            _serviceFactory = serviceFactory;
            _start = start;
            _stop = stop;
            _pause = pause;
            _continue = @continue;
            _shutdown = shutdown;
            _sessionChanged = sessionChanged;
            _powerEvent = powerEvent;
            _customCommand = customCommand;
            _serviceEvents = serviceEvents;
        }

        public IServiceHandle Build(IHostSettings settings)
        {
            try
            {
                var service = _serviceFactory(settings);

                return new DelegateServiceHandle(service, _start, _stop, _pause, _continue, _shutdown, _sessionChanged, _powerEvent, _customCommand, _serviceEvents);
            }
            catch (Exception ex)
            {
                throw new ServiceBuilderException("An exception occurred creating the service: " + typeof(T).Name, ex);
            }
        }

        private sealed class DelegateServiceHandle : IServiceHandle
        {
            private readonly Func<T, IHostControl, bool> _continue;
            private readonly Action<T, IHostControl, int> _customCommand;
            private readonly Func<T, IHostControl, bool> _pause;
            private readonly Func<T, IHostControl, IPowerEventArguments, bool> _powerEvent;
            private readonly T _service;
            private readonly IServiceEvents _serviceEvents;
            private readonly Action<T, IHostControl, ISessionChangedArguments> _sessionChanged;
            private readonly Action<T, IHostControl> _shutdown;
            private readonly Func<T, IHostControl, bool> _start;
            private readonly Func<T, IHostControl, bool> _stop;

            public DelegateServiceHandle(T service, Func<T, IHostControl, bool> start, Func<T, IHostControl, bool> stop,
                Func<T, IHostControl, bool> pause, Func<T, IHostControl, bool> @continue, Action<T, IHostControl> shutdown,
                Action<T, IHostControl, ISessionChangedArguments> sessionChanged, Func<T, IHostControl, IPowerEventArguments, bool> powerEvent,
                Action<T, IHostControl, int> customCommand, IServiceEvents serviceEvents)
            {
                _service = service;
                _start = start;
                _stop = stop;
                _pause = pause;
                _continue = @continue;
                _shutdown = shutdown;
                _sessionChanged = sessionChanged;
                _powerEvent = powerEvent;
                _customCommand = customCommand;
                _serviceEvents = serviceEvents;
            }

            public bool Continue(IHostControl hostControl) => _continue != null && _continue(_service, hostControl);

            public void CustomCommand(IHostControl hostControl, int command) => _customCommand?.Invoke(_service, hostControl, command);

            public void Dispose()
            {
                if (_service is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            public bool Pause(IHostControl hostControl) => _pause != null && _pause(_service, hostControl);

            public bool PowerEvent(IHostControl hostControl, IPowerEventArguments arguments)
            {
                if (_powerEvent != null)
                {
                    return _powerEvent(_service, hostControl, arguments);
                }

                return false;
            }

            public void SessionChanged(IHostControl hostControl, ISessionChangedArguments arguments) => _sessionChanged?.Invoke(_service, hostControl, arguments);

            public void Shutdown(IHostControl hostControl) => _shutdown?.Invoke(_service, hostControl);

            public bool Start(IHostControl hostControl)
            {
                _serviceEvents.BeforeStart(hostControl);

                var started = _start(_service, hostControl);
                if (started)
                {
                    _serviceEvents.AfterStart(hostControl);
                }
                return started;
            }

            public bool Stop(IHostControl hostControl)
            {
                _serviceEvents.BeforeStop(hostControl);

                var stopped = _stop(_service, hostControl);
                if (stopped)
                {
                    _serviceEvents.AfterStop(hostControl);
                }
                return stopped;
            }
        }
    }
}
