// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
using Serilog;
using Topshelf.Logging;

namespace Topshelf.Serilog
{
    public sealed class SerilogLogWriterFactory : ILogWriterFactory
    {
        private readonly Func<string, ILogger> _loggerFactory;

        private SerilogLogWriterFactory(ILogger logger) => _loggerFactory = name => logger.ForContext("SourceContext", name);

        public static void Use(ILogger logger) => HostLogger.UseLogger(new SerilogHostLoggerConfigurator(logger));

        public ILogWriter Get(string name) => new SerilogLogWriter(_loggerFactory(name));

        public void Shutdown()
        {
            // Method intentionally left empty.
        }

        [Serializable]
        public class SerilogHostLoggerConfigurator : IHostLoggerConfigurator
        {
            private readonly ILogger _logger;

            public SerilogHostLoggerConfigurator(ILogger logger) => _logger = logger;

            public ILogWriterFactory CreateLogWriterFactory() => new SerilogLogWriterFactory(_logger);
        }
    }
}
