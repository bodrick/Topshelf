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
using Microsoft.Extensions.Logging;
using Topshelf.Logging;

namespace Topshelf.Extensions.Logging
{
    /// <summary>
    /// Implements a <see cref="ILogWriterFactory"/> for Microsoft extensions for logging.
    /// </summary>
    /// <seealso cref="ILogWriterFactory" />
    public sealed class LoggingExtensionsLogWriterFactory : ILogWriterFactory
    {
        /// <summary>
        /// The logger factory
        /// </summary>
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingExtensionsLogWriterFactory"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        private LoggingExtensionsLogWriterFactory(ILoggerFactory loggerFactory) => _loggerFactory = loggerFactory;

        /// <summary>
        /// Gets the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>LogWriter.</returns>
        public ILogWriter Get(string name) => new LoggingExtensionsLogWriter(_loggerFactory.CreateLogger(name));

        /// <summary>
        /// Shutdowns this instance.
        /// </summary>
        public void Shutdown()
        {
            // Method intentionally left empty.
        }

        /// <summary>
        /// Implements a Topshelf <see cref="IHostLoggerConfigurator"/> for Microsoft extensions for logging.
        /// </summary>
        /// <seealso cref="IHostLoggerConfigurator" />
        [Serializable]
        public class LoggingExtensionsHostLoggerConfigurator : IHostLoggerConfigurator
        {
            /// <summary>
            /// The logger factory
            /// </summary>
            private readonly ILoggerFactory _loggerFactory;

            /// <summary>
            /// Initializes a new instance of the <see cref="LoggingExtensionsHostLoggerConfigurator"/> class.
            /// </summary>
            /// <param name="loggerFactory">The logger factory.</param>
            public LoggingExtensionsHostLoggerConfigurator(ILoggerFactory loggerFactory) => _loggerFactory = loggerFactory;

            /// <summary>
            /// Creates the log writer factory.
            /// </summary>
            /// <returns>LogWriterFactory.</returns>
            public ILogWriterFactory CreateLogWriterFactory() => new LoggingExtensionsLogWriterFactory(_loggerFactory);
        }
    }
}
