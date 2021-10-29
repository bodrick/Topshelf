// Copyright 2007-2014 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
using Topshelf.Runtime;

namespace Topshelf.Configuration.Builders
{
    public class CommandBuilder :
        IHostBuilder
    {
        private readonly int _command;

        public CommandBuilder(IHostBuilder builder, int command)
        {
            _command = command;
            Settings = builder.Settings;
            Environment = builder.Environment;
        }

        public IHostEnvironment Environment { get; }

        public IHostSettings Settings { get; }

        public IHost Build(IServiceBuilder serviceBuilder) => new CommandHost(Environment, Settings, _command);

        public void Match<T>(Action<T> callback)
            where T : class, IHostBuilder
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var self = this as T;
            if (self != null)
            {
                callback(self);
            }
        }
    }
}
