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
namespace Topshelf.Configuration.Configurators
{
    public static class ValidationResultExtensions
    {
        public static IValidateResult Failure(this IConfigurator configurator, string message) => new ValidateResult(ValidationResultDisposition.Failure, message);

        public static IValidateResult Failure(this IConfigurator configurator, string key, string message) => new ValidateResult(ValidationResultDisposition.Failure, key, message);

        public static IValidateResult Failure(this IConfigurator configurator, string key, string value, string message) => new ValidateResult(ValidationResultDisposition.Failure, key, value, message);

        public static IValidateResult Success(this IConfigurator configurator, string message) => new ValidateResult(ValidationResultDisposition.Success, message);

        public static IValidateResult Success(this IConfigurator configurator, string key, string message) => new ValidateResult(ValidationResultDisposition.Success, key, message);

        public static IValidateResult Success(this IConfigurator configurator, string key, string value, string message) => new ValidateResult(ValidationResultDisposition.Success, key, value, message);

        public static IValidateResult Warning(this IConfigurator configurator, string message) => new ValidateResult(ValidationResultDisposition.Warning, message);

        public static IValidateResult Warning(this IConfigurator configurator, string key, string message) => new ValidateResult(ValidationResultDisposition.Warning, key, message);

        public static IValidateResult Warning(this IConfigurator configurator, string key, string value, string message) => new ValidateResult(ValidationResultDisposition.Warning, key, value, message);

        public static IValidateResult WithParentKey(this IValidateResult result, string parentKey)
        {
            var key = $"{parentKey}.{result.Key}";
            return new ValidateResult(result.Disposition, key, result.Value, result.Message);
        }
    }
}
