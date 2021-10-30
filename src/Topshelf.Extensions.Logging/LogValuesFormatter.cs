// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Topshelf.Extensions.Logging
{
    /// <summary>
    /// Formatter to convert the named format items like {NamedformatItem} to <see cref="string.Format(IFormatProvider, string, object)"/> format.
    /// </summary>
    internal sealed class LogValuesFormatter
    {
        private const string NullValue = "(null)";
        private static readonly char[] FormatDelimiters = { ',', ':' };
        private readonly string _format;

        // NOTE: If this assembly ever builds for netcoreapp, the below code should change to:
        // - Be annotated as [SkipLocalsInit] to avoid zero'ing the stackalloc'd char span
        // - Format _valueNames.Count directly into a span

        public LogValuesFormatter(string format)
        {
            OriginalFormat = format ?? throw new ArgumentNullException(nameof(format));

            using var vsb = new ValueStringBuilder(stackalloc char[256]);
            var scanIndex = 0;
            var endIndex = format.Length;

            while (scanIndex < endIndex)
            {
                var openBraceIndex = FindBraceIndex(format, '{', scanIndex, endIndex);
                if (scanIndex == 0 && openBraceIndex == endIndex)
                {
                    // No holes found.
                    _format = format;
                    return;
                }

                var closeBraceIndex = FindBraceIndex(format, '}', openBraceIndex, endIndex);

                if (closeBraceIndex == endIndex)
                {
                    vsb.Append(format.AsSpan(scanIndex, endIndex - scanIndex));
                    scanIndex = endIndex;
                }
                else
                {
                    // Format item syntax : { index[,alignment][ :formatString] }.
                    var formatDelimiterIndex = FindIndexOfAny(format, FormatDelimiters, openBraceIndex, closeBraceIndex);

                    vsb.Append(format.AsSpan(scanIndex, openBraceIndex - scanIndex + 1));
                    vsb.Append(ValueNames.Count.ToString());
                    ValueNames.Add(format.Substring(openBraceIndex + 1, formatDelimiterIndex - openBraceIndex - 1));
                    vsb.Append(format.AsSpan(formatDelimiterIndex, closeBraceIndex - formatDelimiterIndex + 1));

                    scanIndex = closeBraceIndex + 1;
                }
            }

            _format = vsb.ToString();
        }

        public string OriginalFormat { get; }
        public List<string> ValueNames { get; } = new();

        public string Format(object?[]? values)
        {
            var formattedValues = values;

            if (values != null)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    var formattedValue = FormatArgument(values[i]);
                    // If the formatted value is changed, we allocate and copy items to a new array to avoid mutating the array passed in to this method
                    if (!ReferenceEquals(formattedValue, values[i]))
                    {
                        formattedValues = new object[values.Length];
                        Array.Copy(values, formattedValues, i);
                        formattedValues[i++] = formattedValue;
                        for (; i < values.Length; i++)
                        {
                            formattedValues[i] = FormatArgument(values[i]);
                        }
                        break;
                    }
                }
            }

            return string.Format(CultureInfo.InvariantCulture, _format, formattedValues ?? Array.Empty<object>());
        }

        public KeyValuePair<string, object?> GetValue(object?[] values, int index)
        {
            if (index < 0 || index > ValueNames.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (ValueNames.Count > index)
            {
                return new KeyValuePair<string, object?>(ValueNames[index], values[index]);
            }

            return new KeyValuePair<string, object?>("{OriginalFormat}", OriginalFormat);
        }

        public IEnumerable<KeyValuePair<string, object?>> GetValues(object[] values)
        {
            var valueArray = new KeyValuePair<string, object?>[values.Length + 1];
            for (var index = 0; index != ValueNames.Count; ++index)
            {
                valueArray[index] = new KeyValuePair<string, object?>(ValueNames[index], values[index]);
            }

            valueArray[^1] = new KeyValuePair<string, object?>("{OriginalFormat}", OriginalFormat);
            return valueArray;
        }

        internal string Format() => _format;

        internal string Format(object? arg0) => string.Format(CultureInfo.InvariantCulture, _format, FormatArgument(arg0));

        internal string Format(object? arg0, object? arg1) => string.Format(CultureInfo.InvariantCulture, _format, FormatArgument(arg0), FormatArgument(arg1));

        internal string Format(object? arg0, object? arg1, object? arg2) => string.Format(CultureInfo.InvariantCulture, _format, FormatArgument(arg0), FormatArgument(arg1), FormatArgument(arg2));

        // NOTE: This method mutates the items in the array if needed to avoid extra allocations, and should only be used when caller expects this to happen
        internal string FormatWithOverwrite(object?[]? values)
        {
            if (values != null)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = FormatArgument(values[i]);
                }
            }

            return string.Format(CultureInfo.InvariantCulture, _format, values ?? Array.Empty<object>());
        }

        private static int FindBraceIndex(string format, char brace, int startIndex, int endIndex)
        {
            // Example: {{prefix{{{Argument}}}suffix}}.
            var braceIndex = endIndex;
            var scanIndex = startIndex;
            var braceOccurrenceCount = 0;

            while (scanIndex < endIndex)
            {
                if (braceOccurrenceCount > 0 && format[scanIndex] != brace)
                {
                    if (braceOccurrenceCount % 2 == 0)
                    {
                        // Even number of '{' or '}' found. Proceed search with next occurrence of '{' or '}'.
                        braceOccurrenceCount = 0;
                        braceIndex = endIndex;
                    }
                    else
                    {
                        // An unescaped '{' or '}' found.
                        break;
                    }
                }
                else if (format[scanIndex] == brace)
                {
                    if (brace == '}')
                    {
                        if (braceOccurrenceCount == 0)
                        {
                            // For '}' pick the first occurrence.
                            braceIndex = scanIndex;
                        }
                    }
                    else
                    {
                        // For '{' pick the last occurrence.
                        braceIndex = scanIndex;
                    }

                    braceOccurrenceCount++;
                }

                scanIndex++;
            }

            return braceIndex;
        }

        private static int FindIndexOfAny(string format, char[] chars, int startIndex, int endIndex)
        {
            var findIndex = format.IndexOfAny(chars, startIndex, endIndex - startIndex);
            return findIndex == -1 ? endIndex : findIndex;
        }

        private static object FormatArgument(object? value)
        {
            if (value == null)
            {
                return NullValue;
            }

            // since 'string' implements IEnumerable, special case it
            if (value is string)
            {
                return value;
            }

            // if the value implements IEnumerable, build a comma separated string.
            if (value is IEnumerable enumerable)
            {
                using var vsb = new ValueStringBuilder(stackalloc char[256]);
                var first = true;
                foreach (var e in enumerable)
                {
                    if (!first)
                    {
                        vsb.Append(", ");
                    }

                    vsb.Append(e != null ? e.ToString() : NullValue);
                    first = false;
                }
                return vsb.ToString();
            }

            return value;
        }
    }
}
