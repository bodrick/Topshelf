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
using Topshelf.Configuration.Builders;
using Topshelf.Configuration.Configurators;

namespace Topshelf.Configuration.HostConfigurators
{
    /// <summary>
    /// Can configure/replace the input <see cref="IHostBuilder"/>, returning the original
    /// or a new <see cref="IHostBuilder"/>.
    /// </summary>
    public interface IHostBuilderConfigurator : IConfigurator
    {
        /// <summary>
        /// Configures the host builder.
        /// </summary>
        /// <param name="builder">The host builder.</param>
        /// <returns>The configured host builder.</returns>
        IHostBuilder Configure(IHostBuilder builder);
    }
}
