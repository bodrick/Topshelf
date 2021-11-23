using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace System.Configuration.Install
{
    public class Installer : Component
    {
        private const string WrappedExceptionSource = Res.WrappedExceptionSource;
        private EventHandler<InstallEventArgs>? _afterCommitHandler;
        private EventHandler<InstallEventArgs>? _afterInstallHandler;
        private EventHandler<InstallEventArgs>? _afterRollbackHandler;
        private EventHandler<InstallEventArgs>? _afterUninstallHandler;
        private EventHandler<InstallEventArgs>? _beforeCommitHandler;
        private EventHandler<InstallEventArgs>? _beforeInstallHandler;
        private EventHandler<InstallEventArgs>? _beforeRollbackHandler;
        private EventHandler<InstallEventArgs>? _beforeUninstallHandler;
        private InstallerCollection? _installers;
        private Installer? _parent;

        /// <summary>Occurs after the <see cref="Install(IDictionary)" /> methods of all the installers in the <see cref="Installers" /> property have run.</summary>
        public event EventHandler<InstallEventArgs> AfterInstall
        {
            add => _afterInstallHandler += value;
            remove => _afterInstallHandler -= value;
        }

        /// <summary>Occurs after the installations of all the installers in the <see cref="Installers" /> property are rolled back.</summary>
        public event EventHandler<InstallEventArgs> AfterRollback
        {
            add => _afterRollbackHandler += value;
            remove => _afterRollbackHandler -= value;
        }

        /// <summary>Occurs after all the installers in the <see cref="Installers" /> property perform their uninstallation operations.</summary>
        public event EventHandler<InstallEventArgs> AfterUninstall
        {
            add => _afterUninstallHandler += value;
            remove => _afterUninstallHandler -= value;
        }

        /// <summary>Occurs before the <see cref="Install(IDictionary)" /> method of each installer in the installer collection has run.</summary>
        public event EventHandler<InstallEventArgs> BeforeInstall
        {
            add => _beforeInstallHandler += value;
            remove => _beforeInstallHandler -= value;
        }

        /// <summary>Occurs before the installers in the <see cref="Installers" /> property are rolled back.</summary>
        public event EventHandler<InstallEventArgs> BeforeRollback
        {
            add => _beforeRollbackHandler += value;
            remove => _beforeRollbackHandler -= value;
        }

        /// <summary>Occurs before the installers in the <see cref="Installers" /> property perform their uninstall operations.</summary>
        public event EventHandler<InstallEventArgs> BeforeUninstall
        {
            add => _beforeUninstallHandler += value;
            remove => _beforeUninstallHandler -= value;
        }

        public event EventHandler<InstallEventArgs> Committed
        {
            add => _afterCommitHandler += value;
            remove => _afterCommitHandler -= value;
        }

        /// <summary>Occurs before the installers in the <see cref="Installers" /> property commit their installations.</summary>
        public event EventHandler<InstallEventArgs> Committing
        {
            add => _beforeCommitHandler += value;
            remove => _beforeCommitHandler -= value;
        }

        public InstallContext? Context { get; set; }

        public InstallerCollection Installers => _installers ??= new InstallerCollection(this);

        [TypeConverter(typeof(InstallerParentConverter))]
        [ResDescription("Desc_Installer_Parent")]
        public Installer? Parent
        {
            get => _parent;
            set
            {
                if (value == this)
                {
                    throw new InvalidOperationException(Res.GetString(Res.InstallBadParent));
                }

                if (value == _parent)
                {
                    // nothing to do
                    return;
                }

                if (value != null && InstallerTreeContains(value))
                {
                    throw new InvalidOperationException(Res.GetString(Res.InstallRecursiveParent));
                }

                if (_parent != null)
                {
                    var index = _parent.Installers.IndexOf(this);
                    if (index != -1)
                    {
                        _parent.Installers.RemoveAt(index);
                    }
                }

                _parent = value;
                if (_parent?.Installers.Contains(this) == false)
                {
                    _parent.Installers.Add(this);
                }
            }
        }

        [ResDescription("Desc_Installer_HelpText")]
        protected virtual string HelpText
        {
            get
            {
                var stringBuilder = new StringBuilder();
                foreach (var helpText in Installers.Select(f => f.HelpText))
                {
                    if (helpText.Length > 0)
                    {
                        stringBuilder.Append("\r\n").Append(helpText);
                    }
                }
                return stringBuilder.ToString();
            }
        }

        public virtual void Install(IDictionary stateSaver)
        {
            if (stateSaver == null)
            {
                throw new ArgumentNullException(nameof(stateSaver));
            }
            try
            {
                OnBeforeInstall(stateSaver);
            }
            catch (Exception ex)
            {
                WriteEventHandlerError(Res.GetString(Res.InstallSeverityError), "OnBeforeInstall", ex);
                throw new InvalidOperationException(Res.GetString(Res.InstallEventException, "OnBeforeInstall", GetType().FullName ?? string.Empty), ex);
            }
            var lastInstallerAttempted = -1;
            var savedStates = new List<IDictionary>();
            try
            {
                foreach (var installer in Installers)
                {
                    // Pass down our context to each of the contained installers.
                    // We set all of the contexts before calling any installers just
                    // in case one contained installer gets another to examine its
                    // context before installation (as in the case of ServiceInstallers).
                    installer.Context = Context;
                }

                for (var j = 0; j < Installers.Count; j++)
                {
                    var installer = Installers[j];
                    // each contained installer gets a new IDictionary to write to. This way
                    // there can be no name conflicts between installers.
                    IDictionary nestedStateSaver = new Hashtable();
                    try
                    {
                        lastInstallerAttempted = j;
                        installer.Install(nestedStateSaver);
                    }
                    finally
                    {
                        savedStates.Add(nestedStateSaver);
                    }
                }
            }
            finally
            {
                stateSaver.Add("_reserved_lastInstallerAttempted", lastInstallerAttempted);
                stateSaver.Add("_reserved_nestedSavedStates", savedStates.ToArray());
            }
            try
            {
                OnAfterInstall(stateSaver);
            }
            catch (Exception e)
            {
                WriteEventHandlerError(Res.GetString(Res.InstallSeverityError), "OnAfterInstall", e);
                throw new InvalidOperationException(Res.GetString(Res.InstallEventException, "OnAfterInstall", GetType().FullName ?? string.Empty), e);
            }
        }

        public virtual void Uninstall(IDictionary? savedState)
        {
            Exception? savedException = null;
            try
            {
                OnBeforeUninstall(savedState);
            }
            catch (Exception e)
            {
                WriteEventHandlerError(Res.GetString(Res.InstallSeverityWarning), "OnBeforeUninstall", e);
                Context?.LogMessage(Res.GetString(Res.InstallUninstallException));
                savedException = e;
            }

            // do the uninstall

            // uninstall is special: savedState can be null. (The state file may have been deleted since
            // the application was installed.) If it isn't, we read state out as usual. Otherwise we create
            // some new, empty state to pass to the contained installers.
            IDictionary[]? nestedSavedStates;
            if (savedState != null)
            {
                nestedSavedStates = savedState["_reserved_nestedSavedStates"] as IDictionary[];
                if (nestedSavedStates == null || nestedSavedStates.Length != Installers.Count)
                {
                    throw new ArgumentException(Res.GetString(Res.InstallDictionaryCorrupted, "savedState"), nameof(savedState));
                }
            }
            else
            {
                nestedSavedStates = new IDictionary[Installers.Count];
            }
            //go in reverse order when uninstalling
            for (var i = Installers.Count - 1; i >= 0; i--)
            {
                // set all the contexts first.  see note in Install
                Installers[i].Context = Context;
            }

            for (var i = Installers.Count - 1; i >= 0; i--)
            {
                try
                {
                    Installers[i].Uninstall(nestedSavedStates[i]);
                }
                catch (Exception e)
                {
                    if (!IsWrappedException(e))
                    {
                        // only print the message if this is not a wrapper around an exception we already printed out.
                        Context?.LogMessage(Res.GetString(Res.InstallLogUninstallException, Installers[i].ToString()));
                        LogException(e, Context);
                        Context?.LogMessage(Res.GetString(Res.InstallUninstallException));
                    }
                    savedException = e;
                }
            }

            // raise the OnAfterUninstall event
            try
            {
                OnAfterUninstall(savedState);
            }
            catch (Exception e)
            {
                WriteEventHandlerError(Res.GetString(Res.InstallSeverityWarning), "OnAfterUninstall", e);
                Context?.LogMessage(Res.GetString(Res.InstallUninstallException));
                savedException = e;
            }

            if (savedException == null)
            {
                return;
            }

            var wrappedException = savedException;
            if (!IsWrappedException(savedException))
            {
                wrappedException = new InstallException(Res.GetString(Res.InstallUninstallException), savedException)
                {
                    Source = WrappedExceptionSource
                };
            }
            throw wrappedException;
        }

        internal static void LogException(Exception e, InstallContext? context)
        {
            var toplevel = true;
            while (e != null)
            {
                if (toplevel)
                {
                    context?.LogMessage(e.GetType().FullName + ": " + e.Message);
                    toplevel = false;
                }
                else
                {
                    context?.LogMessage(Res.GetString(Res.InstallLogInner, e.GetType().FullName ?? string.Empty, e.Message));
                }

                if (context?.IsParameterTrue("showcallstack") == true)
                {
                    context.LogMessage(e.StackTrace);
                }

                if (e.InnerException != null)
                {
                    e = e.InnerException;
                }
            }
        }

        protected void Commit(IDictionary savedState)
        {
            if (savedState == null)
            {
                throw new ArgumentException(Res.GetString(Res.InstallNullParameter, "savedState"), nameof(savedState));
            }

            if (savedState["_reserved_lastInstallerAttempted"] == null || savedState["_reserved_nestedSavedStates"] == null)
            {
                throw new ArgumentException(Res.GetString(Res.InstallDictionaryMissingValues, "savedState"), nameof(savedState));
            }

            Exception? savedException = null;
            try
            {
                OnCommitting(savedState);
            }
            catch (Exception e)
            {
                WriteEventHandlerError(Res.GetString(Res.InstallSeverityWarning), "OnCommitting", e);
                Context?.LogMessage(Res.GetString(Res.InstallCommitException));
                savedException = e;
            }

            var lastInstallerAttempted = (int)(savedState["_reserved_lastInstallerAttempted"] ?? 0);
            if (savedState["_reserved_nestedSavedStates"] is not IDictionary[] array || lastInstallerAttempted + 1 != array.Length ||
                lastInstallerAttempted >= Installers.Count)
            {
                throw new ArgumentException(Res.GetString(Res.InstallDictionaryCorrupted, "savedState"), nameof(savedState));
            }

            foreach (var installer in Installers)
            {
                installer.Context = Context;
            }

            for (var j = 0; j <= lastInstallerAttempted; j++)
            {
                try
                {
                    Installers[j].Commit(array[j]);
                }
                catch (Exception e)
                {
                    if (!IsWrappedException(e))
                    {
                        Context?.LogMessage(Res.GetString(Res.InstallLogCommitException, Installers[j].ToString()));
                        LogException(e, Context);
                        Context?.LogMessage(Res.GetString(Res.InstallCommitException));
                    }
                    savedException = e;
                }
            }
            savedState["_reserved_nestedSavedStates"] = array;
            savedState.Remove("_reserved_lastInstallerAttempted");
            try
            {
                OnCommitted(savedState);
            }
            catch (Exception e)
            {
                WriteEventHandlerError(Res.GetString(Res.InstallSeverityWarning), "OnCommitted", e);
                Context?.LogMessage(Res.GetString(Res.InstallCommitException));
                savedException = e;
            }
            if (savedException == null)
            {
                return;
            }
            var wrappedException = savedException;
            if (!IsWrappedException(savedException))
            {
                wrappedException = new InstallException(Res.GetString(Res.InstallCommitException), savedException)
                {
                    Source = WrappedExceptionSource
                };
            }
            throw wrappedException;
        }

        protected virtual void Rollback(IDictionary savedState)
        {
            if (savedState == null)
            {
                throw new ArgumentException(Res.GetString(Res.InstallNullParameter, "savedState"), nameof(savedState));
            }

            if (savedState["_reserved_lastInstallerAttempted"] == null || savedState["_reserved_nestedSavedStates"] == null)
            {
                throw new ArgumentException(Res.GetString(Res.InstallDictionaryMissingValues, "savedState"), nameof(savedState));
            }

            Exception? savedException = null;
            try
            {
                OnBeforeRollback(savedState);
            }
            catch (Exception e)
            {
                WriteEventHandlerError(Res.GetString(Res.InstallSeverityWarning), "OnBeforeRollback", e);
                Context?.LogMessage(Res.GetString(Res.InstallRollbackException));
                savedException = e;
            }

            var lastInstallerAttempted = (int)(savedState["_reserved_lastInstallerAttempted"] ?? 0);
            if (savedState["_reserved_nestedSavedStates"] is not IDictionary[] nestedSavedStates ||
                lastInstallerAttempted + 1 != nestedSavedStates.Length || lastInstallerAttempted >= Installers.Count)
            {
                throw new ArgumentException(Res.GetString(Res.InstallDictionaryCorrupted, "savedState"), nameof(savedState));
            }

            for (var i = Installers.Count - 1; i >= 0; i--)
            {
                Installers[i].Context = Context;
            }
            for (var i = lastInstallerAttempted; i >= 0; i--)
            {
                try
                {
                    Installers[i].Rollback(nestedSavedStates[i]);
                }
                catch (Exception e)
                {
                    if (!IsWrappedException(e))
                    {
                        Context?.LogMessage(Res.GetString(Res.InstallLogRollbackException, Installers[i].ToString()));
                        LogException(e, Context);
                        Context?.LogMessage(Res.GetString(Res.InstallRollbackException));
                    }
                    savedException = e;
                }
            }
            try
            {
                OnAfterRollback(savedState);
            }
            catch (Exception e)
            {
                WriteEventHandlerError(Res.GetString(Res.InstallSeverityWarning), "OnAfterRollback", e);
                Context?.LogMessage(Res.GetString(Res.InstallRollbackException));
                savedException = e;
            }
            if (savedException == null)
            {
                return;
            }
            var wrappedException = savedException;
            if (!IsWrappedException(savedException))
            {
                wrappedException = new InstallException(Res.GetString(Res.InstallRollbackException), savedException)
                {
                    Source = WrappedExceptionSource
                };
            }
            throw wrappedException;
        }

        private static bool IsWrappedException(Exception e) => e is InstallException && string.Equals(e.Source, WrappedExceptionSource, StringComparison.OrdinalIgnoreCase) && e.TargetSite?.ReflectedType == typeof(Installer);

        private bool InstallerTreeContains(Installer target) =>
            Installers.Contains(target) || Installers.Any(installer => installer.InstallerTreeContains(target));

        /// <summary>Raises the <see cref="AfterInstall" /> event.</summary>
        /// <param name="savedState">An <see cref="IDictionary" /> that contains the state of the computer after all the installers contained in the <see cref="Installers" /> property have completed their installations. </param>
        private void OnAfterInstall(IDictionary savedState) => _afterInstallHandler?.Invoke(this, new InstallEventArgs(savedState));

        /// <summary>Raises the <see cref="AfterRollback" /> event.</summary>
        /// <param name="savedState">An <see cref="IDictionary" /> that contains the state of the computer after the installers contained in the <see cref="Installers" /> property are rolled back. </param>
        private void OnAfterRollback(IDictionary savedState) => _afterRollbackHandler?.Invoke(this, new InstallEventArgs(savedState));

        /// <summary>Raises the <see cref="AfterUninstall" /> event.</summary>
        /// <param name="savedState">An <see cref="IDictionary" /> that contains the state of the computer after all the installers contained in the <see cref="Installers" /> property are uninstalled. </param>
        private void OnAfterUninstall(IDictionary? savedState) => _afterUninstallHandler?.Invoke(this, new InstallEventArgs(savedState));

        /// <summary>Raises the <see cref="BeforeInstall" /> event.</summary>
        /// <param name="savedState">An <see cref="IDictionary" /> that contains the state of the computer before the installers in the <see cref="Installers" /> property are installed. This <see cref="IDictionary" /> object should be empty at this point. </param>
        private void OnBeforeInstall(IDictionary savedState) => _beforeInstallHandler?.Invoke(this, new InstallEventArgs(savedState));

        /// <summary>Raises the <see cref="BeforeRollback" /> event.</summary>
        /// <param name="savedState">An <see cref="IDictionary" /> that contains the state of the computer before the installers in the <see cref="Installers" /> property are rolled back. </param>
        private void OnBeforeRollback(IDictionary savedState) => _beforeRollbackHandler?.Invoke(this, new InstallEventArgs(savedState));

        /// <summary>Raises the <see cref="BeforeUninstall" /> event.</summary>
        /// <param name="savedState">An <see cref="IDictionary" /> that contains the state of the computer before the installers in the <see cref="Installers" /> property uninstall their installations. </param>
        private void OnBeforeUninstall(IDictionary? savedState) => _beforeUninstallHandler?.Invoke(this, new InstallEventArgs(savedState));

        private void OnCommitted(IDictionary savedState) => _afterCommitHandler?.Invoke(this, new InstallEventArgs(savedState));

        /// <summary>Raises the <see cref="Committing" /> event.</summary>
        /// <param name="savedState">An <see cref="IDictionary" /> that contains the state of the computer before the installers in the <see cref="Installers" /> property are committed. </param>
        private void OnCommitting(IDictionary savedState) => _beforeCommitHandler?.Invoke(this, new InstallEventArgs(savedState));

        private void WriteEventHandlerError(string severity, string eventName, Exception e)
        {
            Context?.LogMessage(Res.GetString(Res.InstallLogError, severity, eventName, GetType().FullName ?? string.Empty));
            LogException(e, Context);
        }
    }
}
