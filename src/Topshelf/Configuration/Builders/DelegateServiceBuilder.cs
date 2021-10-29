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
using Topshelf.Runtime;

namespace Topshelf.Builders
{
    public class DelegateServiceBuilder<T> :
        ServiceBuilder
        where T : class
    {
        private readonly Func<T, HostControl, bool> _continue;
        private readonly Action<T, HostControl, int> _customCommand;
        private readonly Func<T, HostControl, bool> _pause;
        private readonly Func<T, HostControl, IPowerEventArguments, bool> _powerEvent;
        private readonly IServiceEvents _serviceEvents;
        private readonly ServiceFactory<T> _serviceFactory;
        private readonly Action<T, HostControl, ISessionChangedArguments> _sessionChanged;
        private readonly Action<T, HostControl> _shutdown;
        private readonly Func<T, HostControl, bool> _start;
        private readonly Func<T, HostControl, bool> _stop;

        public DelegateServiceBuilder(ServiceFactory<T> serviceFactory, Func<T, HostControl, bool> start,
            Func<T, HostControl, bool> stop, Func<T, HostControl, bool> pause, Func<T, HostControl, bool> @continue,
            Action<T, HostControl> shutdown, Action<T, HostControl, ISessionChangedArguments> sessionChanged,
            Func<T, HostControl, IPowerEventArguments, bool> powerEvent,
            Action<T, HostControl, int> customCommand, IServiceEvents serviceEvents)
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

        public IServiceHandle Build(HostSettings settings)
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

        private class DelegateServiceHandle :
            IServiceHandle
        {
            private readonly Func<T, HostControl, bool> _continue;
            private readonly Action<T, HostControl, int> _customCommand;
            private readonly Func<T, HostControl, bool> _pause;
            private readonly Func<T, HostControl, IPowerEventArguments, bool> _powerEvent;
            private readonly T _service;
            private readonly IServiceEvents _serviceEvents;
            private readonly Action<T, HostControl, ISessionChangedArguments> _sessionChanged;
            private readonly Action<T, HostControl> _shutdown;
            private readonly Func<T, HostControl, bool> _start;
            private readonly Func<T, HostControl, bool> _stop;

            public DelegateServiceHandle(T service, Func<T, HostControl, bool> start, Func<T, HostControl, bool> stop,
                Func<T, HostControl, bool> pause, Func<T, HostControl, bool> @continue, Action<T, HostControl> shutdown,
                Action<T, HostControl, ISessionChangedArguments> sessionChanged, Func<T, HostControl, IPowerEventArguments, bool> powerEvent,
                Action<T, HostControl, int> customCommand, IServiceEvents serviceEvents)
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

            public bool Continue(HostControl hostControl) => _continue != null && _continue(_service, hostControl);

            public void CustomCommand(HostControl hostControl, int command)
            {
                if (_customCommand != null)
                {
                    _customCommand(_service, hostControl, command);
                }
            }

            public void Dispose()
            {
                var disposable = _service as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }

            public bool Pause(HostControl hostControl) => _pause != null && _pause(_service, hostControl);

            public bool PowerEvent(HostControl hostControl, IPowerEventArguments arguments)
            {
                if (_powerEvent != null)
                {
                    return _powerEvent(_service, hostControl, arguments);
                }

                return false;
            }

            public void SessionChanged(HostControl hostControl, ISessionChangedArguments arguments)
            {
                if (_sessionChanged != null)
                {
                    _sessionChanged(_service, hostControl, arguments);
                }
            }

            public void Shutdown(HostControl hostControl)
            {
                if (_shutdown != null)
                {
                    _shutdown(_service, hostControl);
                }
            }

            public bool Start(HostControl hostControl)
            {
                _serviceEvents.BeforeStart(hostControl);

                var started = _start(_service, hostControl);
                if (started)
                {
                    _serviceEvents.AfterStart(hostControl);
                }
                return started;
            }

            public bool Stop(HostControl hostControl)
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
