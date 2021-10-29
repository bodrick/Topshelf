using System.Collections;

namespace System.Configuration.Install
{
    public class TransactedInstaller : Installer
    {
        public override void Install(IDictionary stateSaver)
        {
            Context ??= new InstallContext();

            Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoTransacted"));
            try
            {
                try
                {
                    Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoBeginInstall"));
                    base.Install(stateSaver);
                }
                catch (Exception ex)
                {
                    Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoException"));
                    LogException(ex, Context);
                    Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoBeginRollback"));
                    try
                    {
                        Rollback(stateSaver);
                    }
                    catch
                    {
                        // Ignore
                    }
                    Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoRollbackDone"));
                    throw new InvalidOperationException(Res.GetString("InstallRollback"), ex);
                }

                Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoBeginCommit"));
                try
                {
                    Commit(stateSaver);
                }
                finally
                {
                    Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoCommitDone"));
                }
            }
            finally
            {
                Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoTransactedDone"));
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            Context ??= new InstallContext();
            Context.LogMessage(Environment.NewLine + Environment.NewLine + Res.GetString("InstallInfoBeginUninstall"));
            try
            {
                base.Uninstall(savedState);
            }
            finally
            {
                Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoUninstallDone"));
            }
        }
    }
}
