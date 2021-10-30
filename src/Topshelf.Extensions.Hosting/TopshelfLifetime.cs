using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Topshelf.Extensions.Hosting
{
    public class TopshelfLifetime : IHostLifetime
    {
        public TopshelfLifetime(IHostApplicationLifetime hostApplicationLifetime, IServiceProvider services) =>
            HostApplicationLifetime = hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));

        private IHostApplicationLifetime HostApplicationLifetime { get; }

        public Task WaitForStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
