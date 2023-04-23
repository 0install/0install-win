﻿// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using ZeroInstall.Store.Configuration;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;

namespace ZeroInstall;

partial class BootstrapProcess
{
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
            machineConfig.ReadFromFiles(machineWideOnly: true);
            machineConfig.SelfUpdateUri = Config.SelfUpdateUri;
            machineConfig.Save(machineWide: true);
        }
    }

    /// <summary>
    /// Asks the user to provide a custom path for storing implementations.
    /// </summary>
    private void CustomizePath()
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

        string? newPath = _installDir ?? _handler.GetCustomPath(_machineWide, currentPath);
        var newPaths = new List<string>();
        if (!string.IsNullOrEmpty(newPath)) newPaths.Add(newPath);

        if (_machineWide) ImplementationStores.SetMachineWideDirectories(newPaths);
        else ImplementationStores.SetUserDirectories(newPaths);
    }

    /// <summary>
    /// Adds keys for Zero Install (and optionally an app) to the <see cref="TrustDB"/>.
    /// </summary>
    private void TrustKeys()
    {
        try
        {
            var trust = TrustDB.Load();
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
