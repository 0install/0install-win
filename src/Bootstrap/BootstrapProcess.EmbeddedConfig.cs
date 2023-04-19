// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using ZeroInstall.Store.Configuration;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;

namespace ZeroInstall;

partial class BootstrapProcess
{
    private readonly EmbeddedConfig _embeddedConfig = EmbeddedConfig.Load();

    /// <summary>
    /// Updates <see cref="Config.SelfUpdateUri"/>.
    /// </summary>
    private void UpdateSelfUpdateUri()
    {
        if (_embeddedConfig.SelfUpdateUri == null || _embeddedConfig.SelfUpdateUri == new FeedUri(Config.DefaultSelfUpdateUri))
        {
            if (Config.SelfUpdateUri != new FeedUri(Config.DefaultSelfUpdateUri))
            {
                Log.Warn($"Resetting self-update URI from {Config.SelfUpdateUri} back to default");
                Config.SelfUpdateUri = new(Config.DefaultSelfUpdateUri);
            }
        }
        else if (_embeddedConfig.SelfUpdateUri != Config.SelfUpdateUri && GetDeployedInstance() == null)
        {
            Log.Info($"Setting self-update URI to {_embeddedConfig.SelfUpdateUri}");
            Config.SelfUpdateUri = _embeddedConfig.SelfUpdateUri;

            if (Config.FeedMirror == new FeedUri(Config.DefaultFeedMirror))
            {
                Log.Info("Disabling feed mirror");
                Config.FeedMirror = null;
            }
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

        string? newPath = _handler.GetCustomPath(_machineWide, currentPath);
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
            trust.TrustKey("88C8A1F375928691D7365C0259AA3927C24E4E1E", new Domain("apps.0install.net"));
            if (_embeddedConfig.KeyFingerprint != null)
            {
                if (_embeddedConfig is {AppUri: not null, AppName: not null})
                    trust.TrustKey(_embeddedConfig.KeyFingerprint, new Domain(_embeddedConfig.AppUri.Host));
                else if (_embeddedConfig.SelfUpdateUri != null)
                    trust.TrustKey(_embeddedConfig.KeyFingerprint, new Domain(_embeddedConfig.SelfUpdateUri.Host));
            }
            trust.Save();
        }
        #region Error handling
        catch (Exception ex)
        {
            Log.Error("Failed to add key to trust database", ex);
        }
        #endregion
    }
}
