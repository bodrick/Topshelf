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
using Microsoft.Extensions.Logging;

namespace Topshelf.Logging
{
    /// <summary>
    /// Implements a <see cref="ILogWriterFactory"/> for Microsoft extensions for logging.
    /// </summary>
    /// <seealso cref="Topshelf.Logging.ILogWriterFactory" />
    public partial class LoggingExtensionsLogWriterFactory : ILogWriterFactory
    {
        /// <summary>
        /// The logger factory
        /// </summary>
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingExtensionsLogWriterFactory"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        public LoggingExtensionsLogWriterFactory(ILoggerFactory loggerFactory) => this.loggerFactory = loggerFactory;

        /// <summary>
        /// Gets the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>LogWriter.</returns>
        public ILogWriter Get(string name) => new LoggingExtensionsLogWriter(loggerFactory.CreateLogger(name));

        /// <summary>
        /// Shutdowns this instance.
        /// </summary>
        public void Shutdown()
        {
            // Method intentionally left empty.
        }
    }
}
