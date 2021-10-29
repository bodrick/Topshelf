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
using System.IO;

namespace Topshelf.Hosts
{
    /// <summary>
    ///   Displays the Topshelf command line reference
    /// </summary>
    public class HelpHost : IHost
    {
        public HelpHost(string prefixText) => PrefixText = prefixText;

        public string PrefixText { get; }

        public TopshelfExitCode Run()
        {
            if (!string.IsNullOrEmpty(PrefixText))
            {
                Console.WriteLine(PrefixText);
            }

            const string helpText = "Topshelf.HelpText.txt";

            var stream = typeof(HelpHost).Assembly.GetManifestResourceStream(helpText);
            if (stream == null)
            {
                Console.WriteLine("Unable to load help text");
                return TopshelfExitCode.AbnormalExit;
            }

            using TextReader reader = new StreamReader(stream);
            var text = reader.ReadToEnd();
            Console.WriteLine(text);

            return TopshelfExitCode.Ok;
        }
    }
}
