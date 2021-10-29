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
using NLog;

namespace Topshelf.Logging
{
    public class NLogLogWriterFactory : ILogWriterFactory
    {
        private readonly LogFactory _logFactory;

        public NLogLogWriterFactory(LogFactory logFactory) => _logFactory = logFactory;

        public NLogLogWriterFactory() : this(new LogFactory())
        {
        }

        public static void Use() => HostLogger.UseLogger(new NLogHostLoggerConfigurator());

        public static void Use(LogFactory factory) => HostLogger.UseLogger(new NLogHostLoggerConfigurator(factory));

        public ILogWriter Get(string name) => new NLogLogWriter(_logFactory.GetLogger(name), name);

        public void Shutdown()
        {
            _logFactory.Flush();
            _logFactory.SuspendLogging();

            LogManager.Shutdown();
        }

        [Serializable]
        public class NLogHostLoggerConfigurator : IHostLoggerConfigurator
        {
            private readonly LogFactory? _factory;

            public NLogHostLoggerConfigurator(LogFactory factory) => _factory = factory;

            public NLogHostLoggerConfigurator()
            {
            }

            public ILogWriterFactory CreateLogWriterFactory() => _factory != null ? new NLogLogWriterFactory(_factory) : new NLogLogWriterFactory();
        }
    }
}
