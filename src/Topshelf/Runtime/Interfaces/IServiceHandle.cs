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

namespace Topshelf.Runtime
{
    /// <summary>
    /// A handle to a service being hosted by the Host
    /// </summary>
    public interface IServiceHandle : IDisposable
    {
        /// <summary>
        /// Continue the service from a paused state
        /// </summary>
        /// <param name="hostControl"></param>
        /// <returns>True if the service was able to continue, otherwise false</returns>
        bool Continue(IHostControl hostControl);

        /// <summary>
        /// Handle the custom command
        /// </summary>
        /// <param name="hostControl"></param>
        /// <param name="command"></param>
        void CustomCommand(IHostControl hostControl, int command);

        /// <summary>
        /// Pause the service
        /// </summary>
        /// <param name="hostControl"></param>
        /// <returns>True if the service was paused, otherwise false</returns>
        bool Pause(IHostControl hostControl);

        /// <summary>
        /// Handle the power change event
        /// </summary>
        /// <param name="hostControl"></param>
        /// <param name="arguments"></param>
        bool PowerEvent(IHostControl hostControl, IPowerEventArguments arguments);

        /// <summary>
        /// Handle the session change event
        /// </summary>
        /// <param name="hostControl"></param>
        /// <param name="arguments"></param>
        void SessionChanged(IHostControl hostControl, ISessionChangedArguments arguments);

        /// <summary>
        /// Handle the shutdown event
        /// </summary>
        /// <param name="hostControl"></param>
        void Shutdown(IHostControl hostControl);

        /// <summary>
        /// Start the service
        /// </summary>
        /// <param name="hostControl"></param>
        /// <returns>True if the service was started, otherwise false</returns>
        bool Start(IHostControl hostControl);

        /// <summary>
        /// Stop the service
        /// </summary>
        /// <param name="hostControl"></param>
        /// <returns>True if the service was stopped, or false if the service cannot be stopped at this time</returns>
        bool Stop(IHostControl hostControl);
    }
}
