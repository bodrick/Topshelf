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
using System.Reflection;
using Topshelf.Configuration.HostConfigurators;

namespace Topshelf.Configuration
{
    public static class HelpHostConfiguratorExtensions
    {
        /// <summary>
        /// Specifies a text resource to be loaded and displayed before the built-in system help text is displayed
        /// </summary>
        /// <param name="hostConfigurator"></param>
        /// <param name="assembly">The assembly containing the text resource</param>
        /// <param name="resourceName">The name of the embedded resource</param>
        public static IHostConfigurator LoadHelpTextPrefix(this IHostConfigurator hostConfigurator, Assembly assembly, string resourceName)
        {
            var configurator = new PrefixHelpTextHostConfigurator(assembly, resourceName);
            hostConfigurator.AddConfigurator(configurator);
            return hostConfigurator;
        }

        /// <summary>
        /// Sets additional text to be displayed before the built-in help text is displayed
        /// </summary>
        /// <param name="hostConfigurator"></param>
        /// <param name="text"></param>
        public static IHostConfigurator SetHelpTextPrefix(this IHostConfigurator hostConfigurator, string text)
        {
            var configurator = new PrefixHelpTextHostConfigurator(text);
            hostConfigurator.AddConfigurator(configurator);
            return hostConfigurator;
        }
    }
}
