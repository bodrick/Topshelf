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
using Topshelf.Configuration.HostConfigurators;

namespace Topshelf.Configuration.Options
{
    /// <summary>
    /// Represents an option to set a service dependency.
    /// </summary>
    /// <seealso cref="IOption" />
    public class DependencyOption : IOption
    {
        /// <summary>
        /// The dependency name
        /// </summary>
        private readonly string _dependencyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyOption"/> class.
        /// </summary>
        /// <param name="dependencyName">Name of the dependency.</param>
        public DependencyOption(string dependencyName) => _dependencyName = dependencyName;

        /// <summary>
        /// Applies the option to the specified host configurator.
        /// </summary>
        /// <param name="configurator">The host configurator.</param>
        public void ApplyTo(IHostConfigurator configurator) => configurator.AddDependency(_dependencyName);
    }
}
