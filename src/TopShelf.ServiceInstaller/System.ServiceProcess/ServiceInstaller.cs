using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace System.ServiceProcess
{
    public class ServiceInstaller : ComponentInstaller
    {
        private const string LocalServiceName = "NT AUTHORITY\\LocalService";
        private const string NetworkServiceName = "NT AUTHORITY\\NetworkService";
        //private EventLogInstaller eventLogInstaller;

        private static bool environmentChecked;
        private static bool isWin9x;
        private bool delayedStartMode;
        private string description = "";
        private string displayName = "";
        private string serviceName = "";
        private string[] servicesDependedOn = new string[0];

        private ServiceStartMode startType = ServiceStartMode.Manual;

        /// <summary>Initializes a new instance of the <see cref="T:System.ServiceProcess.ServiceInstaller" /> class.</summary>
        public ServiceInstaller()
        {
            //this.eventLogInstaller = new EventLogInstaller();
            //this.eventLogInstaller.Log = "Application";
            //this.eventLogInstaller.Source = "";
            //this.eventLogInstaller.UninstallAction = UninstallAction.Remove;
            //base.Installers.Add(this.eventLogInstaller);
        }

        /// <summary>Gets or sets a value that indicates whether the service should be delayed from starting until other automatically started services are running.</summary>
        /// <returns>true to delay automatic start of the service; otherwise, false. The default is false.</returns>
        [DefaultValue(false)]
        [ServiceProcessDescription("ServiceInstallerDelayedAutoStart")]
        public bool DelayedAutoStart
        {
            get => delayedStartMode;
            set => delayedStartMode = value;
        }

        /// <summary>Gets or sets the description for the service.</summary>
        /// <returns>The description of the service. The default is an empty string ("").</returns>
        [DefaultValue("")]
        [ComVisible(false)]
        [ServiceProcessDescription("ServiceInstallerDescription")]
        public string Description
        {
            get => description;
            set
            {
                if (value == null)
                {
                    value = "";
                }
                description = value;
            }
        }

        /// <summary>Indicates the friendly name that identifies the service to the user.</summary>
        /// <returns>The name associated with the service, used frequently for interactive tools.</returns>
        [DefaultValue("")]
        [ServiceProcessDescription("ServiceInstallerDisplayName")]
        public string DisplayName
        {
            get => displayName;
            set
            {
                if (value == null)
                {
                    value = "";
                }
                displayName = value;
            }
        }

        /// <summary>Indicates the name used by the system to identify this service. This property must be identical to the <see cref="P:System.ServiceProcess.ServiceBase.ServiceName" /> of the service you want to install.</summary>
        /// <returns>The name of the service to be installed. This value must be set before the install utility attempts to install the service.</returns>
        /// <exception cref="T:System.ArgumentException">The <see cref="P:System.ServiceProcess.ServiceInstaller.ServiceName" /> property is invalid. </exception>
        [DefaultValue("")]
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [ServiceProcessDescription("ServiceInstallerServiceName")]
        public string ServiceName
        {
            get => serviceName;
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if (!ServiceControllerExtensions.ValidServiceName(value))
                {
                    throw new ArgumentException(Res.GetString("ServiceName", value, 80.ToString(CultureInfo.CurrentCulture)));
                }
                serviceName = value;
                //this.eventLogInstaller.Source = value;
            }
        }

        /// <summary>Indicates the services that must be running for this service to run.</summary>
        /// <returns>An array of services that must be running before the service associated with this installer can run.</returns>
        [ServiceProcessDescription("ServiceInstallerServicesDependedOn")]
        public string[] ServicesDependedOn
        {
            get => servicesDependedOn;
            set
            {
                if (value == null)
                {
                    value = new string[0];
                }
                servicesDependedOn = value;
            }
        }

        /// <summary>Indicates how and when this service is started.</summary>
        /// <returns>A <see cref="T:System.ServiceProcess.ServiceStartMode" /> that represents the way the service is started. The default is Manual, which specifies that the service will not automatically start after reboot.</returns>
        /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The start mode is not a value of the <see cref="T:System.ServiceProcess.ServiceStartMode" /> enumeration.</exception>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        /// </PermissionSet>
        [DefaultValue(ServiceStartMode.Manual)]
        [ServiceProcessDescription("ServiceInstallerStartType")]
        public ServiceStartMode StartType
        {
            get => startType;
            set
            {
                if (!Enum.IsDefined(typeof(ServiceStartMode), value))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(ServiceStartMode));
                }
                if (value != 0 && value != ServiceStartMode.System)
                {
                    startType = value;
                    return;
                }
                throw new ArgumentException(Res.GetString("ServiceStartType", value));
            }
        }

        /// <summary>Copies properties from an instance of <see cref="T:System.ServiceProcess.ServiceBase" /> to this installer.</summary>
        /// <param name="component">The <see cref="T:System.ComponentModel.IComponent" /> from which to copy. </param>
        /// <exception cref="T:System.ArgumentException">The component you are associating with this installer does not inherit from <see cref="T:System.ServiceProcess.ServiceBase" />. </exception>
        public override void CopyFromComponent(IComponent component)
        {
            if (!(component is ServiceBase))
            {
                throw new ArgumentException(Res.GetString("NotAService"));
            }
            var serviceBase = (ServiceBase)component;
            ServiceName = serviceBase.ServiceName;
        }

        /// <summary>Installs the service by writing service application information to the registry. This method is meant to be used by installation tools, which process the appropriate methods automatically.</summary>
        /// <param name="stateSaver">An <see cref="T:System.Collections.IDictionary" /> that contains the context information associated with the installation. </param>
        /// <exception cref="T:System.InvalidOperationException">The installation does not contain a <see cref="T:System.ServiceProcess.ServiceProcessInstaller" /> for the executable.-or- The file name for the assembly is null or an empty string.-or- The service name is invalid.-or- The Service Control Manager could not be opened. </exception>
        /// <exception cref="T:System.ArgumentException">The display name for the service is more than 255 characters in length.</exception>
        /// <exception cref="T:System.ComponentModel.Win32Exception">The system could not generate a handle to the service. -or-A service with that name is already installed.</exception>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
        ///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        public override void Install(IDictionary stateSaver)
        {
            base.Context.LogMessage(Res.GetString("InstallingService", ServiceName));
            try
            {
                ServiceInstaller.CheckEnvironment();
                string servicesStartName = null;
                string password = null;
                ServiceProcessInstaller serviceProcessInstaller = null;
                if (base.Parent is ServiceProcessInstaller)
                {
                    serviceProcessInstaller = (ServiceProcessInstaller)base.Parent;
                }
                else
                {
                    var num = 0;
                    while (num < base.Parent.Installers.Count)
                    {
                        if (!(base.Parent.Installers[num] is ServiceProcessInstaller))
                        {
                            num++;
                            continue;
                        }
                        serviceProcessInstaller = (ServiceProcessInstaller)base.Parent.Installers[num];
                        break;
                    }
                }
                if (serviceProcessInstaller == null)
                {
                    throw new InvalidOperationException(Res.GetString("NoInstaller"));
                }
                switch (serviceProcessInstaller.Account)
                {
                    case ServiceAccount.LocalService:
                        servicesStartName = "NT AUTHORITY\\LocalService";
                        break;

                    case ServiceAccount.NetworkService:
                        servicesStartName = "NT AUTHORITY\\NetworkService";
                        break;

                    case ServiceAccount.User:
                        servicesStartName = serviceProcessInstaller.Username;
                        password = serviceProcessInstaller.Password;
                        break;
                }
                var text = base.Context.Parameters["assemblypath"];
                if (string.IsNullOrEmpty(text))
                {
                    throw new InvalidOperationException(Res.GetString("FileName"));
                }
                if (text.IndexOf('"') == -1)
                {
                    text = "\"" + text + "\"";
                }
                if (!ServiceInstaller.ValidateServiceName(ServiceName))
                {
                    throw new InvalidOperationException(Res.GetString("ServiceName", ServiceName, 80.ToString(CultureInfo.CurrentCulture)));
                }
                if (DisplayName.Length > 255)
                {
                    throw new ArgumentException(Res.GetString("DisplayNameTooLong", DisplayName));
                }
                string dependencies = null;
                if (ServicesDependedOn.Length != 0)
                {
                    var stringBuilder = new StringBuilder();
                    for (var i = 0; i < ServicesDependedOn.Length; i++)
                    {
                        var text2 = ServicesDependedOn[i];
                        try
                        {
                            text2 = new ServiceController(text2, ".").ServiceName;
                        }
                        catch
                        {
                        }
                        stringBuilder.Append(text2);
                        stringBuilder.Append('\0');
                    }
                    stringBuilder.Append('\0');
                    dependencies = stringBuilder.ToString();
                }
                var intPtr = SafeNativeMethods.OpenSCManager(null, null, 983103);
                var intPtr2 = IntPtr.Zero;
                if (intPtr == IntPtr.Zero)
                {
                    throw new InvalidOperationException(Res.GetString("OpenSC", "."), new Win32Exception());
                }
                var serviceType = 16;
                var num2 = 0;
                for (var j = 0; j < base.Parent.Installers.Count; j++)
                {
                    if (base.Parent.Installers[j] is ServiceInstaller)
                    {
                        num2++;
                        if (num2 > 1)
                        {
                            break;
                        }
                    }
                }
                if (num2 > 1)
                {
                    serviceType = 32;
                }
                try
                {
                    intPtr2 = NativeMethods.CreateService(intPtr, ServiceName, DisplayName, 983551, serviceType, (int)StartType, 1, text, null, IntPtr.Zero, dependencies, servicesStartName, password);
                    if (intPtr2 == IntPtr.Zero)
                    {
                        throw new Win32Exception();
                    }
                    if (Description.Length != 0)
                    {
                        var sERVICE_DESCRIPTION = default(NativeMethods.SERVICE_DESCRIPTION);
                        sERVICE_DESCRIPTION.description = Marshal.StringToHGlobalUni(Description);
                        var num3 = NativeMethods.ChangeServiceConfig2(intPtr2, 1u, ref sERVICE_DESCRIPTION);
                        Marshal.FreeHGlobal(sERVICE_DESCRIPTION.description);
                        if (!num3)
                        {
                            throw new Win32Exception();
                        }
                    }
                    if (Environment.OSVersion.Version.Major > 5 && StartType == ServiceStartMode.Automatic)
                    {
                        var sERVICE_DELAYED_AUTOSTART_INFO = default(NativeMethods.SERVICE_DELAYED_AUTOSTART_INFO);
                        sERVICE_DELAYED_AUTOSTART_INFO.fDelayedAutostart = DelayedAutoStart;
                        if (!NativeMethods.ChangeServiceConfig2(intPtr2, 3u, ref sERVICE_DELAYED_AUTOSTART_INFO))
                        {
                            throw new Win32Exception();
                        }
                    }
                    stateSaver["installed"] = true;
                }
                finally
                {
                    if (intPtr2 != IntPtr.Zero)
                    {
                        SafeNativeMethods.CloseServiceHandle(intPtr2);
                    }
                    SafeNativeMethods.CloseServiceHandle(intPtr);
                }
                base.Context.LogMessage(Res.GetString("InstallOK", ServiceName));
            }
            finally
            {
                base.Install(stateSaver);
            }
        }

        /// <summary>Indicates whether two installers would install the same service.</summary>
        /// <returns>true if calling <see cref="M:System.ServiceProcess.ServiceInstaller.Install(System.Collections.IDictionary)" /> on both of these installers would result in installing the same service; otherwise, false.</returns>
        /// <param name="otherInstaller">A <see cref="T:System.Configuration.Install.ComponentInstaller" /> to which you are comparing the current installer. </param>
        public override bool IsEquivalentInstaller(ComponentInstaller otherInstaller)
        {
            var serviceInstaller = otherInstaller as ServiceInstaller;
            if (serviceInstaller == null)
            {
                return false;
            }
            return serviceInstaller.ServiceName == ServiceName;
        }

        /// <summary>Rolls back service application information written to the registry by the installation procedure. This method is meant to be used by installation tools, which process the appropriate methods automatically.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the context information associated with the installation. </param>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
        ///   <IPermission class="System.ServiceProcess.ServiceControllerPermission, System.ServiceProcess, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
        /// </PermissionSet>
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
            var obj = savedState["installed"];
            if (obj != null && (bool)obj)
            {
                RemoveService();
            }
        }

        /// <summary>Uninstalls the service by removing information about it from the registry.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the context information associated with the installation. </param>
        /// <exception cref="T:System.ComponentModel.Win32Exception">The Service Control Manager could not be opened.-or- The system could not get a handle to the service. </exception>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
        ///   <IPermission class="System.ServiceProcess.ServiceControllerPermission, System.ServiceProcess, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
        /// </PermissionSet>
        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            RemoveService();
        }

        internal static void CheckEnvironment()
        {
            if (ServiceInstaller.environmentChecked)
            {
                if (!ServiceInstaller.isWin9x)
                {
                    return;
                }
                throw new PlatformNotSupportedException(Res.GetString("CantControlOnWin9x"));
            }
            ServiceInstaller.isWin9x = (Environment.OSVersion.Platform != PlatformID.Win32NT);
            ServiceInstaller.environmentChecked = true;
            if (!ServiceInstaller.isWin9x)
            {
                return;
            }
            throw new PlatformNotSupportedException(Res.GetString("CantInstallOnWin9x"));
        }

        private static bool ValidateServiceName(string name)
        {
            if (name != null && name.Length != 0 && name.Length <= 80)
            {
                var array = name.ToCharArray();
                for (var i = 0; i < array.Length; i++)
                {
                    if (array[i] < ' ' || array[i] == '/' || array[i] == '\\')
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private void RemoveService()
        {
            base.Context.LogMessage(Res.GetString("ServiceRemoving", ServiceName));
            var intPtr = SafeNativeMethods.OpenSCManager(null, null, 983103);
            if (intPtr == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            var intPtr2 = IntPtr.Zero;
            try
            {
                intPtr2 = NativeMethods.OpenService(intPtr, ServiceName, 65536);
                if (intPtr2 == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }
                NativeMethods.DeleteService(intPtr2);
            }
            finally
            {
                if (intPtr2 != IntPtr.Zero)
                {
                    SafeNativeMethods.CloseServiceHandle(intPtr2);
                }
                SafeNativeMethods.CloseServiceHandle(intPtr);
            }
            base.Context.LogMessage(Res.GetString("ServiceRemoved", ServiceName));
            try
            {
                using (var serviceController = new ServiceController(ServiceName))
                {
                    if (serviceController.Status != ServiceControllerStatus.Stopped)
                    {
                        base.Context.LogMessage(Res.GetString("TryToStop", ServiceName));
                        serviceController.Stop();
                        var num = 10;
                        serviceController.Refresh();
                        while (serviceController.Status != ServiceControllerStatus.Stopped && num > 0)
                        {
                            Thread.Sleep(1000);
                            serviceController.Refresh();
                            num--;
                        }
                    }
                }
            }
            catch
            {
            }
            Thread.Sleep(5000);
        }

        private bool ShouldSerializeServicesDependedOn()
        {
            if (servicesDependedOn != null && servicesDependedOn.Length != 0)
            {
                return true;
            }
            return false;
        }
    }
}
