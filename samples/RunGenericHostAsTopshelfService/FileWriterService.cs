using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace RunGenericHostAsTopshelfService
{
    internal sealed class FileWriterService : IHostedService, IDisposable
    {
        private const string Path = @"c:\temp\TestApplication.txt";
        private bool _disposed;
        private Timer? _timer;

        public void Dispose() => Dispose(true);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            _timer = new Timer(
                _ => WriteTimeToFile(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private static void WriteTimeToFile()
        {
            if (!File.Exists(Path))
            {
                using var sw = File.CreateText(Path);
                sw.WriteLine(DateTime.UtcNow.ToString("O"));
            }
            else
            {
                using var sw = File.AppendText(Path);
                sw.WriteLine(DateTime.UtcNow.ToString("O"));
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _timer?.Dispose();
            }

            _disposed = true;
        }
    }
}
