using Microsoft.Extensions.Hosting;

namespace Topshelf.Hosting.WindowsServices
{
    /// <summary>
    /// Extensions to <see cref="IHostBuilder"/> for Windows service hosting scenarios.
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Runs the specified application inside a Windows service and blocks until the service is stopped.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="serviceName">The name of the service to run.</param>
        public static void RunAsService(this IHost host, string serviceName = null) =>
            new Win32ServiceHost(new HostService(host, serviceName)).Run();
    }
}
