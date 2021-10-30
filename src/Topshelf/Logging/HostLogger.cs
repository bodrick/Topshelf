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

namespace Topshelf.Logging
{
    public static class HostLogger
    {
        private static readonly object Locker = new();
        private static IHostLoggerConfigurator? _configurator;
        private static ILogWriterFactory? _logWriterFactory;

        public static ILogWriterFactory Current
        {
            get
            {
                lock (Locker)
                {
                    return _logWriterFactory ?? CreateLogWriterFactory();
                }
            }
        }

        public static IHostLoggerConfigurator CurrentHostLoggerConfigurator => _configurator ??= new TraceHostLoggerConfigurator();

        public static ILogWriter Get<T>() where T : class => Get(typeof(T).GetTypeName());

        public static ILogWriter Get(Type type) => Get(type.GetTypeName());

        public static ILogWriter Get(string name) => Current.Get(name);

        public static void Shutdown()
        {
            lock (Locker)
            {
                if (_logWriterFactory != null)
                {
                    _logWriterFactory.Shutdown();
                    _logWriterFactory = null;
                }
            }
        }

        public static void UseLogger(IHostLoggerConfigurator configurator)
        {
            lock (Locker)
            {
                _configurator = configurator;

                var logger = _configurator.CreateLogWriterFactory();

                _logWriterFactory?.Shutdown();
                _logWriterFactory = null;
                _logWriterFactory = logger;
            }
        }

        private static ILogWriterFactory CreateLogWriterFactory()
        {
            _logWriterFactory = CurrentHostLoggerConfigurator.CreateLogWriterFactory();

            return _logWriterFactory;
        }
    }
}
