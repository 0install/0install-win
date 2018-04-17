// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using ZeroInstall.Store;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Available tabs in a <see cref="Config"/> GUI.
    /// </summary>
    public enum ConfigTab
    {
        Default,
        Updates,
        Storage,
        Catalog,
        Trust,
        Sync,
        Language,
        Advanced
    }
}
