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
using System.Diagnostics;
using System.Globalization;

namespace Topshelf.Logging
{
    public class TraceLogWriter : ILogWriter
    {
        private readonly LoggingLevel _level;
        private readonly TraceSource _source;

        public TraceLogWriter(TraceSource source)
        {
            _source = source;
            _level = LoggingLevel.FromSourceLevels(source.Switch.Level);
        }

        public bool IsDebugEnabled => _level >= LoggingLevel.Debug;
        public bool IsErrorEnabled => _level >= LoggingLevel.Error;
        public bool IsFatalEnabled => _level >= LoggingLevel.Fatal;
        public bool IsInfoEnabled => _level >= LoggingLevel.Info;
        public bool IsWarnEnabled => _level >= LoggingLevel.Warn;

        /// <summary>
        /// Logs a debug message.
        ///
        /// </summary>
        /// <param name="obj">The message to log</param>
        public void Debug(object obj)
        {
            if (!IsDebugEnabled)
            {
                return;
            }

            Log(LoggingLevel.Debug, obj, null);
        }

        /// <summary>
        /// Logs a debug message.
        ///
        /// </summary>
        /// <param name="obj">The message to log</param>
        /// <param name="exception">The exception to log</param>
        public void Debug(object obj, Exception exception)
        {
            if (!IsDebugEnabled)
            {
                return;
            }

            Log(LoggingLevel.Debug, obj, exception);
        }

        public void Debug(LogWriterOutputProvider messageProvider)
        {
            if (!IsDebugEnabled)
            {
                return;
            }

            var obj = messageProvider();

            LogInternal(LoggingLevel.Debug, obj, null);
        }

        /// <summary>
        /// Logs a debug message.
        ///
        /// </summary>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void DebugFormat(string format, params object[] args)
        {
            if (!IsDebugEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Debug, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        /// <summary>
        /// Logs a debug message.
        ///
        /// </summary>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsDebugEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Debug, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs a debug message.
        ///
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void DebugFormat(Exception exception, string format, params object[] args)
        {
            if (!IsDebugEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Debug, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }

        /// <summary>
        /// Logs a debug message.
        ///
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsDebugEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Debug, string.Format(formatProvider, format, args), exception);
        }

        /// <summary>
        /// Logs an error message.
        ///
        /// </summary>
        /// <param name="obj">The message to log</param>
        public void Error(object obj)
        {
            if (!IsErrorEnabled)
            {
                return;
            }

            Log(LoggingLevel.Error, obj, null);
        }

        /// <summary>
        /// Logs an error message.
        ///
        /// </summary>
        /// <param name="obj">The message to log</param>
        /// <param name="exception">The exception to log</param>
        public void Error(object obj, Exception exception)
        {
            if (!IsErrorEnabled)
            {
                return;
            }

            Log(LoggingLevel.Error, obj, exception);
        }

        public void Error(LogWriterOutputProvider messageProvider)
        {
            if (!IsErrorEnabled)
            {
                return;
            }

            var obj = messageProvider();

            LogInternal(LoggingLevel.Error, obj, null);
        }

        /// <summary>
        /// Logs an error message.
        ///
        /// </summary>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void ErrorFormat(string format, params object[] args)
        {
            if (!IsErrorEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Error, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        /// <summary>
        /// Logs an error message.
        ///
        /// </summary>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsErrorEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Error, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs an error message.
        ///
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void ErrorFormat(Exception exception, string format, params object[] args)
        {
            if (!IsErrorEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Error, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }

        /// <summary>
        /// Logs an error message.
        ///
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsErrorEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Error, string.Format(formatProvider, format, args), exception);
        }

        /// <summary>
        /// Logs a fatal message.
        ///
        /// </summary>
        /// <param name="obj">The message to log</param>
        public void Fatal(object obj)
        {
            if (!IsFatalEnabled)
            {
                return;
            }

            Log(LoggingLevel.Fatal, obj, null);
        }

        /// <summary>
        /// Logs a fatal message.
        ///
        /// </summary>
        /// <param name="obj">The message to log</param>
        /// <param name="exception">The exception to log</param>
        public void Fatal(object obj, Exception exception)
        {
            if (!IsFatalEnabled)
            {
                return;
            }

            Log(LoggingLevel.Fatal, obj, exception);
        }

        public void Fatal(LogWriterOutputProvider messageProvider)
        {
            if (!IsFatalEnabled)
            {
                return;
            }

            var obj = messageProvider();

            LogInternal(LoggingLevel.Fatal, obj, null);
        }

        /// <summary>
        /// Logs a fatal message.
        ///
        /// </summary>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void FatalFormat(string format, params object[] args)
        {
            if (!IsFatalEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Fatal, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        /// <summary>
        /// Logs a fatal message.
        ///
        /// </summary>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsFatalEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Fatal, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs a fatal message.
        ///
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void FatalFormat(Exception exception, string format, params object[] args)
        {
            if (!IsFatalEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Fatal, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }

        /// <summary>
        /// Logs a fatal message.
        ///
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsFatalEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Fatal, string.Format(formatProvider, format, args), exception);
        }

        /// <summary>
        /// Logs an info message.
        ///
        /// </summary>
        /// <param name="obj">The message to log</param>
        public void Info(object obj)
        {
            if (!IsInfoEnabled)
            {
                return;
            }

            Log(LoggingLevel.Info, obj, null);
        }

        /// <summary>
        /// Logs an info message.
        ///
        /// </summary>
        /// <param name="obj">The message to log</param>
        /// <param name="exception">The exception to log</param>
        public void Info(object obj, Exception exception)
        {
            if (!IsInfoEnabled)
            {
                return;
            }

            Log(LoggingLevel.Info, obj, exception);
        }

        public void Info(LogWriterOutputProvider messageProvider)
        {
            if (!IsInfoEnabled)
            {
                return;
            }

            var obj = messageProvider();

            LogInternal(LoggingLevel.Info, obj, null);
        }

        /// <summary>
        /// Logs an info message.
        ///
        /// </summary>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void InfoFormat(string format, params object[] args)
        {
            if (!IsInfoEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Info, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        /// <summary>
        /// Logs an info message.
        ///
        /// </summary>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsInfoEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Info, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs an info message.
        ///
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void InfoFormat(Exception exception, string format, params object[] args)
        {
            if (!IsInfoEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Info, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }

        /// <summary>
        /// Logs an info message.
        ///
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsInfoEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Info, string.Format(formatProvider, format, args), exception);
        }

        public void Log(LoggingLevel level, object obj)
        {
            if (_level < level)
            {
                return;
            }

            LogInternal(level, obj, null);
        }

        public void Log(LoggingLevel level, object obj, Exception? exception)
        {
            if (_level < level)
            {
                return;
            }

            LogInternal(level, obj, exception);
        }

        public void Log(LoggingLevel level, LogWriterOutputProvider messageProvider)
        {
            if (_level < level)
            {
                return;
            }

            var obj = messageProvider();

            LogInternal(level, obj, null);
        }

        public void LogFormat(LoggingLevel level, string format, params object[] args)
        {
            if (_level < level)
            {
                return;
            }

            LogInternal(level, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        public void LogFormat(LoggingLevel level, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (_level < level)
            {
                return;
            }

            LogInternal(level, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs a warn message.
        ///
        /// </summary>
        /// <param name="obj">The message to log</param>
        public void Warn(object obj)
        {
            if (!IsWarnEnabled)
            {
                return;
            }

            Log(LoggingLevel.Warn, obj, null);
        }

        /// <summary>
        /// Logs a warn message.
        ///
        /// </summary>
        /// <param name="obj">The message to log</param>
        /// <param name="exception">The exception to log</param>
        public void Warn(object obj, Exception exception)
        {
            if (!IsWarnEnabled)
            {
                return;
            }

            Log(LoggingLevel.Warn, obj, exception);
        }

        public void Warn(LogWriterOutputProvider messageProvider)
        {
            if (!IsWarnEnabled)
            {
                return;
            }

            var obj = messageProvider();

            LogInternal(LoggingLevel.Warn, obj, null);
        }

        /// <summary>
        /// Logs a warn message.
        ///
        /// </summary>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void WarnFormat(string format, params object[] args)
        {
            if (!IsWarnEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Warn, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        /// <summary>
        /// Logs a warn message.
        ///
        /// </summary>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsWarnEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Warn, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs a warn message.
        ///
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void WarnFormat(Exception exception, string format, params object[] args)
        {
            if (!IsWarnEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Warn, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }

        /// <summary>
        /// Logs a warn message.
        ///
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsWarnEnabled)
            {
                return;
            }

            LogInternal(LoggingLevel.Warn, string.Format(formatProvider, format, args), exception);
        }

        private void LogInternal(LoggingLevel level, object obj, Exception? exception)
        {
            var message = obj == null
                                 ? string.Empty
                                 : obj.ToString();

            if (exception == null)
            {
                _source.TraceEvent(level.TraceEventType, 0, message);
            }
            else
            {
                _source.TraceData(level.TraceEventType, 0, message, exception);
            }
        }
    }
}
