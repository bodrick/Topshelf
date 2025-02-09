// Copyright 2007-2013 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
using System.Reflection;
using Topshelf.Configuration.Builders;
using Topshelf.Configuration.Configurators;

namespace Topshelf.Configuration.HostConfigurators
{
    public class PrefixHelpTextHostConfigurator : IHostBuilderConfigurator
    {
        public PrefixHelpTextHostConfigurator(Assembly assembly, string resourceName)
        {
            Assembly = assembly;
            ResourceName = resourceName;
            Text = string.Empty;
        }

        public PrefixHelpTextHostConfigurator(string text) => Text = text;

        private Assembly? Assembly { get; }
        private string? ResourceName { get; }
        private string Text { get; set; }

        public IHostBuilder Configure(IHostBuilder builder)
        {
            builder.Match<HelpBuilder>(x => x.SetAdditionalHelpText(Text));
            return builder;
        }

        public IEnumerable<IValidateResult> Validate()
        {
            IValidateResult? loadResult = null;
            if (Assembly != null)
            {
                if (ResourceName == null)
                {
                    yield return this.Failure("A resource name must be specified");
                }

                try
                {
                    var stream = Assembly.GetManifestResourceStream(ResourceName!);
                    if (stream == null)
                    {
                        loadResult = this.Failure("Resource", $"Unable to load resource stream: {ResourceName}");
                    }
                    else
                    {
                        using TextReader reader = new StreamReader(stream);
                        Text = reader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    loadResult = this.Failure($"Failed to load help source: {ex.Message}");
                }

                if (loadResult != null)
                {
                    yield return loadResult;
                }
            }

            if (Text == null)
            {
                yield return this.Failure("No additional help text was specified");
            }
        }
    }
}
