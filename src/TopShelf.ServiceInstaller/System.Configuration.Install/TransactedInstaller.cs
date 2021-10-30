using System.Collections;

namespace System.Configuration.Install
{
    public class TransactedInstaller : Installer
    {
        public override void Install(IDictionary stateSaver)
        {
            Context ??= new InstallContext();

            Context.LogMessage(Environment.NewLine + Res.GetString(Res.InstallInfoTransacted));
            try
            {
                try
                {
                    Context.LogMessage(Environment.NewLine + Res.GetString(Res.InstallInfoBeginInstall));
                    base.Install(stateSaver);
                }
                catch (Exception ex)
                {
                    Context.LogMessage(Environment.NewLine + Res.GetString(Res.InstallInfoException));
                    LogException(ex, Context);
                    Context.LogMessage(Environment.NewLine + Res.GetString(Res.InstallInfoBeginRollback));
                    try
                    {
                        Rollback(stateSaver);
                    }
                    catch
                    {
                        // Ignore
                    }
                    Context.LogMessage(Environment.NewLine + Res.GetString(Res.InstallInfoRollbackDone));
                    throw new InvalidOperationException(Res.GetString(Res.InstallRollback), ex);
                }

                Context.LogMessage(Environment.NewLine + Res.GetString(Res.InstallInfoBeginCommit));
                try
                {
                    Commit(stateSaver);
                }
                finally
                {
                    Context.LogMessage(Environment.NewLine + Res.GetString(Res.InstallInfoCommitDone));
                }
            }
            finally
            {
                Context.LogMessage(Environment.NewLine + Res.GetString(Res.InstallInfoTransactedDone));
            }
        }

        public override void Uninstall(IDictionary? savedState)
        {
            Context ??= new InstallContext();
            Context.LogMessage(Environment.NewLine + Environment.NewLine + Res.GetString(Res.InstallInfoBeginUninstall));
            try
            {
                base.Uninstall(savedState);
            }
            finally
            {
                Context.LogMessage(Environment.NewLine + Res.GetString(Res.InstallInfoUninstallDone));
            }
        }
    }
}
