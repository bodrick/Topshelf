using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Topshelf.Runtime.Windows
{
    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle() : base(true) { }

        protected override bool ReleaseHandle() => CloseHandle(handle);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool CloseHandle(IntPtr handle);
    }
}
