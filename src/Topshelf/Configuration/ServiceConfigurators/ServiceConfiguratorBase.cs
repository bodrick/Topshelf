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
    public abstract class ServiceConfiguratorBase
    {
        protected readonly ServiceEventsImpl ServiceEvents;

        protected ServiceConfiguratorBase() => ServiceEvents = new ServiceEventsImpl();

        public void AfterStartingService(Action<IHostStartedContext> callback) => ServiceEvents.AddAfterStart(callback);

        public void AfterStoppingService(Action<IHostStoppedContext> callback) => ServiceEvents.AddAfterStop(callback);

        public void BeforeStartingService(Action<IHostStartContext> callback) => ServiceEvents.AddBeforeStart(callback);

        public void BeforeStoppingService(Action<IHostStopContext> callback) => ServiceEvents.AddBeforeStop(callback);
    }
}
