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
using System.Diagnostics;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

#pragma warning disable S101,IDE1006,S1144

namespace Topshelf.Runtime.Windows
{
    internal static class Kernel32
    {
        private const uint TH32CS_SNAPPROCESS = 2;
        private const int MAX_PATH = 260;

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool CloseHandle(IntPtr handle);

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESSENTRY32
        {
            internal uint dwSize;
            internal uint cntUsage;
            internal uint th32ProcessID;
            internal IntPtr th32DefaultHeapID;
            internal uint th32ModuleID;
            internal uint cntThreads;
            internal uint th32ParentProcessID;
            internal int pcPriClassBase;
            internal uint dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            internal string szExeFile;
        }

        internal static Process? GetParentProcess()
        {
            var snapshotHandle = IntPtr.Zero;
            try
            {
                // Get a list of all processes
                snapshotHandle = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

                if (snapshotHandle == IntPtr.Zero)
                {
                    return null;
                }

                var processInfo = new PROCESSENTRY32 { dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32)) };

                if (!Process32First(snapshotHandle, ref processInfo))
                {
                    return null;
                }

                var currentProcessId = Environment.ProcessId;
                do
                {
                    if (currentProcessId == processInfo.th32ProcessID)
                    {
                        return Process.GetProcessById((int)processInfo.th32ParentProcessID);
                    }
                }
                while (Process32Next(snapshotHandle, ref processInfo));
            }
            catch
            {
                // Ignored
            }
            finally
            {
                CloseHandle(snapshotHandle);
            }

            return null;
        }
    }
}
