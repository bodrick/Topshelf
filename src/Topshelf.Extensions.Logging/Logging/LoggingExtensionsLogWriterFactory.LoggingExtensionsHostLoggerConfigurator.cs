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
using Microsoft.Extensions.Logging;

namespace Topshelf.Logging
{
    /// <summary>
    /// Provides Topshelf extensions for Microsoft extensions for logging.
    /// </summary>
    public partial class LoggingExtensionsLogWriterFactory
    {
        /// <summary>
        /// Implements a Topshelf <see cref="HostLoggerConfigurator"/> for Microsoft extensions for logging.
        /// </summary>
        /// <seealso cref="Topshelf.Logging.HostLoggerConfigurator" />
        [Serializable]
        public class LoggingExtensionsHostLoggerConfigurator : HostLoggerConfigurator
        {
            /// <summary>
            /// The logger factory
            /// </summary>
            private readonly ILoggerFactory loggerFactory;

            /// <summary>
            /// Initializes a new instance of the <see cref="LoggingExtensionsHostLoggerConfigurator"/> class.
            /// </summary>
            /// <param name="loggerFactory">The logger factory.</param>
            public LoggingExtensionsHostLoggerConfigurator(ILoggerFactory loggerFactory) => this.loggerFactory = loggerFactory;

            /// <summary>
            /// Creates the log writer factory.
            /// </summary>
            /// <returns>LogWriterFactory.</returns>
            public LogWriterFactory CreateLogWriterFactory() => new LoggingExtensionsLogWriterFactory(loggerFactory);
        }
    }
}
