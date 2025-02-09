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
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Topshelf.Exceptions;

namespace Topshelf.Runtime.Windows
{
    public static class WindowsServiceRecoveryController
    {
        public static void SetServiceRecoveryOptions(IHostSettings settings, ServiceRecoveryOptions options)
        {
            ScmHandle? scmHandle = null;
            ScmHandle? serviceHandle = null;
            var lpsaActions = IntPtr.Zero;
            var lpInfo = IntPtr.Zero;
            var lpFlagInfo = IntPtr.Zero;

            try
            {
                var actions = options.Actions.Select(x => x.GetAction()).ToList();
                if (actions.Count == 0)
                {
                    throw new TopshelfException("Must be at least one failure action configured");
                }

                scmHandle = NativeMethods.OpenSCManager(null, null, (uint)NativeMethods.SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
                if (scmHandle == null)
                {
                    throw new TopshelfException("Failed to open service control manager");
                }

                serviceHandle = NativeMethods.OpenService(scmHandle, settings.ServiceName, (uint)NativeMethods.SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
                if (serviceHandle == null)
                {
                    throw new TopshelfException($"Failed to open service: {settings.ServiceName}");
                }

                var actionSize = Marshal.SizeOf(typeof(NativeMethods.SC_ACTION));
                lpsaActions = Marshal.AllocHGlobal((actionSize * actions.Count) + 1);
                if (lpsaActions == IntPtr.Zero)
                {
                    throw new TopshelfException("Unable to allocate memory for service recovery actions");
                }

                var nextAction = lpsaActions;
                foreach (var action in actions)
                {
                    Marshal.StructureToPtr(action, nextAction, false);
                    nextAction = (IntPtr)(nextAction.ToInt64() + actionSize);
                }

                var rebootMessage = options.Actions.Where(x => x.GetType() == typeof(RestartSystemRecoveryAction))
                    .OfType<RestartSystemRecoveryAction>().Select(x => x.RestartMessage).FirstOrDefault() ?? string.Empty;

                var runProgramCommand = options.Actions.Where(x => x.GetType() == typeof(RunProgramRecoveryAction))
                    .OfType<RunProgramRecoveryAction>().Select(x => x.Command).FirstOrDefault() ?? string.Empty;

                var failureActions = new NativeMethods.SERVICE_FAILURE_ACTIONS
                {
                    dwResetPeriod = (int)TimeSpan.FromDays(options.ResetPeriod).TotalSeconds,
                    lpRebootMsg = rebootMessage,
                    lpCommand = runProgramCommand,
                    cActions = actions.Count,
                    actions = lpsaActions
                };

                lpInfo = Marshal.AllocHGlobal(Marshal.SizeOf(failureActions));
                if (lpInfo == IntPtr.Zero)
                {
                    throw new TopshelfException("Failed to allocate memory for failure actions");
                }

                Marshal.StructureToPtr(failureActions, lpInfo, false);

                // If user specified a Restart option, get shutdown privileges
                if (options.Actions.Any(x => x.GetType() == typeof(RestartSystemRecoveryAction)))
                {
                    RequestShutdownPrivileges();
                }

                if (!NativeMethods.ChangeServiceConfig2(serviceHandle, NativeMethods.SERVICE_CONFIG_FAILURE_ACTIONS, lpInfo))
                {
                    throw new TopshelfException($"Failed to change service recovery options. Windows Error: {new Win32Exception().Message}");
                }

                if (!options.RecoverOnCrashOnly)
                {
                    var flag = new NativeMethods.SERVICE_FAILURE_ACTIONS_FLAG { fFailureActionsOnNonCrashFailures = true };

                    lpFlagInfo = Marshal.AllocHGlobal(Marshal.SizeOf(flag));
                    if (lpFlagInfo == IntPtr.Zero)
                    {
                        throw new TopshelfException("Failed to allocate memory for failure flag");
                    }

                    Marshal.StructureToPtr(flag, lpFlagInfo, false);

                    try
                    {
                        NativeMethods.ChangeServiceConfig2(serviceHandle, NativeMethods.SERVICE_CONFIG_FAILURE_ACTIONS_FLAG, lpFlagInfo);
                    }
                    catch
                    {
                        // this fails on XP, but we don't care really as it's optional
                    }
                }
            }
            finally
            {
                if (lpFlagInfo != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(lpFlagInfo);
                }

                if (lpInfo != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(lpInfo);
                }

                if (lpsaActions != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(lpsaActions);
                }

                serviceHandle?.Close();
                scmHandle?.Close();
            }
        }

        private static void RequestShutdownPrivileges()
        {
            ThrowOnFail(
                NativeMethods.OpenProcessToken(System.Diagnostics.Process.GetCurrentProcess().Handle,
                    (int)NativeMethods.SYSTEM_ACCESS.TOKEN_ADJUST_PRIVILEGES |
                    (int)NativeMethods.SYSTEM_ACCESS.TOKEN_QUERY, out var hToken));

            NativeMethods.TOKEN_PRIVILEGES tkp;
            tkp.PrivilegeCount = 1;
            tkp.Privileges.Attributes = (int)NativeMethods.SYSTEM_ACCESS.SE_PRIVILEGE_ENABLED;
            const string seShutdownPrivilege = "SeShutdownPrivilege";
            ThrowOnFail(NativeMethods.LookupPrivilegeValue(string.Empty, seShutdownPrivilege, out tkp.Privileges.pLuid));
            ThrowOnFail(NativeMethods.AdjustTokenPrivileges(hToken, false, ref tkp, 0U, IntPtr.Zero, IntPtr.Zero));
        }

        private static void ThrowOnFail(bool success)
        {
            if (!success)
            {
                throw new TopshelfException(
                    $"Computer shutdown was specified as a recovery option, but privileges could not be acquired. Windows Error: {new Win32Exception().Message}");
            }
        }
    }
}
