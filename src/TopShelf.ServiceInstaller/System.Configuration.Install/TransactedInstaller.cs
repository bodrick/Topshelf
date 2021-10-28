using System.Collections;

namespace System.Configuration.Install
{
    public class TransactedInstaller : Installer
    {
        public override void Install(IDictionary savedState)
        {
            if (Context == null)
            {
                Context = new InstallContext();
            }
            Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoTransacted"));
            try
            {
                var flag = true;
                try
                {
                    Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoBeginInstall"));
                    base.Install(savedState);
                }
                catch (Exception ex)
                {
                    flag = false;
                    Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoException"));
                    LogException(ex, Context);
                    Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoBeginRollback"));
                    try
                    {
                        Rollback(savedState);
                    }
                    catch (Exception)
                    {
                    }
                    Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoRollbackDone"));
                    throw new InvalidOperationException(Res.GetString("InstallRollback"), ex);
                }
                if (flag)
                {
                    Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoBeginCommit"));
                    try
                    {
                        Commit(savedState);
                    }
                    finally
                    {
                        Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoCommitDone"));
                    }
                }
            }
            finally
            {
                Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoTransactedDone"));
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            if (Context == null)
            {
                Context = new InstallContext();
            }
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
