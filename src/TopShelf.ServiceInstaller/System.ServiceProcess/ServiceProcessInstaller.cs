using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Runtime.InteropServices;
using System.Text;

namespace System.ServiceProcess
{
    public class ServiceProcessInstaller : ComponentInstaller
    {
        private static bool _helpPrinted;
        private bool _haveLoginInfo;
        private string? _password;
        private ServiceAccount _serviceAccount = ServiceAccount.User;
        private string? _username;

        /// <summary>Gets or sets the type of account under which to run this service application.</summary>
        /// <returns>A <see cref="ServiceAccount" /> that defines the type of account under which the system runs this service. The default is User.</returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        ///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        [DefaultValue(ServiceAccount.User)]
        [ServiceProcessDescription(Res.ServiceProcessInstallerAccount)]
        public ServiceAccount Account
        {
            get
            {
                if (!_haveLoginInfo)
                {
                    GetLoginInfo();
                }
                return _serviceAccount;
            }
            set
            {
                _haveLoginInfo = false;
                _serviceAccount = value;
            }
        }

        /// <summary>Gets help text displayed for service installation options.</summary>
        /// <returns>Help text that provides a description of the steps for setting the user name and password in order to run the service under a particular account.</returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        /// </PermissionSet>
        protected override string HelpText
        {
            get
            {
                if (_helpPrinted)
                {
                    return base.HelpText;
                }
                _helpPrinted = true;
                return Res.GetString(Res.HelpText) + "\r\n" + base.HelpText;
            }
        }

        /// <summary>Gets or sets the password associated with the user account under which the service application runs.</summary>
        /// <returns>The password associated with the account under which the service should run. The default is an empty string (""). The property is not public, and is never serialized.</returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        ///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        [Browsable(false)]
        public string Password
        {
            get
            {
                if (!_haveLoginInfo)
                {
                    GetLoginInfo();
                }
                return _password ?? string.Empty;
            }
            set
            {
                _haveLoginInfo = false;
                _password = value;
            }
        }

        /// <summary>Gets or sets the user account under which the service application will run.</summary>
        /// <returns>The account under which the service should run. The default is an empty string ("").</returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        ///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [Browsable(false)]
        public string Username
        {
            get
            {
                if (!_haveLoginInfo)
                {
                    GetLoginInfo();
                }
                return _username ?? string.Empty;
            }
            set
            {
                _haveLoginInfo = false;
                _username = value;
            }
        }

        /// <summary>Implements the base class <see cref="ComponentInstaller.CopyFromComponent(IComponent)" /> method with no <see cref="ServiceProcessInstaller" /> class-specific behavior.</summary>
        /// <param name="component">The <see cref="IComponent" /> that represents the service process. </param>
        public override void CopyFromComponent(IComponent component)
        {
        }

        /// <summary>Writes service application information to the registry. This method is meant to be used by installation tools, which call the appropriate methods automatically.</summary>
        /// <param name="stateSaver">An <see cref="IDictionary" /> that contains the context information associated with the installation. </param>
        /// <exception cref="ArgumentException">The <paramref name="stateSaver" /> is null. </exception>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
        ///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        public override void Install(IDictionary stateSaver)
        {
            try
            {
                ServiceInstaller.CheckEnvironment();
                try
                {
                    if (!_haveLoginInfo)
                    {
                        try
                        {
                            GetLoginInfo();
                        }
                        catch
                        {
                            stateSaver["hadServiceLogonRight"] = true;
                            throw;
                        }
                    }
                }
                finally
                {
                    stateSaver["Account"] = Account;
                    if (Account == ServiceAccount.User)
                    {
                        stateSaver["Username"] = Username;
                    }
                }
                if (Account == ServiceAccount.User)
                {
                    // grant the right to run as a service to the given username. If we don't do this,
                    // the service will be unable to start under that account.
                    var policyHandle = OpenSecurityPolicy();
                    var hasServiceLogonRight = true;
                    try
                    {
                        var accountSid = GetAccountSid(Username);
                        hasServiceLogonRight = AccountHasRight(policyHandle, accountSid, "SeServiceLogonRight");
                        if (!hasServiceLogonRight)
                        {
                            GrantAccountRight(policyHandle, accountSid, "SeServiceLogonRight");
                        }
                    }
                    finally
                    {
                        stateSaver["hadServiceLogonRight"] = hasServiceLogonRight;
                        SafeNativeMethods.LsaClose(policyHandle);
                    }
                }
            }
            finally
            {
                // now install all the contained services. They will use the Username and Password properties to do
                // their installation.
                base.Install(stateSaver);
            }
        }

        /// <summary>Rolls back service application information written to the registry by the installation procedure. This method is meant to be used by installation tools, which process the appropriate methods automatically.</summary>
        /// <param name="savedState">An <see cref="IDictionary" /> that contains the context information associated with the installation. </param>
        /// <exception cref="ArgumentException">The <paramref name="savedState" /> is null.-or- The <paramref name="savedState" /> is corrupted or non-existent. </exception>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
        /// </PermissionSet>
        protected override void Rollback(IDictionary savedState)
        {
            try
            {
                var account = savedState["Account"] as ServiceAccount?;
                var hadServiceLogonRight = savedState["hadServiceLogonRight"] as bool?;
                if (account is ServiceAccount.User && hadServiceLogonRight is not true)
                {
                    var accountName = savedState["Username"] as string;
                    if (!string.IsNullOrEmpty(accountName))
                    {
                        var intPtr = OpenSecurityPolicy();
                        try
                        {
                            var accountSid = GetAccountSid(accountName);
                            RemoveAccountRight(intPtr, accountSid, "SeServiceLogonRight");
                        }
                        finally
                        {
                            SafeNativeMethods.LsaClose(intPtr);
                        }
                    }
                }
            }
            finally
            {
                base.Rollback(savedState);
            }
        }

        private static bool AccountHasRight(IntPtr policyHandle, byte[] accountSid, string rightName)
        {
            var status = NativeMethods.LsaEnumerateAccountRights(policyHandle, accountSid, out var pRights, out var rightsCount);
            switch (status)
            {
                case NativeMethods.STATUS_OBJECT_NAME_NOT_FOUND:
                    return false;

                case 0:
                    try
                    {
                        var pCurRights = pRights;
                        for (var i = 0; i < rightsCount; i++)
                        {
                            var lsaUnicodeStringWithPointer = new NativeMethods.LSA_UNICODE_STRING_withPointer();
                            Marshal.PtrToStructure(pCurRights, (object)lsaUnicodeStringWithPointer);
                            var array = new char[lsaUnicodeStringWithPointer.length / 2];
                            Marshal.Copy(lsaUnicodeStringWithPointer.pwstr, array, 0, array.Length);
                            if (string.Equals(new string(array, 0, array.Length), rightName, StringComparison.Ordinal))
                            {
                                return true;
                            }
                            pCurRights = (IntPtr)((long)pCurRights + Marshal.SizeOf(typeof(NativeMethods.LSA_UNICODE_STRING)));
                        }
                        return false;
                    }
                    finally
                    {
                        SafeNativeMethods.LsaFreeMemory(pRights);
                    }

                default:
                    throw new Win32Exception(SafeNativeMethods.LsaNtStatusToWinError(status));
            }
        }

        private static byte[] GetAccountSid(string accountName)
        {
            var newSid = new byte[256];
            var sidLen = new[] { newSid.Length };
            var domName = new char[1024];
            var domNameLen = new[] { domName.Length };
            var sidNameUse = new int[1];
            if (accountName[..2] == ".\\")
            {
                var compName = new StringBuilder(32);
                var nameLen = 32;
                if (!NativeMethods.GetComputerName(compName, ref nameLen))
                {
                    throw new Win32Exception();
                }
                accountName = compName + accountName[1..];
            }
            if (!NativeMethods.LookupAccountName(null, accountName, newSid, sidLen, domName, domNameLen, sidNameUse))
            {
                throw new Win32Exception();
            }
            var sid = new byte[sidLen[0]];
            Array.Copy(newSid, 0, sid, 0, sidLen[0]);
            return sid;
        }

        private static void GrantAccountRight(IntPtr policyHandle, byte[] accountSid, string rightName)
        {
            var lsaUnicodeString = new NativeMethods.LSA_UNICODE_STRING
            {
                buffer = rightName
            };
            lsaUnicodeString.length = (short)(lsaUnicodeString.buffer.Length * 2);
            lsaUnicodeString.maximumLength = lsaUnicodeString.length;
            var num = NativeMethods.LsaAddAccountRights(policyHandle, accountSid, lsaUnicodeString, 1);
            if (num == 0)
            {
                return;
            }
            throw new Win32Exception(SafeNativeMethods.LsaNtStatusToWinError(num));
        }

        private static IntPtr OpenSecurityPolicy()
        {
            var gcHandle = GCHandle.Alloc(new NativeMethods.LSA_OBJECT_ATTRIBUTES(), GCHandleType.Pinned);
            try
            {
                var pointerObjectAttributes = gcHandle.AddrOfPinnedObject();
                var num = NativeMethods.LsaOpenPolicy(null, pointerObjectAttributes, 2064, out var result);
                if (num != 0)
                {
                    throw new Win32Exception(SafeNativeMethods.LsaNtStatusToWinError(num));
                }
                return result;
            }
            finally
            {
                gcHandle.Free();
            }
        }

        private static void RemoveAccountRight(IntPtr policyHandle, byte[] accountSid, string rightName)
        {
            var lsaUnicodeString = new NativeMethods.LSA_UNICODE_STRING
            {
                buffer = rightName
            };
            lsaUnicodeString.length = (short)(lsaUnicodeString.buffer.Length * 2);
            lsaUnicodeString.maximumLength = lsaUnicodeString.length;
            var num = NativeMethods.LsaRemoveAccountRights(policyHandle, accountSid, false, lsaUnicodeString, 1);
            if (num == 0)
            {
                return;
            }
            throw new Win32Exception(SafeNativeMethods.LsaNtStatusToWinError(num));
        }

        private void GetLoginInfo()
        {
            if (Context != null && !DesignMode && !_haveLoginInfo)
            {
                _haveLoginInfo = true;
                if (_serviceAccount != ServiceAccount.User)
                {
                    return;
                }
                if (Context.Parameters.ContainsKey("username"))
                {
                    _username = Context.Parameters["username"];
                }
                if (Context.Parameters.ContainsKey("password"))
                {
                    _password = Context.Parameters["password"];
                }
                if (!string.IsNullOrEmpty(_username) && _password != null)
                {
                    return;
                }
                if (!Context.Parameters.ContainsKey("unattended"))
                {
                    throw new PlatformNotSupportedException();
                }
                throw new InvalidOperationException(Res.GetString(Res.UnattendedCannotPrompt, Context.Parameters["assemblypath"] ?? string.Empty));
            }
        }
    }
}
