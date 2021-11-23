using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace System.ServiceProcess
{
    /// <summary>Installs a class that extends <see cref="ServiceBase" /> to implement a service. This class is called by the install utility when installing a service application.</summary>
    public class ServiceInstaller : ComponentInstaller
    {
        private const string LocalServiceName = "NT AUTHORITY\\LocalService";
        private const string NetworkServiceName = "NT AUTHORITY\\NetworkService";
        private static bool _environmentChecked;
        private static bool _isWin9X;
        private readonly EventLogInstaller _eventLogInstaller;
        private bool _disposed;
        private string _serviceName = string.Empty;
        private ServiceStartMode _startType = ServiceStartMode.Manual;

        /// <summary>Initializes a new instance of the <see cref="ServiceInstaller" /> class.</summary>
        public ServiceInstaller()
        {
            // Create an EventLogInstaller and add it to our Installers collection to take
            // care of the service's EventLog property.
            _eventLogInstaller = new EventLogInstaller
            {
                Log = "Application",
                // we change these two later when our own properties are set.
                Source = "",
                UninstallAction = UninstallAction.Remove
            };

            Installers.Add(_eventLogInstaller);
        }

        /// <summary>Gets or sets a value that indicates whether the service should be delayed from starting until other automatically started services are running.</summary>
        /// <returns>true to delay automatic start of the service; otherwise, false. The default is false.</returns>
        [DefaultValue(false)]
        [ServiceProcessDescription(Res.ServiceProcessInstallerAccount)]
        public bool DelayedAutoStart { get; set; }

        /// <summary>Gets or sets the description for the service.</summary>
        /// <returns>The description of the service. The default is an empty string ("").</returns>
        [DefaultValue("")]
        [ComVisible(false)]
        [ServiceProcessDescription(Res.ServiceInstallerDescription)]
        public string Description { get; set; } = string.Empty;

        /// <summary>Indicates the friendly name that identifies the service to the user.</summary>
        /// <returns>The name associated with the service, used frequently for interactive tools.</returns>
        [DefaultValue("")]
        [ServiceProcessDescription(Res.ServiceInstallerDisplayName)]
        public string DisplayName { get; init; } = string.Empty;

        /// <summary>Indicates the name used by the system to identify this service. This property must be identical to the <see cref="ServiceBase.ServiceName" /> of the service you want to install.</summary>
        /// <returns>The name of the service to be installed. This value must be set before the install utility attempts to install the service.</returns>
        /// <exception cref="ArgumentException">The <see cref="ServiceName" /> property is invalid. </exception>
        [DefaultValue("")]
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [ServiceProcessDescription(Res.ServiceInstallerServiceName)]
        public string ServiceName
        {
            get => _serviceName;
            set
            {
                if (!ValidateServiceName(value))
                {
                    throw new ArgumentException(Res.GetString(Res.ServiceName, value, ServiceBase.MaxNameLength.ToString(CultureInfo.CurrentCulture)), nameof(value));
                }
                _serviceName = value;
                _eventLogInstaller.Source = value;
            }
        }

        /// <summary>Indicates the services that must be running for this service to run.</summary>
        /// <returns>An array of services that must be running before the service associated with this installer can run.</returns>
        [ServiceProcessDescription(Res.ServiceInstallerServicesDependedOn)]
        public string[] ServicesDependedOn { get; init; } = Array.Empty<string>();

        /// <summary>Indicates how and when this service is started.</summary>
        /// <returns>A <see cref="ServiceStartMode" /> that represents the way the service is started. The default is Manual, which specifies that the service will not automatically start after reboot.</returns>
        /// <exception cref="InvalidEnumArgumentException">The start mode is not a value of the <see cref="ServiceStartMode" /> enumeration.</exception>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        /// </PermissionSet>
        /// <exception cref="ArgumentException"></exception>
        [DefaultValue(ServiceStartMode.Manual)]
        [ServiceProcessDescription(Res.ServiceInstallerStartType)]
        public ServiceStartMode StartType
        {
            get => _startType;
            set
            {
                if (!Enum.IsDefined(typeof(ServiceStartMode), value))
                {
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(ServiceStartMode));
                }
                if (value is not 0 and not ServiceStartMode.System)
                {
                    _startType = value;
                    return;
                }
                throw new ArgumentException(Res.GetString(Res.ServiceStartType, value), nameof(value));
            }
        }

        /// <summary>Copies properties from an instance of <see cref="ServiceBase" /> to this installer.</summary>
        /// <param name="component">The <see cref="IComponent" /> from which to copy. </param>
        /// <exception cref="ArgumentException">The component you are associating with this installer does not inherit from <see cref="ServiceBase" />. </exception>
        public override void CopyFromComponent(IComponent component)
        {
            if (component is not ServiceBase serviceBase)
            {
                throw new ArgumentException(Res.GetString(Res.NotAService), nameof(component));
            }

            ServiceName = serviceBase.ServiceName;
        }

        /// <summary>Installs the service by writing service application information to the registry. This method is meant to be used by installation tools, which process the appropriate methods automatically.</summary>
        /// <param name="stateSaver">An <see cref="IDictionary" /> that contains the context information associated with the installation. </param>
        /// <exception cref="InvalidOperationException">The installation does not contain a <see cref="ServiceProcessInstaller" /> for the executable.-or- The file name for the assembly is null or an empty string.-or- The service name is invalid.-or- The Service Control Manager could not be opened. </exception>
        /// <exception cref="ArgumentException">The display name for the service is more than 255 characters in length.</exception>
        /// <exception cref="Win32Exception">The system could not generate a handle to the service. -or-A service with that name is already installed.</exception>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
        ///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        public override void Install(IDictionary stateSaver)
        {
            Context?.LogMessage(Res.GetString(Res.InstallingService, ServiceName));
            try
            {
                CheckEnvironment();
                string? username = null;
                string? password = null;
                // find the ServiceProcessInstaller for our process. It's either the
                // parent or one of our peers in the parent's Installers collection.
                ServiceProcessInstaller? processInstaller = null;
                if (Parent is ServiceProcessInstaller serviceProcessInstaller)
                {
                    processInstaller = serviceProcessInstaller;
                }
                else if (Parent?.Installers != null)
                {
                    foreach (var installer in Parent.Installers)
                    {
                        if (installer is ServiceProcessInstaller serviceProcessInstaller3)
                        {
                            processInstaller = serviceProcessInstaller3;
                            break;
                        }
                    }
                }
                if (processInstaller == null)
                {
                    throw new InvalidOperationException(Res.GetString(Res.NoInstaller));
                }
                switch (processInstaller.Account)
                {
                    case ServiceAccount.LocalService:
                        username = LocalServiceName;
                        break;

                    case ServiceAccount.NetworkService:
                        username = NetworkServiceName;
                        break;

                    case ServiceAccount.User:
                        username = processInstaller.Username;
                        password = processInstaller.Password;
                        break;
                }

                // check all our parameters
                var moduleFileName = Context?.Parameters["assemblypath"];
                if (string.IsNullOrEmpty(moduleFileName))
                {
                    throw new InvalidOperationException(Res.GetString(Res.FileName));
                }

                // Put quotas around module file name. Otherwise a service might fail to start if there is space in the path.
                // Note: Though CreateService accepts a binaryPath allowing
                // arguments for automatic services, in /assemblypath=foo,
                // foo is simply the path to the executable.
                // Therefore, it is best to quote if there are no quotes,
                // and best to not quote if there are quotes.
                if (!moduleFileName.Contains('"', StringComparison.OrdinalIgnoreCase))
                {
                    moduleFileName = "\"" + moduleFileName + "\"";
                }
                if (!ValidateServiceName(ServiceName))
                {
                    // Event Log cannot be used here, since the service doesn't exist yet.
                    throw new InvalidOperationException(Res.GetString(Res.ServiceName, ServiceName, ServiceBase.MaxNameLength.ToString(CultureInfo.CurrentCulture)));
                }

                // Check DisplayName length.
                if (DisplayName.Length > 255)
                {
                    // MSDN suggests that 256 is the max length, but in
                    // fact anything over 255 causes problems.
                    throw new ArgumentException(Res.GetString(Res.DisplayNameTooLong, DisplayName));
                }

                string? servicesDependedOn = null;
                if (ServicesDependedOn.Length != 0)
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var serviceName in ServicesDependedOn)
                    {
                        // we have to build a list of the services' short names. But the user
                        // might have used long names in the ServicesDependedOn property. Try
                        // to use ServiceController's logic to get the short name.
                        var tempServiceName = serviceName;
                        try
                        {
                            tempServiceName = new ServiceController(tempServiceName, ".").ServiceName;
                        }
                        catch
                        {
                            // Ignore
                        }
                        //The servicesDependedOn need to be separated by a null
                        stringBuilder.Append(tempServiceName).Append('\0');
                    }
                    stringBuilder.Append('\0');
                    servicesDependedOn = stringBuilder.ToString();
                }

                // Open the service manager
                var serviceManagerHandle = SafeNativeMethods.OpenSCManager(null, null, (uint)NativeMethods.SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
                var serviceHandle = IntPtr.Zero;
                if (serviceManagerHandle == IntPtr.Zero)
                {
                    throw new InvalidOperationException(Res.GetString(Res.OpenSC, "."), new Win32Exception());
                }
                var serviceType = 16;
                // count the number of UserNTServiceInstallers. More than one means we set the SHARE_PROCESS flag.
                var serviceInstallerCount = 0;
                if (Parent?.Installers != null)
                {
                    foreach (var installer in Parent.Installers)
                    {
                        if (installer is ServiceInstaller)
                        {
                            serviceInstallerCount++;
                            if (serviceInstallerCount > 1)
                            {
                                break;
                            }
                        }
                    }
                }

                if (serviceInstallerCount > 1)
                {
                    serviceType = 32;
                }
                try
                {
                    // Install the service
                    serviceHandle = NativeMethods.CreateService(serviceManagerHandle, ServiceName, DisplayName, 983551, serviceType, (int)StartType, 1, moduleFileName, null, IntPtr.Zero, servicesDependedOn, username, password);
                    if (serviceHandle == IntPtr.Zero)
                    {
                        throw new Win32Exception();
                    }

                    // A local variable in an unsafe method is already fixed -- so we don't need a "fixed { }" blocks to protect
                    // across the p/invoke calls below.

                    if (Description.Length != 0)
                    {
                        var serviceDescription = default(NativeMethods.SERVICE_DESCRIPTION);
                        serviceDescription.description = Marshal.StringToHGlobalUni(Description);
                        var success = NativeMethods.ChangeServiceConfig2(serviceHandle, 1u, ref serviceDescription);
                        Marshal.FreeHGlobal(serviceDescription.description);
                        if (!success)
                        {
                            throw new Win32Exception();
                        }
                    }
                    if (Environment.OSVersion.Version.Major > 5 && StartType == ServiceStartMode.Automatic)
                    {
                        var serviceDelayedAutoStartInfo = default(NativeMethods.SERVICE_DELAYED_AUTOSTART_INFO);
                        serviceDelayedAutoStartInfo.fDelayedAutostart = DelayedAutoStart;
                        if (!NativeMethods.ChangeServiceConfig2(serviceHandle, 3u, ref serviceDelayedAutoStartInfo))
                        {
                            throw new Win32Exception();
                        }
                    }
                    stateSaver["installed"] = true;
                }
                finally
                {
                    if (serviceHandle != IntPtr.Zero)
                    {
                        SafeNativeMethods.CloseServiceHandle(serviceHandle);
                    }
                    SafeNativeMethods.CloseServiceHandle(serviceManagerHandle);
                }
                Context?.LogMessage(Res.GetString(Res.InstallOK, ServiceName));
            }
            finally
            {
                base.Install(stateSaver);
            }
        }

        /// <summary>Indicates whether two installers would install the same service.</summary>
        /// <returns>true if calling <see cref="Install(IDictionary)" /> on both of these installers would result in installing the same service; otherwise, false.</returns>
        /// <param name="otherInstaller">A <see cref="ComponentInstaller" /> to which you are comparing the current installer. </param>
        public override bool IsEquivalentInstaller(ComponentInstaller otherInstaller) =>
            otherInstaller is ServiceInstaller serviceInstaller && string.Equals(serviceInstaller.ServiceName, ServiceName, StringComparison.OrdinalIgnoreCase);

        /// <summary>Uninstalls the service by removing information about it from the registry.</summary>
        /// <param name="savedState">An <see cref="IDictionary" /> that contains the context information associated with the installation. </param>
        /// <exception cref="Win32Exception">The Service Control Manager could not be opened.-or- The system could not get a handle to the service. </exception>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
        ///   <IPermission class="System.ServiceProcess.ServiceControllerPermission, System.ServiceProcess, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
        /// </PermissionSet>
        public override void Uninstall(IDictionary? savedState)
        {
            base.Uninstall(savedState);
            RemoveService();
        }

        internal static void CheckEnvironment()
        {
            if (_environmentChecked)
            {
                if (!_isWin9X)
                {
                    return;
                }
                throw new PlatformNotSupportedException(Res.GetString(Res.CantControlOnWin9x));
            }
            _isWin9X = Environment.OSVersion.Platform != PlatformID.Win32NT;
            _environmentChecked = true;
            if (!_isWin9X)
            {
                return;
            }
            throw new PlatformNotSupportedException(Res.GetString(Res.CantInstallOnWin9x));
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (disposing)
            {
                _eventLogInstaller.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>Rolls back service application information written to the registry by the installation procedure. This method is meant to be used by installation tools, which process the appropriate methods automatically.</summary>
        /// <param name="savedState">An <see cref="IDictionary" /> that contains the context information associated with the installation. </param>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
        ///   <IPermission class="System.ServiceProcess.ServiceControllerPermission, System.ServiceProcess, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
        /// </PermissionSet>
        protected override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
            var obj = savedState["installed"];
            if (obj != null && (bool)obj)
            {
                RemoveService();
            }
        }

        protected virtual void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private static bool ValidateServiceName(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length > 80)
            {
                return false;
            }

            return name.All(t => t is not (< ' ' or '/' or '\\'));
        }

        private void RemoveService()
        {
            //
            // SCUM deletes a service when the Service is stopped and there is no open handle to the Service.
            // Service will be deleted asynchronously, so it takes a while for the deletion to be complete.
            // The recommended way to delete a Service is:
            // (a)  DeleteService/closehandle,
            // (b) Stop service & wait until it is stopped & close handle
            // (c)  Wait for 5-10 secs for the async deletion to go through.
            //
            Context?.LogMessage(Res.GetString(Res.ServiceRemoving, ServiceName));
            var serviceManagerHandle = SafeNativeMethods.OpenSCManager(null, null, (uint)NativeMethods.SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
            if (serviceManagerHandle == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            var serviceHandle = IntPtr.Zero;
            try
            {
                serviceHandle = NativeMethods.OpenService(serviceManagerHandle, ServiceName, 65536);
                if (serviceHandle == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }
                NativeMethods.DeleteService(serviceHandle);
            }
            finally
            {
                if (serviceHandle != IntPtr.Zero)
                {
                    SafeNativeMethods.CloseServiceHandle(serviceHandle);
                }
                SafeNativeMethods.CloseServiceHandle(serviceManagerHandle);
            }
            Context?.LogMessage(Res.GetString(Res.ServiceRemoved, ServiceName));

            // Stop the service
            try
            {
                using var serviceController = new ServiceController(ServiceName);
                if (serviceController.Status != ServiceControllerStatus.Stopped)
                {
                    Context?.LogMessage(Res.GetString(Res.TryToStop, ServiceName));
                    serviceController.Stop();
                    var timeout = 10;
                    serviceController.Refresh();
                    while (serviceController.Status != ServiceControllerStatus.Stopped && timeout > 0)
                    {
                        Thread.Sleep(1000);
                        serviceController.Refresh();
                        timeout--;
                    }
                }
            }
            catch
            {
                // Ignore
            }
            Thread.Sleep(5000);
        }
    }
}
