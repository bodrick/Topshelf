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
    public class ControlServiceBuilder<T> :
        ServiceBuilder
        where T : class, ServiceControl
    {
        private readonly ServiceEvents _serviceEvents;
        private readonly Func<HostSettings, T> _serviceFactory;

        public ControlServiceBuilder(Func<HostSettings, T> serviceFactory, ServiceEvents serviceEvents)
        {
            _serviceFactory = serviceFactory;
            _serviceEvents = serviceEvents;
        }

        public ServiceHandle Build(HostSettings settings)
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

        private class ControlServiceHandle :
            ServiceHandle
        {
            private readonly T _service;
            private readonly ServiceEvents _serviceEvents;

            public ControlServiceHandle(T service, ServiceEvents serviceEvents)
            {
                _service = service;
                _serviceEvents = serviceEvents;
            }

            public bool Continue(HostControl hostControl)
            {
                var service = _service as ServiceSuspend;

                return service != null && service.Continue(hostControl);
            }

            public void CustomCommand(HostControl hostControl, int command)
            {
                var customCommand = _service as ServiceCustomCommand;
                if (customCommand != null)
                {
                    customCommand.CustomCommand(hostControl, command);
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

            public bool Pause(HostControl hostControl)
            {
                var service = _service as ServiceSuspend;

                return service != null && service.Pause(hostControl);
            }

            public bool PowerEvent(HostControl hostControl, PowerEventArguments arguments)
            {
                var powerEvent = _service as ServicePowerEvent;
                if (powerEvent != null)
                {
                    return powerEvent.PowerEvent(hostControl, arguments);
                }

                return false;
            }

            public void SessionChanged(HostControl hostControl, SessionChangedArguments arguments)
            {
                var sessionChange = _service as ServiceSessionChange;
                if (sessionChange != null)
                {
                    sessionChange.SessionChange(hostControl, arguments);
                }
            }

            public void Shutdown(HostControl hostControl)
            {
                var serviceShutdown = _service as ServiceShutdown;
                if (serviceShutdown != null)
                {
                    serviceShutdown.Shutdown(hostControl);
                }
            }

            public bool Start(HostControl hostControl)
            {
                _serviceEvents.BeforeStart(hostControl);
                var started = _service.Start(hostControl);
                if (started)
                {
                    _serviceEvents.AfterStart(hostControl);
                }

                return started;
            }

            public bool Stop(HostControl hostControl)
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
