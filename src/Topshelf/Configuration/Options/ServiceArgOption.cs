using Topshelf.Configuration.HostConfigurators;

namespace Topshelf.Configuration.Options
{
    public class ServiceArgOption : IOption
    {
        private readonly string _argName;
        private readonly string _argValue;

        public ServiceArgOption(string value)
        {
            var splitIndex = value.IndexOf(':', System.StringComparison.OrdinalIgnoreCase);
            _argName = value[..splitIndex];
            _argValue = value[(splitIndex + 1)..];
        }

        public void ApplyTo(IHostConfigurator configurator) => configurator.AddServiceArgument(_argName, _argValue);
    }
}
