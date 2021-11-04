using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Topshelf.Extensions.Hosting;

namespace DisposeHostWhenServiceStopped
{
    public class MyService : IHostedService
    {
        private readonly Service _service;

        public MyService(Service service) => _service = service;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Service.DoSomething();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public sealed class Service : IDisposable
    {
        public void Dispose() => Console.WriteLine("Service is disposed");

        public static void DoSomething() => throw new InvalidOperationException("Break things");
    }

    internal class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) => new HostBuilder()
                .ConfigureServices(serviceCollection => serviceCollection
                    .AddHostedService<MyService>()
                    .AddSingleton<Service>());

        private static async Task Main(string[] args) =>
                    //await CreateHostBuilder(args).RunConsoleAsync();

                    CreateHostBuilder(args).RunAsTopshelfService(s =>
            {
                s.SetServiceName("Name");
                s.SetDisplayName("Name");
            });
    }
}
