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
using Topshelf.Configuration.CommandLineParser;
using Topshelf.Configuration.Options;

namespace Topshelf.Configuration.HostConfigurators
{
    internal class CommandLineDefinitionConfigurator : ICommandLineConfigurator
    {
        private readonly Action<string> _callback;
        private readonly string _name;

        public CommandLineDefinitionConfigurator(string name, Action<string> callback)
        {
            _name = name;
            _callback = callback;
        }

        public void Configure(ICommandLineElementParser<IOption> parser) =>
            parser.Add(from s in parser.Definition(_name)
                       select (IOption)new ServiceDefinitionOption(s, _callback));

        private class ServiceDefinitionOption : IOption
        {
            private readonly Action<string> _callback;
            private readonly string _value;

            public ServiceDefinitionOption(IDefinitionElement element, Action<string> callback)
            {
                _callback = callback;
                _value = element.Value;
            }

            public void ApplyTo(IHostConfigurator configurator) => _callback(_value);
        }
    }
}
