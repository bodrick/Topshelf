using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Topshelf.Extensions.Hosting;

namespace RunGenericHostAsTopshelfService
{
    internal class Program
    {
        private static void Main()
        {
            var builder = new HostBuilder()
                .ConfigureServices(services => services.AddHostedService<FileWriterService>());

            builder.RunAsTopshelfService(hc =>
            {
                hc.SetServiceName("GenericHostInTopshelf");
                hc.SetDisplayName("Generic Host In Topshelf");
                hc.SetDescription("Runs a generic host as a Topshelf service.");
            });
        }
    }
}
