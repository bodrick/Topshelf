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
using Topshelf.Logging;

namespace Topshelf.Extensions.Logging
{
    /// <summary>
    /// Implements a Topshelf <see cref="ILogWriter"/> for Microsoft extensions for logging.
    /// </summary>
    /// <seealso cref="ILogWriter" />
    public class LoggingExtensionsLogWriter : ILogWriter
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingExtensionsLogWriter"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public LoggingExtensionsLogWriter(ILogger logger) => _logger = logger;

        /// <summary>
        /// Gets a value indicating whether this instance is debug enabled.
        /// </summary>
        /// <value><see langword="true" /> if this instance is debug enabled; otherwise, <see langword="false" />.</value>
        public bool IsDebugEnabled => _logger.IsEnabled(LogLevel.Debug);

        /// <summary>
        /// Gets a value indicating whether this instance is error enabled.
        /// </summary>
        /// <value><see langword="true" /> if this instance is error enabled; otherwise, <see langword="false" />.</value>
        public bool IsErrorEnabled => _logger.IsEnabled(LogLevel.Error);

        /// <summary>
        /// Gets a value indicating whether this instance is fatal enabled.
        /// </summary>
        /// <value><see langword="true" /> if this instance is fatal enabled; otherwise, <see langword="false" />.</value>
        public bool IsFatalEnabled => _logger.IsEnabled(LogLevel.Critical);

        /// <summary>
        /// Gets a value indicating whether this instance is information enabled.
        /// </summary>
        /// <value><see langword="true" /> if this instance is information enabled; otherwise, <see langword="false" />.</value>
        public bool IsInfoEnabled => _logger.IsEnabled(LogLevel.Information);

        /// <summary>
        /// Gets a value indicating whether this instance is warn enabled.
        /// </summary>
        /// <value><see langword="true" /> if this instance is warn enabled; otherwise, <see langword="false" />.</value>
        public bool IsWarnEnabled => _logger.IsEnabled(LogLevel.Warning);

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
        public void Debug(object obj, Exception? exception)
        {
            if (obj is string text)
            {
                _logger.LogDebug(0, exception, text);
            }
            else
            {
                _logger.LogDebug(0, exception, "{Obj}", obj);
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
                var provider = messageProvider();
                Debug(provider);
            }
        }

        /// <summary>
        /// Debugs the format.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args) => _logger.LogDebug(format, args);

        /// <summary>
        /// Debugs the format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void DebugFormat(string format, params object[] args) => _logger.LogDebug(format, args);

        /// <summary>
        /// Errors the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Error(object obj)
        {
            if (obj is string text)
            {
                _logger.LogError(text);
            }
            else
            {
                _logger.LogError("{Obj}", obj);
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
                _logger.LogError(0, exception, text);
            }
            else
            {
                _logger.LogError(0, exception, "{Obj}", obj);
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
                var provider = messageProvider();
                Error(provider);
            }
        }

        /// <summary>
        /// Errors the format.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args) => _logger.LogError(format, args);

        /// <summary>
        /// Errors the format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void ErrorFormat(string format, params object[] args) => _logger.LogError(format, args);

        /// <summary>
        /// Fatal the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Fatal(object obj)
        {
            if (obj is string text)
            {
                _logger.LogCritical(text);
            }
            else
            {
                _logger.LogCritical("{Obj}", obj);
            }
        }

        /// <summary>
        /// Fatal the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="exception">The exception.</param>
        public void Fatal(object obj, Exception exception)
        {
            if (obj is string text)
            {
                _logger.LogCritical(0, exception, text);
            }
            else
            {
                _logger.LogCritical(0, exception, "{Obj}", obj);
            }
        }

        /// <summary>
        /// Fatal the specified message provider.
        /// </summary>
        /// <param name="messageProvider">The message provider.</param>
        public void Fatal(LogWriterOutputProvider messageProvider)
        {
            if (IsFatalEnabled)
            {
                var provider = messageProvider();
                Fatal(provider);
            }
        }

        /// <summary>
        /// Fatal the format.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args) => _logger.LogCritical(format, args);

        /// <summary>
        /// Fatal the format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void FatalFormat(string format, params object[] args) => _logger.LogCritical(format, args);

        /// <summary>
        /// Information the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Info(object obj) => Info(obj, null);

        /// <summary>
        /// Information the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="exception">The exception.</param>
        public void Info(object obj, Exception? exception)
        {
            if (obj is string text)
            {
                _logger.LogInformation(0, exception, text);
            }
            else
            {
                _logger.LogInformation(0, exception, "{Obj}", obj);
            }
        }

        /// <summary>
        /// Information the specified message provider.
        /// </summary>
        /// <param name="messageProvider">The message provider.</param>
        public void Info(LogWriterOutputProvider messageProvider)
        {
            if (IsInfoEnabled)
            {
                var provider = messageProvider();
                Info(provider);
            }
        }

        /// <summary>
        /// Information the format.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args) => _logger.LogInformation(format, args);

        /// <summary>
        /// Information the format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void InfoFormat(string format, params object[] args) => _logger.LogInformation(format, args);

        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        public void Log(LoggingLevel level, object obj) => _logger.Log(ToLogLevel(level), 0, obj, null, (s, _) => s.ToString());

        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        /// <param name="exception">The exception.</param>
        public void Log(LoggingLevel level, object obj, Exception exception) => _logger.Log(ToLogLevel(level), 0, obj, exception, (s, _) => s.ToString());

        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="messageProvider">The message provider.</param>
        public void Log(LoggingLevel level, LogWriterOutputProvider messageProvider)
        {
            if (_logger.IsEnabled(ToLogLevel(level)))
            {
                var provider = messageProvider();
                Log(level, provider);
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
        /// Warns the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Warn(object obj)
        {
            if (obj is string text)
            {
                _logger.LogWarning(text);
            }
            else
            {
                _logger.LogWarning("{Obj}", obj);
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
                _logger.LogWarning(0, exception, text);
            }
            else
            {
                _logger.LogWarning(0, exception, "{Obj}", obj);
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
                var provider = messageProvider();
                Warn(provider);
            }
        }

        /// <summary>
        /// Warns the format.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args) => _logger.LogWarning(format, args);

        /// <summary>
        /// Warns the format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void WarnFormat(string format, params object[] args) => _logger.LogWarning(format, args);

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

            if (level == LoggingLevel.Error)
            {
                return LogLevel.Error;
            }

            if (level == LoggingLevel.Warn)
            {
                return LogLevel.Warning;
            }

            if (level == LoggingLevel.Info)
            {
                return LogLevel.Information;
            }

            if (level >= LoggingLevel.Debug)
            {
                return LogLevel.Debug;
            }

            return LogLevel.None;
        }
    }
}
