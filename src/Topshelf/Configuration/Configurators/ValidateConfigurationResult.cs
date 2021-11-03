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
using System.Diagnostics;
using System.Linq;
using Topshelf.Exceptions;

namespace Topshelf.Configuration.Configurators
{
    [Serializable, DebuggerDisplay("{Message}")]
    public sealed class ValidateConfigurationResult : IConfigurationResult
    {
        private readonly IList<IValidateResult> _results;

        private ValidateConfigurationResult(IEnumerable<IValidateResult> results) => _results = results.ToList();

        public bool ContainsFailure => _results.Any(x => x.Disposition == ValidationResultDisposition.Failure);

        public string Message
        {
            get
            {
                var debuggerString = string.Join(Environment.NewLine, _results);
                return string.IsNullOrWhiteSpace(debuggerString)
                    ? string.Empty
                    : debuggerString;
            }
        }

        public IEnumerable<IValidateResult> Results => _results;

        public static IConfigurationResult CompileResults(IEnumerable<IValidateResult> results)
        {
            var result = new ValidateConfigurationResult(results);

            if (!result.ContainsFailure)
            {
                return result;
            }

            var message = $"The service was not properly configured: {Environment.NewLine}{result.Message}";
            throw new HostConfigurationException(message);
        }
    }
}
