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
using System.Threading;
using Topshelf;
using Topshelf.Logging;

namespace SampleTopshelfService
{
    internal class SampleService : IServiceControl
    {
        private static readonly ILogWriter Log = HostLogger.Get<SampleService>();
        private readonly bool _throwOnStart;
        private readonly bool _throwOnStop;
        private readonly bool _throwUnhandled;

        public SampleService(bool throwOnStart, bool throwOnStop, bool throwUnhandled)
        {
            _throwOnStart = throwOnStart;
            _throwOnStop = throwOnStop;
            _throwUnhandled = throwUnhandled;
        }

        public bool Continue(IHostControl hostControl)
        {
            Log.Info("SampleService Continued");
            return true;
        }

        public bool Pause(IHostControl hostControl)
        {
            Log.Info("SampleService Paused");
            return true;
        }

        public bool Start(IHostControl hostControl)
        {
            Log.Info("SampleService Starting...");

            hostControl.RequestAdditionalTime(TimeSpan.FromSeconds(10));

            Thread.Sleep(1000);

            if (_throwOnStart)
            {
                Log.Info("Throwing as requested");
                throw new InvalidOperationException("Throw on Start Requested");
            }

            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep(3000);

                if (_throwUnhandled)
                {
                    throw new InvalidOperationException("Throw Unhandled In Random Thread");
                }

                Log.Info("Requesting stop");

                hostControl.Stop();
            });
            Log.Info("SampleService Started");

            return true;
        }

        public bool Stop(IHostControl hostControl)
        {
            Log.Info("SampleService Stopped");

            if (_throwOnStop)
            {
                throw new InvalidOperationException("Throw on Stop Requested!");
            }

            return true;
        }
    }
}
