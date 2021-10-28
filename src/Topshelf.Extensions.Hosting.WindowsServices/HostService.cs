using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Topshelf.Hosting.WindowsServices
{
    /// <inheritdoc />
    /// <summary>
    /// Provides an implementation of a service that hosts an ASP.NET Core application.
    /// </summary>
    /// <seealso cref="IWin32Service" />
    public class HostService : IWin32Service
    {
        private readonly IHost _host;
        private bool _stopRequestedByWindows;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostService"/> class which hosts the specified host as a Windows service.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="serviceName">The name of the service to run. If <see langword="null"/>, the name of the entry assembly is used.</param>
        public HostService(IHost host, string serviceName = null)
        {
            ServiceName = serviceName ?? Assembly.GetEntryAssembly()?.GetName().Name;
            _host = host;
        }

        /// <inheritdoc />
        public string ServiceName { get; }

        /// <inheritdoc />
        public void Start(string[] startupArguments, ServiceStoppedCallback serviceStoppedCallback)
        {
            _host
                 .Services
                 .GetRequiredService<IHostApplicationLifetime>()
                 .ApplicationStopped
                 .Register(() =>
                 {
                     if (!_stopRequestedByWindows)
                     {
                         serviceStoppedCallback();
                     }
                 });

            _host.Start();
        }

        /// <inheritdoc />
        public void Stop()
        {
            _stopRequestedByWindows = true;
            _host.Dispose();
        }
    }
}
