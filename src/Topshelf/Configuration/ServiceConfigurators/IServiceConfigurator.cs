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

namespace Topshelf.Configuration.ServiceConfigurators
{
    public interface IServiceConfigurator
    {
        /// <summary>
        /// Registers a callback invoked after the service Start method is called.
        /// </summary>
        /// <param name="callback"></param>
        void AfterStartingService(Action<IHostStartedContext> callback);

        /// <summary>
        /// Registers a callback invoked after the service Stop method is called.
        /// </summary>
        /// <param name="callback"></param>
        void AfterStoppingService(Action<IHostStoppedContext> callback);

        /// <summary>
        /// Registers a callback invoked before the service Start method is called.
        /// </summary>
        /// <param name="callback"></param>
        void BeforeStartingService(Action<IHostStartContext> callback);

        /// <summary>
        /// Registers a callback invoked before the service Stop method is called.
        /// </summary>
        /// <param name="callback"></param>
        void BeforeStoppingService(Action<IHostStopContext> callback);
    }

    public interface IServiceConfigurator<T> : IServiceConfigurator where T : class
    {
        void ConstructUsing(ServiceFactory<T> serviceFactory);

        void WhenContinued(Func<T, IHostControl, bool> @continue);

        void WhenCustomCommandReceived(Action<T, IHostControl, int> customCommandReceived);

        void WhenPaused(Func<T, IHostControl, bool> pause);

        void WhenPowerEvent(Func<T, IHostControl, IPowerEventArguments, bool> powerEvent);

        void WhenSessionChanged(Action<T, IHostControl, ISessionChangedArguments> sessionChanged);

        void WhenShutdown(Action<T, IHostControl> shutdown);

        void WhenStarted(Func<T, IHostControl, bool> start);

        void WhenStopped(Func<T, IHostControl, bool> stop);
    }
}
