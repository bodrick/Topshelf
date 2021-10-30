using System.Runtime.InteropServices;

namespace System.ServiceProcess
{
    internal static class SafeNativeMethods
    {
        [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool CloseServiceHandle(IntPtr handle);

        [DllImport("AdvApi32", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern int LsaClose(IntPtr objectHandle);

        [DllImport("AdvApi32", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern int LsaFreeMemory(IntPtr ptr);

        [DllImport("AdvApi32", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern int LsaNtStatusToWinError(int ntStatus);

        [DllImport("AdvApi32", ExactSpelling = true, EntryPoint = "OpenSCManagerW", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr OpenSCManager(string? machineName, string? databaseName, uint access);
    }
}
