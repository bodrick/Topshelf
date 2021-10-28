using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System.Configuration.Install
{
    public class Installer : System.ComponentModel.Component
    {
        internal Installer parent;
        private InstallerCollection installers;

        public InstallContext Context { get; set; }

        [ResDescription("Desc_Installer_HelpText")]
        public virtual string HelpText
        {
            get
            {
                var stringBuilder = new StringBuilder();
                for (var i = 0; i < Installers.Count; i++)
                {
                    var helpText = Installers[i].HelpText;
                    if (helpText.Length > 0)
                    {
                        stringBuilder.Append("\r\n");
                        stringBuilder.Append(helpText);
                    }
                }
                return stringBuilder.ToString();
            }
        }

        public InstallerCollection Installers
        {
            get
            {
                if (installers == null)
                {
                    installers = new InstallerCollection(this);
                }

                return installers;
            }
        }

        [TypeConverter(typeof(InstallerParentConverter))]
        [ResDescription("Desc_Installer_Parent")]
        public Installer Parent
        {
            get => parent;
            set
            {
                if (value == this)
                {
                    throw new InvalidOperationException(Res.GetString("InstallBadParent"));
                }
                if (value != parent)
                {
                    if (value != null && InstallerTreeContains(value))
                    {
                        throw new InvalidOperationException(Res.GetString("InstallRecursiveParent"));
                    }
                    if (parent != null)
                    {
                        var num = parent.Installers.IndexOf(this);
                        if (num != -1)
                        {
                            parent.Installers.RemoveAt(num);
                        }
                    }
                    parent = value;
                    if (parent != null && !parent.Installers.Contains(this))
                    {
                        parent.Installers.Add(this);
                    }
                }
            }
        }

        public virtual void Commit(IDictionary savedState)
        {
            if (savedState == null)
            {
                throw new ArgumentException(Res.GetString("InstallNullParameter", "savedState"));
            }
            if (savedState["_reserved_lastInstallerAttempted"] != null && savedState["_reserved_nestedSavedStates"] != null)
            {
                Exception ex = null;
                try
                {
                    OnCommitting(savedState);
                }
                catch (Exception ex2)
                {
                    WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnCommitting", ex2);
                    Context.LogMessage(Res.GetString("InstallCommitException"));
                    ex = ex2;
                }
                var num = (int)savedState["_reserved_lastInstallerAttempted"];
                var array = (IDictionary[])savedState["_reserved_nestedSavedStates"];
                if (num + 1 == array.Length && num < Installers.Count)
                {
                    for (var i = 0; i < Installers.Count; i++)
                    {
                        Installers[i].Context = Context;
                    }
                    for (var j = 0; j <= num; j++)
                    {
                        try
                        {
                            Installers[j].Commit(array[j]);
                        }
                        catch (Exception ex3)
                        {
                            if (!IsWrappedException(ex3))
                            {
                                Context.LogMessage(Res.GetString("InstallLogCommitException", Installers[j].ToString()));
                                Installer.LogException(ex3, Context);
                                Context.LogMessage(Res.GetString("InstallCommitException"));
                            }
                            ex = ex3;
                        }
                    }
                    savedState["_reserved_nestedSavedStates"] = array;
                    savedState.Remove("_reserved_lastInstallerAttempted");
                    try
                    {
                        OnCommitted(savedState);
                    }
                    catch (Exception ex4)
                    {
                        WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnCommitted", ex4);
                        Context.LogMessage(Res.GetString("InstallCommitException"));
                        ex = ex4;
                    }
                    if (ex == null)
                    {
                        return;
                    }
                    var ex5 = ex;
                    if (!IsWrappedException(ex))
                    {
                        ex5 = new InstallException(Res.GetString("InstallCommitException"), ex)
                        {
                            Source = "WrappedExceptionSource"
                        };
                    }
                    throw ex5;
                }
                throw new ArgumentException(Res.GetString("InstallDictionaryCorrupted", "savedState"));
            }
            throw new ArgumentException(Res.GetString("InstallDictionaryMissingValues", "savedState"));
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
                WriteEventHandlerError(Res.GetString("InstallSeverityError"), "OnBeforeInstall", ex);
                throw new InvalidOperationException(Res.GetString("InstallEventException", "OnBeforeInstall", base.GetType().FullName), ex);
            }
            var num = -1;
            var arrayList = new List<IDictionary>();
            try
            {
                for (var i = 0; i < Installers.Count; i++)
                {
                    Installers[i].Context = Context;
                }
                for (var j = 0; j < Installers.Count; j++)
                {
                    var installer = Installers[j];
                    IDictionary dictionary = new Hashtable();
                    try
                    {
                        num = j;
                        installer.Install(dictionary);
                    }
                    finally
                    {
                        arrayList.Add(dictionary);
                    }
                }
            }
            finally
            {
                stateSaver.Add("_reserved_lastInstallerAttempted", num);
                stateSaver.Add("_reserved_nestedSavedStates", arrayList.ToArray());
            }
            try
            {
                OnAfterInstall(stateSaver);
            }
            catch (Exception ex2)
            {
                WriteEventHandlerError(Res.GetString("InstallSeverityError"), "OnAfterInstall", ex2);
                throw new InvalidOperationException(Res.GetString("InstallEventException", "OnAfterInstall", base.GetType().FullName), ex2);
            }
        }

        public virtual void Rollback(IDictionary savedState)
        {
            if (savedState == null)
            {
                throw new ArgumentException(Res.GetString("InstallNullParameter", "savedState"));
            }
            if (savedState["_reserved_lastInstallerAttempted"] != null && savedState["_reserved_nestedSavedStates"] != null)
            {
                Exception ex = null;
                try
                {
                    OnBeforeRollback(savedState);
                }
                catch (Exception ex2)
                {
                    WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnBeforeRollback", ex2);
                    Context.LogMessage(Res.GetString("InstallRollbackException"));
                    ex = ex2;
                }
                var num = (int)savedState["_reserved_lastInstallerAttempted"];
                var array = (IDictionary[])savedState["_reserved_nestedSavedStates"];
                if (num + 1 == array.Length && num < Installers.Count)
                {
                    for (var num2 = Installers.Count - 1; num2 >= 0; num2--)
                    {
                        Installers[num2].Context = Context;
                    }
                    for (var num3 = num; num3 >= 0; num3--)
                    {
                        try
                        {
                            Installers[num3].Rollback(array[num3]);
                        }
                        catch (Exception ex3)
                        {
                            if (!IsWrappedException(ex3))
                            {
                                Context.LogMessage(Res.GetString("InstallLogRollbackException", Installers[num3].ToString()));
                                Installer.LogException(ex3, Context);
                                Context.LogMessage(Res.GetString("InstallRollbackException"));
                            }
                            ex = ex3;
                        }
                    }
                    try
                    {
                        OnAfterRollback(savedState);
                    }
                    catch (Exception ex4)
                    {
                        WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnAfterRollback", ex4);
                        Context.LogMessage(Res.GetString("InstallRollbackException"));
                        ex = ex4;
                    }
                    if (ex == null)
                    {
                        return;
                    }
                    var ex5 = ex;
                    if (!IsWrappedException(ex))
                    {
                        ex5 = new InstallException(Res.GetString("InstallRollbackException"), ex)
                        {
                            Source = "WrappedExceptionSource"
                        };
                    }
                    throw ex5;
                }
                throw new ArgumentException(Res.GetString("InstallDictionaryCorrupted", "savedState"));
            }
            throw new ArgumentException(Res.GetString("InstallDictionaryMissingValues", "savedState"));
        }

        public virtual void Uninstall(IDictionary savedState)
        {
            Exception ex = null;
            try
            {
                OnBeforeUninstall(savedState);
            }
            catch (Exception ex2)
            {
                WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnBeforeUninstall", ex2);
                Context.LogMessage(Res.GetString("InstallUninstallException"));
                ex = ex2;
            }
            IDictionary[] array;
            if (savedState != null)
            {
                array = (IDictionary[])savedState["_reserved_nestedSavedStates"];
                if (array != null && array.Length == Installers.Count)
                {
                    goto IL_0091;
                }
                throw new ArgumentException(Res.GetString("InstallDictionaryCorrupted", "savedState"));
            }
            array = new IDictionary[Installers.Count];
            goto IL_0091;
            IL_0091:
            for (var num = Installers.Count - 1; num >= 0; num--)
            {
                Installers[num].Context = Context;
            }
            for (var num2 = Installers.Count - 1; num2 >= 0; num2--)
            {
                try
                {
                    Installers[num2].Uninstall(array[num2]);
                }
                catch (Exception ex3)
                {
                    if (!IsWrappedException(ex3))
                    {
                        Context.LogMessage(Res.GetString("InstallLogUninstallException", Installers[num2].ToString()));
                        Installer.LogException(ex3, Context);
                        Context.LogMessage(Res.GetString("InstallUninstallException"));
                    }
                    ex = ex3;
                }
            }
            try
            {
                OnAfterUninstall(savedState);
            }
            catch (Exception ex4)
            {
                WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnAfterUninstall", ex4);
                Context.LogMessage(Res.GetString("InstallUninstallException"));
                ex = ex4;
            }
            if (ex == null)
            {
                return;
            }
            var ex5 = ex;
            if (!IsWrappedException(ex))
            {
                ex5 = new InstallException(Res.GetString("InstallUninstallException"), ex)
                {
                    Source = "WrappedExceptionSource"
                };
            }
            throw ex5;
        }

        #region Event Handlers

        private InstallEventHandler afterCommitHandler;
        private InstallEventHandler afterInstallHandler;
        private InstallEventHandler afterRollbackHandler;
        private InstallEventHandler afterUninstallHandler;
        private InstallEventHandler beforeCommitHandler;
        private InstallEventHandler beforeInstallHandler;
        private InstallEventHandler beforeRollbackHandler;
        private InstallEventHandler beforeUninstallHandler;

        /// <summary>Occurs after the <see cref="M:System.Configuration.Install.Installer.Install(System.Collections.IDictionary)" /> methods of all the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property have run.</summary>
        public event InstallEventHandler AfterInstall
        {
            add
            {
                afterInstallHandler = (InstallEventHandler)Delegate.Combine(afterInstallHandler, value);
            }
            remove
            {
                afterInstallHandler = (InstallEventHandler)Delegate.Remove(afterInstallHandler, value);
            }
        }

        /// <summary>Occurs after the installations of all the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property are rolled back.</summary>
        public event InstallEventHandler AfterRollback
        {
            add
            {
                afterRollbackHandler = (InstallEventHandler)Delegate.Combine(afterRollbackHandler, value);
            }
            remove
            {
                afterRollbackHandler = (InstallEventHandler)Delegate.Remove(afterRollbackHandler, value);
            }
        }

        /// <summary>Occurs after all the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property perform their uninstallation operations.</summary>
        public event InstallEventHandler AfterUninstall
        {
            add
            {
                afterUninstallHandler = (InstallEventHandler)Delegate.Combine(afterUninstallHandler, value);
            }
            remove
            {
                afterUninstallHandler = (InstallEventHandler)Delegate.Remove(afterUninstallHandler, value);
            }
        }

        /// <summary>Occurs before the <see cref="M:System.Configuration.Install.Installer.Install(System.Collections.IDictionary)" /> method of each installer in the installer collection has run.</summary>
        public event InstallEventHandler BeforeInstall
        {
            add
            {
                beforeInstallHandler = (InstallEventHandler)Delegate.Combine(beforeInstallHandler, value);
            }
            remove
            {
                beforeInstallHandler = (InstallEventHandler)Delegate.Remove(beforeInstallHandler, value);
            }
        }

        /// <summary>Occurs before the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property are rolled back.</summary>
        public event InstallEventHandler BeforeRollback
        {
            add
            {
                beforeRollbackHandler = (InstallEventHandler)Delegate.Combine(beforeRollbackHandler, value);
            }
            remove
            {
                beforeRollbackHandler = (InstallEventHandler)Delegate.Remove(beforeRollbackHandler, value);
            }
        }

        /// <summary>Occurs before the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property perform their uninstall operations.</summary>
        public event InstallEventHandler BeforeUninstall
        {
            add
            {
                beforeUninstallHandler = (InstallEventHandler)Delegate.Combine(beforeUninstallHandler, value);
            }
            remove
            {
                beforeUninstallHandler = (InstallEventHandler)Delegate.Remove(beforeUninstallHandler, value);
            }
        }

        public event InstallEventHandler Committed
        {
            add
            {
                afterCommitHandler = (InstallEventHandler)Delegate.Combine(afterCommitHandler, value);
            }
            remove
            {
                afterCommitHandler = (InstallEventHandler)Delegate.Remove(afterCommitHandler, value);
            }
        }

        /// <summary>Occurs before the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property committ their installations.</summary>
        public event InstallEventHandler Committing
        {
            add
            {
                beforeCommitHandler = (InstallEventHandler)Delegate.Combine(beforeCommitHandler, value);
            }
            remove
            {
                beforeCommitHandler = (InstallEventHandler)Delegate.Remove(beforeCommitHandler, value);
            }
        }

        /// <summary>Raises the <see cref="E:System.Configuration.Install.Installer.AfterInstall" /> event.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer after all the installers contained in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property have completed their installations. </param>
        protected virtual void OnAfterInstall(IDictionary savedState) => afterInstallHandler?.Invoke(this, new InstallEventArgs(savedState));

        /// <summary>Raises the <see cref="E:System.Configuration.Install.Installer.AfterRollback" /> event.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer after the installers contained in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property are rolled back. </param>
        protected virtual void OnAfterRollback(IDictionary savedState) => afterRollbackHandler?.Invoke(this, new InstallEventArgs(savedState));

        /// <summary>Raises the <see cref="E:System.Configuration.Install.Installer.AfterUninstall" /> event.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer after all the installers contained in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property are uninstalled. </param>
        protected virtual void OnAfterUninstall(IDictionary savedState) => afterUninstallHandler?.Invoke(this, new InstallEventArgs(savedState));

        /// <summary>Raises the <see cref="E:System.Configuration.Install.Installer.BeforeInstall" /> event.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer before the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property are installed. This <see cref="T:System.Collections.IDictionary" /> object should be empty at this point. </param>
        protected virtual void OnBeforeInstall(IDictionary savedState) => beforeInstallHandler?.Invoke(this, new InstallEventArgs(savedState));

        /// <summary>Raises the <see cref="E:System.Configuration.Install.Installer.BeforeRollback" /> event.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer before the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property are rolled back. </param>
        protected virtual void OnBeforeRollback(IDictionary savedState) => beforeRollbackHandler?.Invoke(this, new InstallEventArgs(savedState));

        /// <summary>Raises the <see cref="E:System.Configuration.Install.Installer.BeforeUninstall" /> event.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer before the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property uninstall their installations. </param>
        protected virtual void OnBeforeUninstall(IDictionary savedState) => beforeUninstallHandler?.Invoke(this, new InstallEventArgs(savedState));

        protected virtual void OnCommitted(IDictionary savedState) => afterCommitHandler?.Invoke(this, new InstallEventArgs(savedState));

        /// <summary>Raises the <see cref="E:System.Configuration.Install.Installer.Committing" /> event.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer before the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property are committed. </param>
        protected virtual void OnCommitting(IDictionary savedState) => beforeCommitHandler?.Invoke(this, new InstallEventArgs(savedState));

        #endregion Event Handlers

        internal static void LogException(Exception e, InstallContext context)
        {
            var flag = true;
            while (e != null)
            {
                if (flag)
                {
                    context.LogMessage(e.GetType().FullName + ": " + e.Message);
                    flag = false;
                }
                else
                {
                    context.LogMessage(Res.GetString("InstallLogInner", e.GetType().FullName, e.Message));
                }
                if (context.IsParameterTrue("showcallstack"))
                {
                    context.LogMessage(e.StackTrace);
                }
                e = e.InnerException;
            }
        }

        internal bool InstallerTreeContains(Installer target)
        {
            if (Installers.Contains(target))
            {
                return true;
            }
            foreach (var installer in Installers)
            {
                if (installer.InstallerTreeContains(target))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsWrappedException(Exception e)
        {
            if (e is InstallException && e.Source == "WrappedExceptionSource")
            {
                return e.TargetSite.ReflectedType == typeof(Installer);
            }
            return false;
        }

        private void WriteEventHandlerError(string severity, string eventName, Exception e)
        {
            Context.LogMessage(Res.GetString("InstallLogError", severity, eventName, base.GetType().FullName));
            Installer.LogException(e, Context);
        }
    }
}
