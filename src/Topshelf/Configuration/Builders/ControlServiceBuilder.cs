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
    public class ControlServiceBuilder<T> : IServiceBuilder where T : class, IServiceControl
    {
        private readonly IServiceEvents _serviceEvents;
        private readonly Func<IHostSettings, T> _serviceFactory;

        public ControlServiceBuilder(Func<IHostSettings, T> serviceFactory, IServiceEvents serviceEvents)
        {
            _serviceFactory = serviceFactory;
            _serviceEvents = serviceEvents;
        }

        public IServiceHandle Build(IHostSettings settings)
        {
            try
            {
                var service = _serviceFactory(settings);

                return new ControlServiceHandle(service, _serviceEvents);
            }
            catch (Exception ex)
            {
                throw new ServiceBuilderException("An exception occurred creating the service: " + typeof(T).Name, ex);
            }
        }

        private class ControlServiceHandle : IServiceHandle
        {
            private readonly T _service;
            private readonly IServiceEvents _serviceEvents;

            public ControlServiceHandle(T service, IServiceEvents serviceEvents)
            {
                _service = service;
                _serviceEvents = serviceEvents;
            }

            public bool Continue(IHostControl hostControl) => _service is IServiceSuspend service && service.Continue(hostControl);

            public void CustomCommand(IHostControl hostControl, int command)
            {
                if (_service is IServiceCustomCommand customCommand)
                {
                    customCommand.CustomCommand(hostControl, command);
                }
            }

            public void Dispose()
            {
                if (_service is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            public bool Pause(IHostControl hostControl) => _service is IServiceSuspend service && service.Pause(hostControl);

            public bool PowerEvent(IHostControl hostControl, IPowerEventArguments arguments)
            {
                if (_service is IServicePowerEvent powerEvent)
                {
                    return powerEvent.PowerEvent(hostControl, arguments);
                }

                return false;
            }

            public void SessionChanged(IHostControl hostControl, ISessionChangedArguments arguments)
            {
                if (_service is IServiceSessionChange sessionChange)
                {
                    sessionChange.SessionChange(hostControl, arguments);
                }
            }

            public void Shutdown(IHostControl hostControl)
            {
                if (_service is IServiceShutdown serviceShutdown)
                {
                    serviceShutdown.Shutdown(hostControl);
                }
            }

            public bool Start(IHostControl hostControl)
            {
                _serviceEvents.BeforeStart(hostControl);
                var started = _service.Start(hostControl);
                if (started)
                {
                    _serviceEvents.AfterStart(hostControl);
                }

                return started;
            }

            public bool Stop(IHostControl hostControl)
            {
                _serviceEvents.BeforeStop(hostControl);
                var stopped = _service.Stop(hostControl);
                if (stopped)
                {
                    _serviceEvents.AfterStop(hostControl);
                }

                return stopped;
            }
        }
    }
}
