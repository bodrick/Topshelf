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
using Topshelf.Caching;

namespace Topshelf.Logging
{
    public class TraceLogWriterFactory : ILogWriterFactory
    {
        private readonly TraceSource _defaultSource;
        private readonly ICache<string, TraceLogWriter> _logs;
        private readonly ICache<string, TraceSource> _sources;
        private TraceListener? _listener;

        public TraceLogWriterFactory()
        {
            _logs = new DictionaryCache<string, TraceLogWriter>(CreateTraceLog);
            _sources = new DictionaryCache<string, TraceSource>(CreateTraceSource);
            _defaultSource = new TraceSource("Default", SourceLevels.Information);
            _listener = AddDefaultConsoleTraceListener(_defaultSource);
            _sources.Get("Topshelf");
        }

        public ILogWriter Get(string name) => _logs[name];

        public void Shutdown()
        {
            Trace.Flush();

            if (_listener != null)
            {
                Trace.Listeners.Remove(_listener);
                _listener.Close();
                (_listener as IDisposable)?.Dispose();
                _listener = null;
            }
        }

        private static TraceListener AddDefaultConsoleTraceListener(TraceSource source)
        {
            var listener = new TopshelfConsoleTraceListener
            {
                Name = "Topshelf"
            };

            source.Listeners.Add(listener);
            return listener;
        }

        private static bool IsSourceConfigured(TraceSource source) => source.Listeners.Count != 1
                   || source.Listeners[0] is not DefaultTraceListener
                   || source.Listeners[0].Name != "Default";

        private static string? ShortenName(string name)
        {
            var length = name.LastIndexOf('.');
            return length != -1 ? name[..length] : null;
        }

        private void ConfigureTraceSource(TraceSource source, string name, SourceLevels sourceLevel)
        {
            var defaultSource = _defaultSource;

            for (var parentName = ShortenName(name); !string.IsNullOrEmpty(parentName); parentName = ShortenName(parentName))
            {
                var parentSource = _sources.Get(parentName, key => new TraceSource(key, sourceLevel));
                if (IsSourceConfigured(parentSource))
                {
                    defaultSource = parentSource;
                    break;
                }
            }

            source.Switch = defaultSource.Switch;
            source.Listeners.Clear();
            foreach (TraceListener listener in defaultSource.Listeners)
            {
                source.Listeners.Add(listener);
            }
        }

        private TraceLogWriter CreateTraceLog(string name) => new(_sources[name]);

        private TraceSource CreateTraceSource(string name)
        {
            var logLevel = LoggingLevel.Info;
            var sourceLevel = logLevel.SourceLevel;
            var source = new TraceSource(name, sourceLevel);
            if (IsSourceConfigured(source))
            {
                return source;
            }

            ConfigureTraceSource(source, name, sourceLevel);

            return source;
        }
    }
}
