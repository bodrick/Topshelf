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

namespace Topshelf.Runtime
{
    public class ServiceEvents : IServiceEvents
    {
        private readonly EventCallbackList<IHostStartedContext> _afterStart;
        private readonly EventCallbackList<IHostStoppedContext> _afterStop;
        private readonly EventCallbackList<IHostStartContext> _beforeStart;
        private readonly EventCallbackList<IHostStopContext> _beforeStop;

        public ServiceEvents()
        {
            _afterStart = new EventCallbackList<IHostStartedContext>();
            _afterStop = new EventCallbackList<IHostStoppedContext>();
            _beforeStart = new EventCallbackList<IHostStartContext>();
            _beforeStop = new EventCallbackList<IHostStopContext>();
        }

        public void AddAfterStart(Action<IHostStartedContext> callback) => _afterStart.Add(callback);

        public void AddAfterStop(Action<IHostStoppedContext> callback) => _afterStop.Add(callback);

        public void AddBeforeStart(Action<IHostStartContext> callback) => _beforeStart.Add(callback);

        public void AddBeforeStop(Action<IHostStopContext> callback) => _beforeStop.Add(callback);

        public void AfterStart(IHostControl hostControl)
        {
            var context = new HostStartedContext(hostControl);

            _afterStart.Notify(context);
        }

        public void AfterStop(IHostControl hostControl)
        {
            var context = new HostStoppedContext(hostControl);

            _afterStop.Notify(context);
        }

        public void BeforeStart(IHostControl hostControl)
        {
            var context = new HostStartContext(hostControl);

            _beforeStart.Notify(context);
        }

        public void BeforeStop(IHostControl hostControl)
        {
            var context = new HostStopContext(hostControl);

            _beforeStop.Notify(context);
        }

        private abstract class Context
        {
            private readonly IHostControl _hostControl;

            protected Context(IHostControl hostControl) => _hostControl = hostControl;

            public void RequestAdditionalTime(TimeSpan timeRemaining) => _hostControl.RequestAdditionalTime(timeRemaining);

            public void Stop() => _hostControl.Stop();

            public void Stop(TopshelfExitCode exitCode) => _hostControl.Stop(exitCode);
        }

        private class HostStartContext : Context, IHostStartContext
        {
            public HostStartContext(IHostControl hostControl) : base(hostControl)
            {
            }

            public void CancelStart() => throw new ServiceControlException("The start service operation was canceled.");
        }

        private class HostStartedContext : Context, IHostStartedContext
        {
            public HostStartedContext(IHostControl hostControl) : base(hostControl)
            {
            }
        }

        private class HostStopContext : Context, IHostStopContext
        {
            public HostStopContext(IHostControl hostControl) : base(hostControl)
            {
            }
        }

        private class HostStoppedContext : Context, IHostStoppedContext
        {
            public HostStoppedContext(IHostControl hostControl) : base(hostControl)
            {
            }
        }
    }
}
