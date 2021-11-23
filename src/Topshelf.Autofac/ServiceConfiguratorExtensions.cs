using System;
using Autofac;
using Topshelf.Configuration.ServiceConfigurators;

namespace Topshelf.Autofac
{
    public static class ServiceConfiguratorExtensions
    {
        public static IServiceConfigurator<T> ConstructUsingAutofacContainer<T>(this IServiceConfigurator<T> configurator) where T : class
        {
            configurator.ConstructUsing(_ =>
            {
                if (AutofacHostBuilderConfigurator.LifetimeScope is null)
                {
                    throw new InvalidOperationException();
                }
                return AutofacHostBuilderConfigurator.LifetimeScope.Resolve<T>();
            });
            return configurator;
        }
    }
}
