using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace System.Diagnostics
{
    public class EventLogInstaller : ComponentInstaller
    {
        private readonly EventSourceCreationData sourceData = new EventSourceCreationData(null, null);

        private UninstallAction uninstallAction;

        /// <summary>Gets or sets the number of categories in the category resource file.</summary>
        /// <returns>The number of categories in the category resource file. The default value is zero.</returns>
        [ComVisible(false)]
        [ResDescription("Desc_CategoryCount")]
        public int CategoryCount
        {
            get => sourceData.CategoryCount;
            set => sourceData.CategoryCount = value;
        }

        /// <summary>Gets or sets the path of the resource file that contains category strings for the source.</summary>
        /// <returns>The path of the category resource file. The default is an empty string ("").</returns>
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [Editor("System.Windows.Forms.Design.FileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [ComVisible(false)]
        [ResDescription("Desc_CategoryResourceFile")]
        public string CategoryResourceFile
        {
            get => sourceData.CategoryResourceFile;
            set => sourceData.CategoryResourceFile = value;
        }

        /// <summary>Gets or sets the name of the log to set the source to.</summary>
        /// <returns>The name of the log. This can be Application, System, or a custom log name. The default is an empty string ("").</returns>
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [ResDescription("Desc_Log")]
        public string Log
        {
            get
            {
                if (sourceData.LogName == null && sourceData.Source != null)
                {
                    sourceData.LogName = EventLog.LogNameFromSourceName(sourceData.Source, ".");
                }
                return sourceData.LogName;
            }
            set => sourceData.LogName = value;
        }

        /// <summary>Gets or sets the path of the resource file that contains message formatting strings for the source.</summary>
        /// <returns>The path of the message resource file. The default is an empty string ("").</returns>
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [Editor("System.Windows.Forms.Design.FileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [ComVisible(false)]
        [ResDescription("Desc_MessageResourceFile")]
        public string MessageResourceFile
        {
            get => sourceData.MessageResourceFile;
            set => sourceData.MessageResourceFile = value;
        }

        /// <summary>Gets or sets the path of the resource file that contains message parameter strings for the source.</summary>
        /// <returns>The path of the message parameter resource file. The default is an empty string ("").</returns>
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [Editor("System.Windows.Forms.Design.FileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [ComVisible(false)]
        [ResDescription("Desc_ParameterResourceFile")]
        public string ParameterResourceFile
        {
            get => sourceData.ParameterResourceFile;
            set => sourceData.ParameterResourceFile = value;
        }

        /// <summary>Gets or sets the source name to register with the log.</summary>
        /// <returns>The name to register with the event log as a source of entries. The default is an empty string ("").</returns>
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [ResDescription("Desc_Source")]
        public string Source
        {
            get => sourceData.Source;
            set => sourceData.Source = value;
        }

        /// <summary>Gets or sets a value that indicates whether the Installutil.exe (Installer Tool) should remove the event log or leave it in its installed state at uninstall time.</summary>
        /// <returns>One of the <see cref="T:System.Configuration.Install.UninstallAction" /> values that indicates what state to leave the event log in when the <see cref="T:System.Diagnostics.EventLog" /> is uninstalled. The default is Remove.</returns>
        /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
        ///   <see cref="P:System.Diagnostics.EventLogInstaller.UninstallAction" /> contains an invalid value. The only valid values for this property are Remove and NoAction.</exception>
        [DefaultValue(UninstallAction.Remove)]
        [ResDescription("Desc_UninstallAction")]
        public UninstallAction UninstallAction
        {
            get => uninstallAction;
            set
            {
                if (!Enum.IsDefined(typeof(UninstallAction), value))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(UninstallAction));
                }
                uninstallAction = value;
            }
        }

        /// <summary>Copies the property values of an <see cref="T:System.Diagnostics.EventLog" /> component that are required at installation time for an event log.</summary>
        /// <param name="component">An <see cref="T:System.ComponentModel.IComponent" /> to use as a template for the <see cref="T:System.Diagnostics.EventLogInstaller" />. </param>
        /// <exception cref="T:System.ArgumentException">The specified component is not an <see cref="T:System.Diagnostics.EventLog" />.-or- The <see cref="P:System.Diagnostics.EventLog.Log" /> or <see cref="P:System.Diagnostics.EventLog.Source" /> property of the specified component is either null or empty. </exception>
        public override void CopyFromComponent(IComponent component)
        {
            var eventLog = component as EventLog;
            if (eventLog == null)
            {
                throw new ArgumentException(Res.GetString("NotAnEventLog"));
            }
            if (eventLog.Log != null && !(eventLog.Log == string.Empty) && eventLog.Source != null && !(eventLog.Source == string.Empty))
            {
                Log = eventLog.Log;
                Source = eventLog.Source;
                return;
            }
            throw new ArgumentException(Res.GetString("IncompleteEventLog"));
        }

        /// <summary>Performs the installation and writes event log information to the registry.</summary>
        /// <param name="stateSaver">An <see cref="T:System.Collections.IDictionary" /> used to save information needed to perform a rollback or uninstall operation. </param>
        /// <exception cref="T:System.PlatformNotSupportedException">The platform the installer is trying to use is not Windows NT 4.0 or later. </exception>
        /// <exception cref="T:System.ArgumentException">The name specified in the <see cref="P:System.Diagnostics.EventLogInstaller.Source" />  property is already registered for a different event log.</exception>
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            base.Context.LogMessage(Res.GetString("CreatingEventLog", Source, Log));
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new PlatformNotSupportedException(Res.GetString("WinNTRequired"));
            }
            stateSaver["baseInstalledAndPlatformOK"] = true;
            var flag = EventLog.Exists(Log, ".");
            stateSaver["logExists"] = flag;
            var flag2 = EventLog.SourceExists(Source, ".");
            stateSaver["alreadyRegistered"] = flag2;
            if (flag2 && EventLog.LogNameFromSourceName(Source, ".") == Log)
            {
                return;
            }
            EventLog.CreateEventSource(sourceData);
        }

        /// <summary>Determines whether an installer and another specified installer refer to the same source.</summary>
        /// <returns>true if this installer and the installer specified by the <paramref name="otherInstaller" /> parameter would install or uninstall the same source; otherwise, false.</returns>
        /// <param name="otherInstaller">The installer to compare. </param>
        public override bool IsEquivalentInstaller(ComponentInstaller otherInstaller)
        {
            var eventLogInstaller = otherInstaller as EventLogInstaller;
            if (eventLogInstaller == null)
            {
                return false;
            }
            return eventLogInstaller.Source == Source;
        }

        /// <summary>Restores the computer to the state it was in before the installation by rolling back the event log information that the installation procedure wrote to the registry.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the pre-installation state of the computer. </param>
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
            base.Context.LogMessage(Res.GetString("RestoringEventLog", Source));
            if (savedState["baseInstalledAndPlatformOK"] != null)
            {
                if (!(bool)savedState["logExists"])
                {
                    EventLog.Delete(Log, ".");
                }
                else
                {
                    var obj = savedState["alreadyRegistered"];
                    var flag = obj != null && (bool)obj;
                    if (!flag && EventLog.SourceExists(Source, "."))
                    {
                        EventLog.DeleteEventSource(Source, ".");
                    }
                }
            }
        }

        /// <summary>Removes an installation by removing event log information from the registry.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the pre-installation state of the computer. </param>
        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            if (UninstallAction == UninstallAction.Remove)
            {
                base.Context.LogMessage(Res.GetString("RemovingEventLog", Source));
                if (EventLog.SourceExists(Source, "."))
                {
                    if (string.Compare(Log, Source, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        EventLog.DeleteEventSource(Source, ".");
                    }
                }
                else
                {
                    base.Context.LogMessage(Res.GetString("LocalSourceNotRegisteredWarning", Source));
                }
                var registryKey = Registry.LocalMachine;
                RegistryKey registryKey2 = null;
                try
                {
                    registryKey = registryKey.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\EventLog", false);
                    if (registryKey != null)
                    {
                        registryKey2 = registryKey.OpenSubKey(Log, false);
                    }
                    if (registryKey2 != null)
                    {
                        var subKeyNames = registryKey2.GetSubKeyNames();
                        if (subKeyNames == null || subKeyNames.Length == 0 || (subKeyNames.Length == 1 && string.Compare(subKeyNames[0], Log, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            base.Context.LogMessage(Res.GetString("DeletingEventLog", Log));
                            EventLog.Delete(Log, ".");
                        }
                    }
                }
                finally
                {
                    if (registryKey != null)
                    {
                        registryKey.Close();
                    }
                    if (registryKey2 != null)
                    {
                        registryKey2.Close();
                    }
                }
            }
        }
    }
}
