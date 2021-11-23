using System.Collections;

namespace System.Configuration.Install
{
    public delegate void InstallEventHandler(object sender, InstallEventArgs e);

    public class InstallEventArgs : EventArgs
    {
        public InstallEventArgs(IDictionary? savedSate) => SavedSate = savedSate;

        public IDictionary? SavedSate { get; }
    }
}
