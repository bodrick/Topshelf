using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace System.Diagnostics
{
    public class EventLogInstaller : ComponentInstaller
    {
        private readonly EventSourceCreationData _sourceData = new(null, null);
        private readonly UninstallAction _uninstallAction;

        /// <summary>Gets or sets the number of categories in the category resource file.</summary>
        /// <returns>The number of categories in the category resource file. The default value is zero.</returns>
        [ComVisible(false)]
        [ResDescription(Res.Desc_CategoryCount)]
        public int CategoryCount
        {
            get => _sourceData.CategoryCount;
            set => _sourceData.CategoryCount = value;
        }

        /// <summary>Gets or sets the path of the resource file that contains category strings for the source.</summary>
        /// <returns>The path of the category resource file. The default is an empty string ("").</returns>
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [Editor("System.Windows.Forms.Design.FileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [ComVisible(false)]
        [ResDescription(Res.Desc_CategoryResourceFile)]
        public string CategoryResourceFile
        {
            get => _sourceData.CategoryResourceFile;
            set => _sourceData.CategoryResourceFile = value;
        }

        /// <summary>Gets or sets the name of the log to set the source to.</summary>
        /// <returns>The name of the log. This can be Application, System, or a custom log name. The default is an empty string ("").</returns>
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [ResDescription(Res.Desc_Log)]
        public string Log
        {
            get
            {
                if (_sourceData.LogName == null && _sourceData.Source != null)
                {
                    _sourceData.LogName = EventLog.LogNameFromSourceName(_sourceData.Source, ".");
                }
                return _sourceData.LogName ?? string.Empty;
            }
            set => _sourceData.LogName = value;
        }

        /// <summary>Gets or sets the path of the resource file that contains message formatting strings for the source.</summary>
        /// <returns>The path of the message resource file. The default is an empty string ("").</returns>
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [Editor("System.Windows.Forms.Design.FileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [ComVisible(false)]
        [ResDescription(Res.Desc_MessageResourceFile)]
        public string MessageResourceFile
        {
            get => _sourceData.MessageResourceFile;
            set => _sourceData.MessageResourceFile = value;
        }

        /// <summary>Gets or sets the path of the resource file that contains message parameter strings for the source.</summary>
        /// <returns>The path of the message parameter resource file. The default is an empty string ("").</returns>
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [Editor("System.Windows.Forms.Design.FileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [ComVisible(false)]
        [ResDescription(Res.Desc_ParameterResourceFile)]
        public string ParameterResourceFile
        {
            get => _sourceData.ParameterResourceFile;
            set => _sourceData.ParameterResourceFile = value;
        }

        /// <summary>Gets or sets the source name to register with the log.</summary>
        /// <returns>The name to register with the event log as a source of entries. The default is an empty string ("").</returns>
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [ResDescription("Desc_Source")]
        public string Source
        {
            get => _sourceData.Source;
            set => _sourceData.Source = value;
        }

        /// <summary>Gets or sets a value that indicates whether the Installutil.exe (Installer Tool) should remove the event log or leave it in its installed state at uninstall time.</summary>
        /// <returns>One of the <see cref="Diagnostics.UninstallAction" /> values that indicates what state to leave the event log in when the <see cref="EventLog" /> is uninstalled. The default is Remove.</returns>
        /// <exception cref="InvalidEnumArgumentException">
        ///   <see cref="UninstallAction" /> contains an invalid value. The only valid values for this property are Remove and NoAction.</exception>
        [DefaultValue(UninstallAction.Remove)]
        [ResDescription(Res.Desc_UninstallAction)]
        public UninstallAction UninstallAction
        {
            get => _uninstallAction;
            init
            {
                if (!Enum.IsDefined(typeof(UninstallAction), value))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(UninstallAction));
                }
                _uninstallAction = value;
            }
        }

        /// <summary>Copies the property values of an <see cref="EventLog" /> component that are required at installation time for an event log.</summary>
        /// <param name="component">An <see cref="IComponent" /> to use as a template for the <see cref="EventLogInstaller" />. </param>
        /// <exception cref="ArgumentException">The specified component is not an <see cref="EventLog" />.-or- The <see cref="EventLog.Log" /> or <see cref="EventLog.Source" /> property of the specified component is either null or empty. </exception>
        public override void CopyFromComponent(IComponent component)
        {
            if (component is not EventLog eventLog)
            {
                throw new ArgumentException(Res.GetString(Res.NotAnEventLog), nameof(component));
            }

            if (string.IsNullOrEmpty(eventLog.Log) || string.IsNullOrEmpty(eventLog.Source))
            {
                throw new ArgumentException(Res.GetString(Res.IncompleteEventLog), nameof(component));
            }

            Log = eventLog.Log;
            Source = eventLog.Source;
        }

        /// <summary>Performs the installation and writes event log information to the registry.</summary>
        /// <param name="stateSaver">An <see cref="IDictionary" /> used to save information needed to perform a rollback or uninstall operation. </param>
        /// <exception cref="PlatformNotSupportedException">The platform the installer is trying to use is not Windows NT 4.0 or later. </exception>
        /// <exception cref="ArgumentException">The name specified in the <see cref="Source" />  property is already registered for a different event log.</exception>
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            Context?.LogMessage(Res.GetString(Res.CreatingEventLog, Source, Log));
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new PlatformNotSupportedException(Res.GetString(Res.WinNTRequired));
            }
            stateSaver["baseInstalledAndPlatformOK"] = true;
            stateSaver["logExists"] = EventLog.Exists(Log, ".");
            var alreadyRegistered = EventLog.SourceExists(Source, ".");
            stateSaver["alreadyRegistered"] = alreadyRegistered;
            if (alreadyRegistered && string.Equals(EventLog.LogNameFromSourceName(Source, "."), Log, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            EventLog.CreateEventSource(_sourceData);
        }

        /// <summary>Determines whether an installer and another specified installer refer to the same source.</summary>
        /// <returns>true if this installer and the installer specified by the <paramref name="otherInstaller" /> parameter would install or uninstall the same source; otherwise, false.</returns>
        /// <param name="otherInstaller">The installer to compare. </param>
        public override bool IsEquivalentInstaller(ComponentInstaller otherInstaller) =>
            otherInstaller is EventLogInstaller eventLogInstaller && string.Equals(eventLogInstaller.Source, Source, StringComparison.OrdinalIgnoreCase);

        /// <summary>Removes an installation by removing event log information from the registry.</summary>
        /// <param name="savedState">An <see cref="IDictionary" /> that contains the pre-installation state of the computer. </param>
        public override void Uninstall(IDictionary? savedState)
        {
            base.Uninstall(savedState);
            if (UninstallAction != UninstallAction.Remove)
            {
                return;
            }

            Context?.LogMessage(Res.GetString(Res.RemovingEventLog, Source));
            if (EventLog.SourceExists(Source, "."))
            {
                if (!string.Equals(Log, Source, StringComparison.OrdinalIgnoreCase))
                {
                    EventLog.DeleteEventSource(Source, ".");
                }
            }
            else
            {
                Context?.LogMessage(Res.GetString(Res.LocalSourceNotRegisteredWarning, Source));
            }

            using var key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\EventLog", false);
            using var logKey = key?.OpenSubKey(Log, false);
            if (logKey == null)
            {
                return;
            }

            var keyNames = logKey.GetSubKeyNames();
            if (keyNames.Length == 0 || (keyNames.Length == 1 && string.Equals(keyNames[0], Log, StringComparison.OrdinalIgnoreCase)))
            {
                Context?.LogMessage(Res.GetString(Res.DeletingEventLog, Log));
                EventLog.Delete(Log, ".");
            }
        }

        /// <summary>Restores the computer to the state it was in before the installation by rolling back the event log information that the installation procedure wrote to the registry.</summary>
        /// <param name="savedState">An <see cref="IDictionary" /> that contains the pre-installation state of the computer. </param>
        protected override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
            Context?.LogMessage(Res.GetString(Res.RestoringEventLog, Source));
            if (savedState["baseInstalledAndPlatformOK"] == null)
            {
                return;
            }

            var logExists = (bool)(savedState["logExists"] ?? false);
            if (!logExists)
            {
                EventLog.Delete(Log, ".");
            }
            else
            {
                bool alreadyRegistered;
                var alreadyRegisteredObj = savedState["alreadyRegistered"];
                if (alreadyRegisteredObj == null)
                {
                    alreadyRegistered = false;
                }
                else
                {
                    alreadyRegistered = (bool)alreadyRegisteredObj;
                }

                if (!alreadyRegistered && EventLog.SourceExists(Source, "."))
                {
                    // delete the source we installed, assuming it succeeded. Then put back whatever used to be there.
                    EventLog.DeleteEventSource(Source, ".");
                }
            }
        }
    }
}
