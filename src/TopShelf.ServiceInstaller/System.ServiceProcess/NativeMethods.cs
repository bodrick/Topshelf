using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

#pragma warning disable S101,IDE1006,CA1069,RCS1234,RCS1157,CA1707,RCS1237,RCS1135,S4070,S1104,CA1051

namespace System.ServiceProcess
{
    public static class NativeMethods
    {
        [Flags]
        public enum ACCESS_MASK : uint
        {
            SPECIFIC_RIGHTS_ALL = 0x0000FFFF,
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            STANDARD_RIGHTS_READ = 0x00020000,
            STANDARD_RIGHTS_WRITE = 0x00020000,
            STANDARD_RIGHTS_EXECUTE = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            STANDARD_RIGHTS_REQUIRED = DELETE | READ_CONTROL | WRITE_DAC | WRITE_OWNER,
            SYNCHRONIZE = 0x00100000,
            STANDARD_RIGHTS_ALL = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE,
            ACCESS_SYSTEM_SECURITY = 0x01000000,
            MAXIMUM_ALLOWED = 0x02000000,
            GENERIC_ALL = 0x10000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_READ = 0x80000000
        }

        [Flags]
        public enum SCM_ACCESS : uint
        {
            /// <summary>
            /// Required to connect to the service control manager.
            /// </summary>
            SC_MANAGER_CONNECT = 0x00001,

            /// <summary>
            /// Required to call the CreateService function to create a service
            /// object and add it to the database.
            /// </summary>
            SC_MANAGER_CREATE_SERVICE = 0x00002,

            /// <summary>
            /// Required to call the EnumServicesStatusEx function to list the
            /// services that are in the database.
            /// </summary>
            SC_MANAGER_ENUMERATE_SERVICE = 0x00004,

            /// <summary>
            /// Required to call the LockServiceDatabase function to acquire a
            /// lock on the database.
            /// </summary>
            SC_MANAGER_LOCK = 0x00008,

            /// <summary>
            /// Required to call the QueryServiceLockStatus function to retrieve
            /// the lock status information for the database.
            /// </summary>
            SC_MANAGER_QUERY_LOCK_STATUS = 0x00010,

            /// <summary>
            /// Required to call the NotifyBootConfigStatus function.
            /// </summary>
            SC_MANAGER_MODIFY_BOOT_CONFIG = 0x00020,

            /// <summary>
            /// Includes STANDARD_RIGHTS_REQUIRED, in addition to all access
            /// rights in this table.
            /// </summary>
            SC_MANAGER_ALL_ACCESS = ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
                                    SC_MANAGER_CONNECT |
                                    SC_MANAGER_CREATE_SERVICE |
                                    SC_MANAGER_ENUMERATE_SERVICE |
                                    SC_MANAGER_LOCK |
                                    SC_MANAGER_QUERY_LOCK_STATUS |
                                    SC_MANAGER_MODIFY_BOOT_CONFIG
        }

        public const int STATUS_OBJECT_NAME_NOT_FOUND = -1073741772;

        [DllImport("AdvApi32", ExactSpelling = true, EntryPoint = "ChangeServiceConfig2W", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern bool ChangeServiceConfig2(IntPtr serviceHandle, uint infoLevel, ref SERVICE_DESCRIPTION serviceDesc);

        [DllImport("AdvApi32", ExactSpelling = true, EntryPoint = "ChangeServiceConfig2W", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern bool ChangeServiceConfig2(IntPtr serviceHandle, uint infoLevel, ref SERVICE_DELAYED_AUTOSTART_INFO serviceDesc);

        [DllImport("AdvApi32", ExactSpelling = true, EntryPoint = "CreateServiceW", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr CreateService(IntPtr databaseHandle, string serviceName, string? displayName, int access,
            int serviceType, int startType, int errorControl, string? binaryPath, string? loadOrderGroup, IntPtr pTagId,
            string? dependencies,
            string? servicesStartName, string? password);

        [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern bool DeleteService(IntPtr serviceHandle);

        [DllImport("AdvApi32", ExactSpelling = true, EntryPoint = "LookupAccountNameW", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern bool LookupAccountName(string? systemName, string accountName, byte[] sid, int[] sidLen,
            char[] refDomainName,
            int[] domNameLen, [In][Out] int[] sidNameUse);

        [DllImport("AdvApi32", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int LsaAddAccountRights(IntPtr policyHandle, byte[] accountSid, LSA_UNICODE_STRING userRights,
            int countOfRights);

        [DllImport("AdvApi32", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int LsaEnumerateAccountRights(IntPtr policyHandle, byte[] accountSid, out IntPtr pLsaUnicodeStringUserRights,
            out int countOfRights);

        [DllImport("AdvApi32", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int LsaOpenPolicy(LSA_UNICODE_STRING? systemName, IntPtr pointerObjectAttributes, int desiredAccess,
            out IntPtr pointerPolicyHandle);

        [DllImport("AdvApi32", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int LsaRemoveAccountRights(IntPtr policyHandle, byte[] accountSid, bool allRights,
            LSA_UNICODE_STRING userRights, int countOfRights);

        [DllImport("AdvApi32", ExactSpelling = true, EntryPoint = "OpenServiceW", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr OpenService(IntPtr databaseHandle, string serviceName, int access);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SERVICE_DELAYED_AUTOSTART_INFO
        {
            public bool fDelayedAutostart;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SERVICE_DESCRIPTION
        {
            public IntPtr description;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class LSA_OBJECT_ATTRIBUTES
        {
            public int length;

            public IntPtr rootDirectory = (IntPtr)0;

            public IntPtr pointerLsaString = (IntPtr)0;

            public int attributes;

            public IntPtr pointerSecurityDescriptor = (IntPtr)0;

            public IntPtr pointerSecurityQualityOfService = (IntPtr)0;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class LSA_UNICODE_STRING
        {
            public short length;

            public short maximumLength;

            public string buffer;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class LSA_UNICODE_STRING_withPointer
        {
            public short length;

            public short maximumLength;

            public IntPtr pwstr = (IntPtr)0;
        }
    }
}
