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

namespace Topshelf.ServiceConfigurators
{
    public interface ServiceConfigurator
    {
        /// <summary>
        /// Registers a callback invoked after the service Start method is called.
        /// </summary>
        void AfterStartingService(Action<IHostStartedContext> callback);

        /// <summary>
        /// Registers a callback invoked after the service Stop method is called.
        /// </summary>
        void AfterStoppingService(Action<IHostStoppedContext> callback);

        /// <summary>
        /// Registers a callback invoked before the service Start method is called.
        /// </summary>
        void BeforeStartingService(Action<IHostStartContext> callback);

        /// <summary>
        /// Registers a callback invoked before the service Stop method is called.
        /// </summary>
        void BeforeStoppingService(Action<IHostStopContext> callback);
    }

    public interface ServiceConfigurator<T> :
        ServiceConfigurator
        where T : class
    {
        void ConstructUsing(ServiceFactory<T> serviceFactory);

        void WhenContinued(Func<T, HostControl, bool> @continue);

        void WhenCustomCommandReceived(Action<T, HostControl, int> customCommandReceived);

        void WhenPaused(Func<T, HostControl, bool> pause);

        void WhenPowerEvent(Func<T, HostControl, IPowerEventArguments, bool> powerEvent);

        void WhenSessionChanged(Action<T, HostControl, ISessionChangedArguments> sessionChanged);

        void WhenShutdown(Action<T, HostControl> shutdown);

        void WhenStarted(Func<T, HostControl, bool> start);

        void WhenStopped(Func<T, HostControl, bool> stop);
    }
}
