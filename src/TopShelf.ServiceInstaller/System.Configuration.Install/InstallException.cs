using System.Runtime.Serialization;

namespace System.Configuration.Install
{
    [Serializable]
    public class InstallException : SystemException
    {
        internal const int Install = unchecked((int)0x80131907);

        public InstallException() => HResult = Install;

        public InstallException(string message) : base(message)
        {
        }

        public InstallException(string? message, Exception innerException) : base(message, innerException)
        {
        }

        protected InstallException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
