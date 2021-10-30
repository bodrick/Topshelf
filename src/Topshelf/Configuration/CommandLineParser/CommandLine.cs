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

namespace Topshelf.Configuration.CommandLineParser
{
    /// <summary>
    ///   Tools for parsing the command line
    /// </summary>
    internal static class CommandLine
    {
        private static readonly StringCommandLineParser Parser = new();

        /// <summary>
        ///   Gets the command line from the Environment.CommandLine, removing the application name if present
        /// </summary>
        /// <returns> The complete, unparsed command line that was specified when the program was executed </returns>
        public static string GetUnparsedCommandLine()
        {
            var line = Environment.CommandLine;

            var applicationPath = Environment.GetCommandLineArgs()[0];

            if (line == applicationPath)
            {
                return string.Empty;
            }

            if (line[..applicationPath.Length] == applicationPath)
            {
                return line[applicationPath.Length..];
            }

            var quotedApplicationPath = "\"" + applicationPath + "\"";

            if (line[..quotedApplicationPath.Length] == quotedApplicationPath)
            {
                return line[quotedApplicationPath.Length..];
            }

            return line;
        }

        public static IEnumerable<T> Parse<T>(Action<ICommandLineElementParser<T>> initializer) => Parse(initializer, GetUnparsedCommandLine());

        /// <summary>
        ///   Parses the command line and matches any specified patterns
        /// </summary>
        /// <typeparam name="T"> The output type of the parser </typeparam>
        /// <param name="initializer"> Used by the caller to add patterns and object generators </param>
        /// <param name="commandLine"> The command line text </param>
        /// <returns> The elements that were found on the command line </returns>
        public static IEnumerable<T> Parse<T>(Action<ICommandLineElementParser<T>> initializer, string commandLine)
        {
            var elementParser = new CommandLineElementParser<T>();
            initializer(elementParser);
            return elementParser.Parse(Parse(commandLine));
        }

        /// <summary>
        ///   Parses the command line
        /// </summary>
        /// <param name="commandLine"> The command line to parse </param>
        /// <returns> The command line elements that were found </returns>
        private static IEnumerable<ICommandLineElement> Parse(string commandLine)
        {
            var result = Parser.All(commandLine);
            while (result != null)
            {
                yield return result.Value;

                var rest = result.Rest;

                result = Parser.All(rest);
            }
        }
    }
}
