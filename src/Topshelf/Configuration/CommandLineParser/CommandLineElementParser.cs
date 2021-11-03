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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Topshelf.Configuration.CommandLineParser
{
    internal class CommandLineElementParser<TResult> : AbstractParser<IEnumerable<ICommandLineElement>>, ICommandLineElementParser<TResult>
    {
        private readonly IList<Parser<IEnumerable<ICommandLineElement>, TResult>> _parsers;

        public CommandLineElementParser()
        {
            _parsers = new List<Parser<IEnumerable<ICommandLineElement>, TResult>>();

            All = from element in _parsers.FirstMatch() select element;
        }

        public static Parser<IEnumerable<ICommandLineElement>, ICommandLineElement> AnyElement => input => input.Any()
            ? new Result<IEnumerable<ICommandLineElement>, ICommandLineElement>(input.First(),
                input.Skip(1))
            : null;

        private Parser<IEnumerable<ICommandLineElement>, TResult> All { get; }

        public void Add(Parser<IEnumerable<ICommandLineElement>, TResult> parser) => _parsers.Add(parser);

        public Parser<IEnumerable<ICommandLineElement>, IArgumentElement> Argument() =>
            AnyElement.Where(c => c is ArgumentElement).Select(c => (IArgumentElement)c);

        public Parser<IEnumerable<ICommandLineElement>, IArgumentElement> Argument(string value) =>
            Argument().Where(arg => string.Equals(arg.Id, value, StringComparison.OrdinalIgnoreCase));

        public Parser<IEnumerable<ICommandLineElement>, IArgumentElement> Argument(Predicate<IArgumentElement> predicate) =>
            Argument().Where(arg => predicate(arg));

        public Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Definition() =>
            AnyElement.Where(c => c is DefinitionElement).Select(c => (IDefinitionElement)c);

        public Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Definition(string key) =>
            Definition().Where(def => def.Key == key);

        public Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Definitions(params string[] keys) =>
            Definition().Where(def => keys.Contains(def.Key));

        public IEnumerable<TResult> Parse(IEnumerable<ICommandLineElement> elements)
        {
            var result = All(elements);
            while (result != null)
            {
                yield return result.Value;

                result = All(result.Rest);
            }
        }

        public Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Switch() =>
            AnyElement.Where(c => c is SwitchElement).Select(c => (ISwitchElement)c);

        public Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Switch(string key) =>
            Switch().Where(sw => string.Equals(sw.Key, key, StringComparison.OrdinalIgnoreCase));

        public Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Switches(params string[] keys) =>
            Switch().Where(sw => keys.Contains(sw.Key));

        public Parser<IEnumerable<ICommandLineElement>, IArgumentElement> ValidPath() =>
            AnyElement.Where(c => c is ArgumentElement element && IsValidPath(element.Id)).Select(c => (IArgumentElement)c);

        private static bool IsValidPath(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), path);
            }

            var directoryName = Path.GetDirectoryName(path) ?? path;
            return Directory.Exists(directoryName);
        }
    }
}
