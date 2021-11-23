using System;
using System.Collections.Generic;
using Autofac;
using Topshelf.Configuration.Builders;
using Topshelf.Configuration.Configurators;
using Topshelf.Configuration.HostConfigurators;

namespace Topshelf.Autofac
{
    public class AutofacHostBuilderConfigurator : IHostBuilderConfigurator
    {
        public AutofacHostBuilderConfigurator(ILifetimeScope lifetimeScope) =>
            LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

        public static ILifetimeScope? LifetimeScope { get; private set; }

        public IHostBuilder Configure(IHostBuilder builder) => builder;

        IEnumerable<IValidateResult> IConfigurator.Validate()
        {
            yield break;
        }
    }
}
