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
using System.Globalization;
using System.Linq;
using System.Reflection;
using Topshelf.Configuration.HostConfigurators;
using Topshelf.Configuration.HostConfigurators.AssemblyExtensions;

namespace Topshelf.Configuration
{
    namespace HostConfigurators.AssemblyExtensions
    {
        public static class AssemblyExtensions
        {
            public static T? GetAttribute<T>(this Assembly assembly) where T : Attribute =>
                assembly.GetCustomAttributes(typeof(T), false)
                    .Cast<T>()
                    .FirstOrDefault();

            public static string ToServiceNameSafeString(this string input) =>
                CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input).Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);

            public static TReturn TryGetProperty<TInput, TReturn>(this TInput attribute, Func<TInput, TReturn> accessor)
                where TInput : Attribute => accessor(attribute);
        }
    }

    public static class HostConfiguratorExtensions
    {
        public static void UseAssemblyInfoForServiceInfo(this IHostConfigurator hostConfigurator, Assembly? assembly)
        {
            if (assembly is null)
            {
                return;
            }

            var assemblyTitleAttribute = assembly.GetAttribute<AssemblyTitleAttribute>();
            if (assemblyTitleAttribute != null)
            {
                hostConfigurator.SetDisplayName(assemblyTitleAttribute.TryGetProperty(x => x.Title));
                hostConfigurator.SetServiceName(assemblyTitleAttribute.TryGetProperty(x => x.Title).ToServiceNameSafeString());
            }

            var assemblyDescriptionAttribute = assembly.GetAttribute<AssemblyDescriptionAttribute>();
            if (assemblyDescriptionAttribute != null)
            {
                hostConfigurator.SetDescription(assemblyDescriptionAttribute.TryGetProperty(x => x.Description));
            }
        }

        public static void UseAssemblyInfoForServiceInfo(this IHostConfigurator hostConfigurator) => hostConfigurator.UseAssemblyInfoForServiceInfo(Assembly.GetEntryAssembly());
    }
}
