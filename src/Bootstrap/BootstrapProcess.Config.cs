// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;
using ZeroInstall.Store.Configuration;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;

namespace ZeroInstall;

partial class BootstrapProcess
{
    /// <summary>
    /// Loads configuration, applying bundled defaults.
    /// </summary>
    private void LoadConfig()
    {
        Config.ReadFromFiles();

        if (ApplyBootstrapConfig) Config.ReadFromBootstrapConfig();
        else Log.Info("Ignoring bootstrap config due to existing Zero Install deployment");

        Config.ReadFromGroupPolicy();
    }

    /// <summary>
    /// Persists potentially customized Zero Install configuration.
    /// </summary>
    private void SaveConfig()
    {
        Config.Save();

        if (_machineWide)
        {
            Log.Info("Saving config in machine-wide location");
            var machineConfig = new Config();
            machineConfig.ReadFromBootstrapConfig();
            machineConfig.ReadFromFilesMachineWideOnly();
            if (ApplyBootstrapConfig) machineConfig.ReadFromBootstrapConfig();
            machineConfig.SelfUpdateUri = Config.SelfUpdateUri;
            machineConfig.Save(machineWide: true);
        }

        if (ApplyBootstrapConfig && BootstrapConfig.Instance.CatalogUri is {} catalogUri)
        {
            Log.Info($"Setting custom catalog source: {catalogUri}");
            Services.Feeds.CatalogManager.SetSources([catalogUri], _machineWide);
        }
    }

    /// <summary>
    /// Indicates whether to apply options bundled together with the <see cref="BootstrapConfig"/>,
    /// potentially overwriting existing configuration.
    /// </summary>
    private bool ApplyBootstrapConfig
        => ZeroInstallDeployment.FindOther(_machineWide) == null // No existing deployment...
        || ZeroInstallDeployment.IsLibraryMode(_machineWide); // ... or existing deployment is in library-mode

    /// <summary>
    /// Asks the user to provide a custom path for storing implementations.
    /// </summary>
    private void CustomizeStorePath()
    {
        var currentPaths = (_machineWide
            ? ImplementationStores.GetMachineWideDirectories()
            : ImplementationStores.GetUserDirectories()).ToList();
        if (currentPaths.Count > 1)
        {
            Log.Info("Refusing to customize path, because multiple custom implementation directories are already configured");
            return;
        }
        string? currentPath = currentPaths.FirstOrDefault();
        string? newPath = _storePath ?? _handler.GetCustomStorePath(_machineWide, currentPath);

        if (newPath != null && IsMachineWidePath(newPath))
        {
            if (!WindowsUtils.IsAdministrator) throw new NotAdminException();
            _machineWide = true;
        }

        string[] newPaths = string.IsNullOrEmpty(newPath) ? [] : [newPath];
        if (_machineWide) ImplementationStores.SetMachineWideDirectories(newPaths);
        else ImplementationStores.SetUserDirectories(newPaths);
    }

    private static bool IsMachineWidePath(string path)
        => path.StartsWithIgnoreCase(WindowsUtils.GetFolderPath(Environment.SpecialFolder.ProgramFiles))
        || path.StartsWithIgnoreCase(WindowsUtils.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

    /// <summary>
    /// Adds keys for Zero Install (and optionally an app) to the <see cref="TrustDB"/>.
    /// </summary>
    private void TrustKeys()
    {
        try
        {
            var trust = _machineWide
                ? TrustDB.LoadMachineWide()
                : TrustDB.Load();
            trust.TrustKey("88C8A1F375928691D7365C0259AA3927C24E4E1E", new("apps.0install.net"));
            if (BootstrapConfig.Instance.KeyFingerprint is {} fingerprint)
            {
                if (BootstrapConfig.Instance.AppUri is {} appUri)
                    trust.TrustKey(fingerprint, new(appUri.Host));
                else if (Config.SelfUpdateUri is {} selfUpdateUri)
                    trust.TrustKey(fingerprint, new(selfUpdateUri.Host));
            }
            trust.Save();
        }
        #region Error handling
        catch (Exception ex)
        {
            Log.Error("Failed to add keys to trust database", ex);
        }
        #endregion
    }
}
