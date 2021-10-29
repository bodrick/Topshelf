// Copyright 2007-2014 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
using Topshelf.Configuration.Builders;
using Topshelf.Configuration.Configurators;

namespace Topshelf.Configuration.HostConfigurators
{
    public class CommandConfigurator : IHostBuilderConfigurator
    {
        private readonly int _command;

        public CommandConfigurator(int command) => _command = command;

        public IHostBuilder Configure(IHostBuilder builder) => new CommandBuilder(builder, _command);

        public IEnumerable<IValidateResult> Validate()
        {
            if (_command is < 128 or > 256)
            {
                yield return this.Failure("Command", "must be between 128 and 256");
            }
        }
    }
}
