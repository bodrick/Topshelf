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
using System.Linq;

namespace Topshelf.Configuration.CommandLineParser
{
    internal static class ExtensionForCommandLineElementParsers
    {
        public static Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Optional(string key, bool defaultValue) => input =>
        {
            var switchElements = input.ToList();
            var query = switchElements
                .Where(x => x is SwitchElement switchElement &&
                            string.Equals(switchElement.Key, key, System.StringComparison.OrdinalIgnoreCase)).Cast<ISwitchElement>()
                .ToList();

            if (query.Count > 0)
            {
                return new Result<IEnumerable<ICommandLineElement>, ISwitchElement>(query[0], switchElements.Except(query));
            }

            return new Result<IEnumerable<ICommandLineElement>, ISwitchElement>(new SwitchElement(key, defaultValue), switchElements);
        };

        public static Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Optional(string key, string defaultValue) => input =>
        {
            var commandLineElements = input.ToList();
            var query = commandLineElements.Where(x =>
                    x is DefinitionElement definitionElement &&
                    string.Equals(definitionElement.Key, key, System.StringComparison.OrdinalIgnoreCase))
                .Cast<IDefinitionElement>().ToList();

            if (query.Count > 0)
            {
                return new Result<IEnumerable<ICommandLineElement>, IDefinitionElement>(query[0], commandLineElements.Except(query));
            }

            return new Result<IEnumerable<ICommandLineElement>, IDefinitionElement>(new DefinitionElement(key, defaultValue),
                commandLineElements);
        };
    }
}
