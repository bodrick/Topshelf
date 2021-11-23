using Autofac;
using Topshelf.Configuration.HostConfigurators;

namespace Topshelf.Autofac
{
    public static class HostConfiguratorExtensions
    {
        public static IHostConfigurator UseAutofacContainer(this IHostConfigurator configurator, ILifetimeScope lifetimeScope)
        {
            configurator.AddConfigurator(new AutofacHostBuilderConfigurator(lifetimeScope));
            return configurator;
        }
    }
}
