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
using Topshelf.Runtime;

namespace Topshelf.Configuration.Builders
{
    public class StartBuilder : IHostBuilder
    {
        private readonly IHostBuilder? _builder;

        public StartBuilder(IHostBuilder builder)
        {
            _builder = GetParentBuilder(builder);
            Settings = builder.Settings;
            Environment = builder.Environment;
        }

        public IHostEnvironment Environment { get; }

        public IHostSettings Settings { get; }

        public IHost Build(IServiceBuilder serviceBuilder)
        {
            if (_builder == null)
            {
                return new StartHost(Environment, Settings);
            }

            var parentHost = _builder.Build(serviceBuilder);
            return new StartHost(Environment, Settings, parentHost);
        }

        public void Match<T>(Action<T> callback) where T : class, IHostBuilder
        {
            if (this is T self)
            {
                callback(self);
            }
        }

        private static IHostBuilder? GetParentBuilder(IHostBuilder builder)
        {
            IHostBuilder? result = null;
            builder.Match<InstallBuilder>(_ => result = builder);
            return result;
        }
    }
}
