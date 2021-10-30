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
using System.Collections.Generic;
using System.Diagnostics;

namespace Topshelf.Logging
{
    public sealed class LoggingLevel
    {
        public static readonly LoggingLevel All = new("All", 6, SourceLevels.All, TraceEventType.Verbose);
        public static readonly LoggingLevel Debug = new("Debug", 5, SourceLevels.Verbose, TraceEventType.Verbose);
        public static readonly LoggingLevel Error = new("Error", 2, SourceLevels.Error, TraceEventType.Error);
        public static readonly LoggingLevel Fatal = new("Fatal", 1, SourceLevels.Critical, TraceEventType.Critical);
        public static readonly LoggingLevel Info = new("Info", 4, SourceLevels.Information, TraceEventType.Information);
        public static readonly LoggingLevel None = new("None", 0, SourceLevels.Off, TraceEventType.Critical);
        public static readonly LoggingLevel Warn = new("Warn", 3, SourceLevels.Warning, TraceEventType.Warning);
        private readonly int _index;

        private LoggingLevel(string name, int index, SourceLevels sourceLevel, TraceEventType traceEventType)
        {
            Name = name;
            _index = index;
            SourceLevel = sourceLevel;
            TraceEventType = traceEventType;
        }

        public static IEnumerable<LoggingLevel> Values
        {
            get
            {
                yield return All;
                yield return Debug;
                yield return Info;
                yield return Warn;
                yield return Error;
                yield return Fatal;
                yield return None;
            }
        }

        public string Name { get; }
        public SourceLevels SourceLevel { get; }
        public TraceEventType TraceEventType { get; }

        public static LoggingLevel FromSourceLevels(SourceLevels level) => level switch
        {
            SourceLevels.Information => Info,
            SourceLevels.Verbose => Debug,
            ~SourceLevels.Off => Debug,
            SourceLevels.Critical => Fatal,
            SourceLevels.Error => Error,
            SourceLevels.Warning => Warn,
            _ => None,
        };

        public static bool operator <(LoggingLevel left, LoggingLevel right) => right != null && left != null && left._index < right._index;

        public static bool operator <=(LoggingLevel left, LoggingLevel right) => right != null && left != null && left._index <= right._index;

        public static bool operator >(LoggingLevel left, LoggingLevel right) => right != null && left != null && left._index > right._index;

        public static bool operator >=(LoggingLevel left, LoggingLevel right) => right != null && left != null && left._index >= right._index;

        public override string ToString() => Name;
    }
}
