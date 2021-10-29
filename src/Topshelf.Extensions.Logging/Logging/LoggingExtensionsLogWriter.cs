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
    /// Implements a Topshelf <see cref="ILogWriter"/> for Microsoft extensions for logging.
    /// </summary>
    /// <seealso cref="Topshelf.Logging.ILogWriter" />
    public class LoggingExtensionsLogWriter : ILogWriter
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingExtensionsLogWriter"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public LoggingExtensionsLogWriter(ILogger logger) => this.logger = logger;

        /// <summary>
        /// Gets a value indicating whether this instance is debug enabled.
        /// </summary>
        /// <value><see langword="true" /> if this instance is debug enabled; otherwise, <see langword="false" />.</value>
        public bool IsDebugEnabled => logger.IsEnabled(LogLevel.Debug);

        /// <summary>
        /// Gets a value indicating whether this instance is information enabled.
        /// </summary>
        /// <value><see langword="true" /> if this instance is information enabled; otherwise, <see langword="false" />.</value>
        public bool IsInfoEnabled => logger.IsEnabled(LogLevel.Information);

        /// <summary>
        /// Gets a value indicating whether this instance is warn enabled.
        /// </summary>
        /// <value><see langword="true" /> if this instance is warn enabled; otherwise, <see langword="false" />.</value>
        public bool IsWarnEnabled => logger.IsEnabled(LogLevel.Warning);

        /// <summary>
        /// Gets a value indicating whether this instance is error enabled.
        /// </summary>
        /// <value><see langword="true" /> if this instance is error enabled; otherwise, <see langword="false" />.</value>
        public bool IsErrorEnabled => logger.IsEnabled(LogLevel.Error);

        /// <summary>
        /// Gets a value indicating whether this instance is fatal enabled.
        /// </summary>
        /// <value><see langword="true" /> if this instance is fatal enabled; otherwise, <see langword="false" />.</value>
        public bool IsFatalEnabled => logger.IsEnabled(LogLevel.Critical);

        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        public void Log(LoggingLevel level, object obj) => logger.Log(ToLogLevel(level), 0, obj, null, (s, e) => s.ToString());

        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        /// <param name="exception">The exception.</param>
        public void Log(LoggingLevel level, object obj, Exception exception) => logger.Log(ToLogLevel(level), 0, obj, exception, (s, e) => s.ToString());

        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="messageProvider">The message provider.</param>
        public void Log(LoggingLevel level, LogWriterOutputProvider messageProvider)
        {
            if (logger.IsEnabled(ToLogLevel(level)))
            {
                System.Diagnostics.Debug.Assert(messageProvider != null, nameof(messageProvider) + " is null.");
#pragma warning disable CC0031 // Check for null before calling a delegate
                var @object = messageProvider();
#pragma warning restore CC0031 // Check for null before calling a delegate

                Log(level, @object);
            }
        }

        /// <summary>
        /// Logs the format.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void LogFormat(LoggingLevel level, IFormatProvider formatProvider, string format, params object[] args) => LogFormat(level, format, args);

        /// <summary>
        /// Logs the format.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void LogFormat(LoggingLevel level, string format, params object[] args) => Log(level, new FormattedLogValues(format, args));

        /// <summary>
        /// Debugs the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Debug(object obj) => Debug(obj, null);

        /// <summary>
        /// Debugs the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="exception">The exception.</param>
        public void Debug(object obj, Exception exception)
        {
            if (obj is string text)
            {
                logger.LogDebug(0, exception, text);
            }
            else
            {
                logger.LogDebug(0, exception, "{obj}", obj);
            }
        }

        /// <summary>
        /// Debugs the specified message provider.
        /// </summary>
        /// <param name="messageProvider">The message provider.</param>
        public void Debug(LogWriterOutputProvider messageProvider)
        {
            if (IsDebugEnabled)
            {
                System.Diagnostics.Debug.Assert(messageProvider != null, nameof(messageProvider) + " is null.");
#pragma warning disable CC0031 // Check for null before calling a delegate
                var @object = messageProvider();
#pragma warning restore CC0031 // Check for null before calling a delegate
                Debug(@object);
            }
        }

        /// <summary>
        /// Debugs the format.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args) => DebugFormat(format, args);

        /// <summary>
        /// Debugs the format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void DebugFormat(string format, params object[] args) => logger.LogDebug(format, args);

        /// <summary>
        /// Informations the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Info(object obj) => Info(obj, null);

        /// <summary>
        /// Informations the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="exception">The exception.</param>
        public void Info(object obj, Exception exception)
        {
            if (obj is string text)
            {
                logger.LogInformation(0, exception, text);
            }
            else
            {
                logger.LogInformation(0, exception, "{obj}", obj);
            }
        }

        /// <summary>
        /// Informations the specified message provider.
        /// </summary>
        /// <param name="messageProvider">The message provider.</param>
        public void Info(LogWriterOutputProvider messageProvider)
        {
            if (IsInfoEnabled)
            {
                System.Diagnostics.Debug.Assert(messageProvider != null, nameof(messageProvider) + " is null.");
#pragma warning disable CC0031 // Check for null before calling a delegate
                var @object = messageProvider();
#pragma warning restore CC0031 // Check for null before calling a delegate
                Info(@object);
            }
        }

        /// <summary>
        /// Informations the format.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args) => InfoFormat(format, args);

        /// <summary>
        /// Informations the format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void InfoFormat(string format, params object[] args) => logger.LogInformation(format, args);

        /// <summary>
        /// Warns the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Warn(object obj)
        {
            if (obj is string text)
            {
                logger.LogWarning(text);
            }
            else
            {
                logger.LogWarning("{obj}", obj);
            }
        }

        /// <summary>
        /// Warns the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="exception">The exception.</param>
        public void Warn(object obj, Exception exception)
        {
            if (obj is string text)
            {
                logger.LogWarning(0, exception, text);
            }
            else
            {
                logger.LogWarning(0, exception, "{obj}", obj);
            }
        }

        /// <summary>
        /// Warns the specified message provider.
        /// </summary>
        /// <param name="messageProvider">The message provider.</param>
        public void Warn(LogWriterOutputProvider messageProvider)
        {
            if (IsWarnEnabled)
            {
                System.Diagnostics.Debug.Assert(messageProvider != null, nameof(messageProvider) + " is null.");
#pragma warning disable CC0031 // Check for null before calling a delegate
                var @object = messageProvider();
#pragma warning restore CC0031 // Check for null before calling a delegate
                Warn(@object);
            }
        }

        /// <summary>
        /// Warns the format.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args) => logger.LogWarning(format, args);

        /// <summary>
        /// Warns the format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void WarnFormat(string format, params object[] args) => logger.LogWarning(format, args);

        /// <summary>
        /// Errors the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Error(object obj)
        {
            if (obj is string text)
            {
                logger.LogError(text);
            }
            else
            {
                logger.LogError("{obj}", obj);
            }
        }

        /// <summary>
        /// Errors the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="exception">The exception.</param>
        public void Error(object obj, Exception exception)
        {
            if (obj is string text)
            {
                logger.LogError(0, exception, text);
            }
            else
            {
                logger.LogError(0, exception, "{obj}", obj);
            }
        }

        /// <summary>
        /// Errors the specified message provider.
        /// </summary>
        /// <param name="messageProvider">The message provider.</param>
        public void Error(LogWriterOutputProvider messageProvider)
        {
            if (IsErrorEnabled)
            {
                System.Diagnostics.Debug.Assert(messageProvider != null, nameof(messageProvider) + " is null.");
#pragma warning disable CC0031 // Check for null before calling a delegate
                var @object = messageProvider();
#pragma warning restore CC0031 // Check for null before calling a delegate
                Error(@object);
            }
        }

        /// <summary>
        /// Errors the format.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args) => ErrorFormat(format, args);

        /// <summary>
        /// Errors the format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void ErrorFormat(string format, params object[] args) => logger.LogError(format, args);

        /// <summary>
        /// Fatals the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Fatal(object obj)
        {
            if (obj is string text)
            {
                logger.LogCritical(text);
            }
            else
            {
                logger.LogCritical("{obj}", obj);
            }
        }

        /// <summary>
        /// Fatals the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="exception">The exception.</param>
        public void Fatal(object obj, Exception exception)
        {
            if (obj is string text)
            {
                logger.LogCritical(0, exception, text);
            }
            else
            {
                logger.LogCritical(0, exception, "{obj}", obj);
            }
        }

        /// <summary>
        /// Fatals the specified message provider.
        /// </summary>
        /// <param name="messageProvider">The message provider.</param>
        public void Fatal(LogWriterOutputProvider messageProvider)
        {
            if (IsFatalEnabled)
            {
                System.Diagnostics.Debug.Assert(messageProvider != null, nameof(messageProvider) + " is null.");
#pragma warning disable CC0031 // Check for null before calling a delegate
                var @object = messageProvider();
#pragma warning restore CC0031 // Check for null before calling a delegate
                Fatal(@object);
            }
        }

        /// <summary>
        /// Fatals the format.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args) => FatalFormat(format, args);

        /// <summary>
        /// Fatals the format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void FatalFormat(string format, params object[] args) => logger.LogCritical(format, args);

        /// <summary>
        /// To the log level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>LogLevel.</returns>
        private static LogLevel ToLogLevel(LoggingLevel level)
        {
            if (level == LoggingLevel.Fatal)
            {
                return LogLevel.Critical;
            }
            else if (level == LoggingLevel.Error)
            {
                return LogLevel.Error;
            }
            else if (level == LoggingLevel.Warn)
            {
                return LogLevel.Warning;
            }
            else if (level == LoggingLevel.Info)
            {
                return LogLevel.Information;
            }
            else if (level >= LoggingLevel.Debug)
            {
                return LogLevel.Debug;
            }
            else
            {
                return LogLevel.None;
            }
        }
    }
}
