using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Topshelf.Configuration;

namespace Topshelf.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Builds and run the host as a Topshelf service.
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="configureTopshelfHost"></param>
        /// <param name="beforeHostStarting"></param>
        /// <param name="afterHostStopped"></param>
        /// <exception cref="ArgumentNullException"><paramref name="configureTopshelfHost"/> is <c>null</c>.</exception>
        public static TopshelfExitCode RunAsTopshelfService(this IHostBuilder hostBuilder,
            Action<Configuration.HostConfigurators.IHostConfigurator> configureTopshelfHost,
            Action<Microsoft.Extensions.Hosting.IHost>? beforeHostStarting = null,
            Action<Microsoft.Extensions.Hosting.IHost>? afterHostStopped = null)
        {
            hostBuilder.UseTopshelfLifetime();

            Microsoft.Extensions.Hosting.IHost? host = null;
            try
            {
                return HostFactory.Run(x =>
                {
                    configureTopshelfHost(x);
                    x.Service((Action<Configuration.ServiceConfigurators.IServiceConfigurator<Microsoft.Extensions.Hosting.IHost>>)(s =>
                    {
                        s.ConstructUsing(() =>
                        {
                            host = hostBuilder.Build();
                            return host;
                        });
                        s.WhenStarted(service =>
                        {
                            beforeHostStarting?.Invoke(service);
                            service.Start();
                        });
                        s.WhenStopped(service =>
                        {
                            service.StopAsync().Wait();
                            afterHostStopped?.Invoke(service);
                        });
                    }));
                });
            }
            finally
            {
                host?.Dispose();
            }
        }

        public static IHostBuilder UseTopshelfLifetime(this IHostBuilder hostBuilder) =>
            hostBuilder.ConfigureServices((_, services) => services.AddSingleton<IHostLifetime, TopshelfLifetime>());
    }
}
