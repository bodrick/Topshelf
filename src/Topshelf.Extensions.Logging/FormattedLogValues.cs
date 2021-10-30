using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Topshelf.Extensions.Logging
{
    /// <summary>
    /// LogValues to enable formatting options supported by <see cref="string.Format(IFormatProvider, string, object?)"/>.
    /// This also enables using {NamedformatItem} in the format string.
    /// </summary>
    internal readonly struct FormattedLogValues : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private const int MaxCachedFormatters = 1024;
        private const string NullFormat = "[null]";
        private static readonly ConcurrentDictionary<string, LogValuesFormatter> Formatters = new();
        private static int _count;
        private readonly LogValuesFormatter? _formatter;
        private readonly string _originalMessage;
        private readonly object[]? _values;

        public FormattedLogValues(string? format, params object[] values)
        {
            if (values.Length != 0 && format != null)
            {
                if (_count >= MaxCachedFormatters)
                {
                    if (!Formatters.TryGetValue(format, out _formatter))
                    {
                        _formatter = new LogValuesFormatter(format);
                    }
                }
                else
                {
                    _formatter = Formatters.GetOrAdd(format, f =>
                    {
                        Interlocked.Increment(ref _count);
                        return new LogValuesFormatter(f);
                    });
                }
            }
            else
            {
                _formatter = null;
            }

            _originalMessage = format ?? NullFormat;
            _values = values;
        }

        public int Count
        {
            get
            {
                if (_formatter == null)
                {
                    return 1;
                }

                return _formatter.ValueNames.Count + 1;
            }
        }

        // for testing purposes
        internal LogValuesFormatter? Formatter => _formatter;

        public KeyValuePair<string, object?> this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (index == Count - 1)
                {
                    return new KeyValuePair<string, object?>("{OriginalFormat}", _originalMessage);
                }

                return _formatter!.GetValue(_values!, index);
            }
        }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => _formatter == null ? _originalMessage : _formatter.Format(_values);
    }
}
