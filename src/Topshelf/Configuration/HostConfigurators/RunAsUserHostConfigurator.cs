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
using System.ServiceProcess;
using Topshelf.Configuration.Builders;
using Topshelf.Configuration.Configurators;

namespace Topshelf.Configuration.HostConfigurators
{
    public class RunAsUserHostConfigurator : IHostBuilderConfigurator
    {
        public RunAsUserHostConfigurator(string username, string password)
        {
            Username = username;
            Password = password;
        }

        private string Password { get; }
        private string Username { get; }

        public IHostBuilder Configure(IHostBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Match<InstallBuilder>(x => x.RunAs(Username, Password, ServiceAccount.User));

            return builder;
        }

        public IEnumerable<IValidateResult> Validate()
        {
            if (string.IsNullOrEmpty(Username))
            {
                yield return this.Failure("Username", "must be specified for a User account type");
            }

            if (Password == null)
            {
                yield return this.Failure("Password", "must be specified for a User account type");
            }
        }
    }
}
