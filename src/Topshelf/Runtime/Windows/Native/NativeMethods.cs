// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

#pragma warning disable S101,IDE1006,CA1069,RCS1234,RCS1157,CA1707,RCS1237,RCS1135,S4070,S1104,CA1051

namespace Topshelf.Runtime.Windows
{
    public static class NativeMethods
    {
        public const int SERVICE_CONFIG_FAILURE_ACTIONS = 2;
        public const int SERVICE_CONFIG_FAILURE_ACTIONS_FLAG = 4;

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

        public enum SC_ACTION_TYPE
        {
            None = 0,
            RestartService = 1,
            RebootComputer = 2,
            RunCommand = 3
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

        [Flags]
        public enum SYSTEM_ACCESS : uint
        {
            SE_PRIVILEGE_ENABLED = 0x00000002,
            TOKEN_QUERY = 0x00000008,
            TOKEN_ADJUST_PRIVILEGES = 0x00000020
        }

        [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AdjustTokenPrivileges(SafeTokenHandle TokenHandle,
            [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState,
            uint BufferLength,
            IntPtr PreviousState,
            IntPtr ReturnLength);

        [DllImport("AdvApi32", ExactSpelling = true, EntryPoint = "ChangeServiceConfig2W", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern bool ChangeServiceConfig2(ScmHandle serviceHandle, uint infoLevel, IntPtr lpInfo);

        [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseServiceHandle(IntPtr hSCObject);

        [DllImport("AdvApi32", ExactSpelling = true, EntryPoint = "LookupPrivilegeValueW", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

        [DllImport("AdvApi32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, out SafeTokenHandle TokenHandle);

        [DllImport("AdvApi32", ExactSpelling = true, EntryPoint = "OpenServiceW", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern ScmHandle OpenService(ScmHandle hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("AdvApi32", ExactSpelling = true, EntryPoint = "OpenSCManagerW", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern ScmHandle OpenSCManager(string? machineName, string? databaseName, uint dwAccess);

        public struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        public struct LUID_AND_ATTRIBUTES
        {
            public int Attributes;
            public LUID pLuid;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SC_ACTION
        {
            /// <summary>
            /// The action to be performed. This member can be one of the following values from the <see cref="SC_ACTION_TYPE" /> enumeration type.
            /// </summary>
            public int Type;

            /// <summary>
            /// The time to wait before performing the specified action, in milliseconds.
            /// </summary>
            public int Delay;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SERVICE_FAILURE_ACTIONS
        {
            public int dwResetPeriod;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpRebootMsg;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpCommand;

            public int cActions;
            public IntPtr actions;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SERVICE_FAILURE_ACTIONS_FLAG
        {
            [MarshalAs(UnmanagedType.Bool)]
            public bool fFailureActionsOnNonCrashFailures;
        }

        public struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }
    }
}
