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
using Topshelf.Hosts;
using Topshelf.Logging;
using Topshelf.Runtime;

namespace Topshelf.Configuration.Builders
{
    public class RunBuilder : IHostBuilder
    {
        private static readonly ILogWriter Log = HostLogger.Get<RunBuilder>();

        public RunBuilder(IHostEnvironment environment, IHostSettings settings)
        {
            Environment = environment;
            Settings = settings;
        }

        public IHostEnvironment Environment { get; }

        public IHostSettings Settings { get; }

        public IHost Build(IServiceBuilder serviceBuilder)
        {
            var serviceHandle = serviceBuilder.Build(Settings);
            return CreateHost(serviceHandle);
        }

        public void Match<T>(Action<T> callback) where T : class, IHostBuilder
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (this is T self)
            {
                callback(self);
            }
        }

        private IHost CreateHost(IServiceHandle serviceHandle)
        {
            if (Environment.IsRunningAsAService)
            {
                Log.Debug("Running as a service, creating service host.");
                return Environment.CreateServiceHost(Settings, serviceHandle);
            }

            Log.Debug("Running as a console application, creating the console host.");
            return new ConsoleRunHost(Settings, Environment, serviceHandle);
        }
    }
}
