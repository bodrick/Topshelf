namespace System.Diagnostics
{
    /// <summary>Specifies what an installer should do during an uninstallation.</summary>
    public enum UninstallAction
    {
        /// <summary>Removes the resource the installer created.</summary>
        Remove = 0,

        /// <summary>Leaves the resource created by the installer as is.</summary>
        NoAction = 1
    }
}
