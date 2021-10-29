// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
using System.Globalization;
using Topshelf.Logging;

namespace Topshelf.Log4Net
{
    public class Log4NetLogWriter : ILogWriter
    {
        private readonly log4net.ILog _log;

        public Log4NetLogWriter(log4net.ILog log) => _log = log;

        public bool IsDebugEnabled => _log.IsDebugEnabled;

        public bool IsErrorEnabled => _log.IsErrorEnabled;

        public bool IsFatalEnabled => _log.IsFatalEnabled;

        public bool IsInfoEnabled => _log.IsInfoEnabled;

        public bool IsWarnEnabled => _log.IsWarnEnabled;

        public void Debug(object obj) => _log.Debug(obj);

        public void Debug(object obj, Exception exception) => _log.Debug(obj, exception);

        public void Debug(LogWriterOutputProvider messageProvider)
        {
            if (!IsDebugEnabled)
            {
                return;
            }

            _log.Debug(messageProvider());
        }

        public void DebugFormat(string format, params object[] args) => _log.DebugFormat(format, args);

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args) => _log.DebugFormat(formatProvider, format, args);

        public void Error(object obj) => _log.Error(obj);

        public void Error(object obj, Exception exception) => _log.Error(obj, exception);

        public void Error(LogWriterOutputProvider messageProvider)
        {
            if (!IsErrorEnabled)
            {
                return;
            }

            _log.Error(messageProvider());
        }

        public void ErrorFormat(string format, params object[] args) => _log.ErrorFormat(format, args);

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args) => _log.ErrorFormat(formatProvider, format, args);

        public void Fatal(object obj) => _log.Fatal(obj);

        public void Fatal(object obj, Exception exception) => _log.Fatal(obj, exception);

        public void Fatal(LogWriterOutputProvider messageProvider)
        {
            if (!IsFatalEnabled)
            {
                return;
            }

            _log.Fatal(messageProvider());
        }

        public void FatalFormat(string format, params object[] args) => _log.FatalFormat(format, args);

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args) => _log.FatalFormat(formatProvider, format, args);

        public void Info(object obj) => _log.Info(obj);

        public void Info(object obj, Exception exception) => _log.Info(obj, exception);

        public void Info(LogWriterOutputProvider messageProvider)
        {
            if (!IsInfoEnabled)
            {
                return;
            }

            _log.Info(messageProvider());
        }

        public void InfoFormat(string format, params object[] args) => _log.InfoFormat(format, args);

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args) => _log.InfoFormat(formatProvider, format, args);

        public void Log(LoggingLevel level, object obj)
        {
            if (level == LoggingLevel.Fatal)
            {
                Fatal(obj);
            }
            else if (level == LoggingLevel.Error)
            {
                Error(obj);
            }
            else if (level == LoggingLevel.Warn)
            {
                Warn(obj);
            }
            else if (level == LoggingLevel.Info)
            {
                Info(obj);
            }
            else if (level >= LoggingLevel.Debug)
            {
                Debug(obj);
            }
        }

        public void Log(LoggingLevel level, object obj, Exception exception)
        {
            if (level == LoggingLevel.Fatal)
            {
                Fatal(obj, exception);
            }
            else if (level == LoggingLevel.Error)
            {
                Error(obj, exception);
            }
            else if (level == LoggingLevel.Warn)
            {
                Warn(obj, exception);
            }
            else if (level == LoggingLevel.Info)
            {
                Info(obj, exception);
            }
            else if (level >= LoggingLevel.Debug)
            {
                Debug(obj, exception);
            }
        }

        public void Log(LoggingLevel level, LogWriterOutputProvider messageProvider)
        {
            if (level == LoggingLevel.Fatal)
            {
                Fatal(messageProvider);
            }
            else if (level == LoggingLevel.Error)
            {
                Error(messageProvider);
            }
            else if (level == LoggingLevel.Warn)
            {
                Warn(messageProvider);
            }
            else if (level == LoggingLevel.Info)
            {
                Info(messageProvider);
            }
            else if (level >= LoggingLevel.Debug)
            {
                Debug(messageProvider);
            }
        }

        public void LogFormat(LoggingLevel level, string format, params object[] args)
        {
            if (level == LoggingLevel.Fatal)
            {
                FatalFormat(CultureInfo.InvariantCulture, format, args);
            }
            else if (level == LoggingLevel.Error)
            {
                ErrorFormat(CultureInfo.InvariantCulture, format, args);
            }
            else if (level == LoggingLevel.Warn)
            {
                WarnFormat(CultureInfo.InvariantCulture, format, args);
            }
            else if (level == LoggingLevel.Info)
            {
                InfoFormat(CultureInfo.InvariantCulture, format, args);
            }
            else if (level >= LoggingLevel.Debug)
            {
                DebugFormat(CultureInfo.InvariantCulture, format, args);
            }
        }

        public void LogFormat(LoggingLevel level, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (level == LoggingLevel.Fatal)
            {
                FatalFormat(formatProvider, format, args);
            }
            else if (level == LoggingLevel.Error)
            {
                ErrorFormat(formatProvider, format, args);
            }
            else if (level == LoggingLevel.Warn)
            {
                WarnFormat(formatProvider, format, args);
            }
            else if (level == LoggingLevel.Info)
            {
                InfoFormat(formatProvider, format, args);
            }
            else if (level >= LoggingLevel.Debug)
            {
                DebugFormat(formatProvider, format, args);
            }
        }

        public void Warn(object obj) => _log.Warn(obj);

        public void Warn(object obj, Exception exception) => _log.Warn(obj, exception);

        public void Warn(LogWriterOutputProvider messageProvider)
        {
            if (!IsWarnEnabled)
            {
                return;
            }

            _log.Warn(messageProvider());
        }

        public void WarnFormat(string format, params object[] args) => _log.WarnFormat(format, args);

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args) => _log.WarnFormat(formatProvider, format, args);
    }
}
