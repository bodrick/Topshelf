namespace System.ServiceProcess
{
    public static class ServiceControllerExtensions
    {
        public static bool ValidServiceName(string serviceName)
        {
            if (serviceName == null)
            {
                return false;
            }
            if (serviceName.Length is <= 80 and not 0)
            {
                foreach (var c in serviceName)
                {
                    if (c is '\\' or '/')
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
