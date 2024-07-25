// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store.Configuration;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Icons;
using ZeroInstall.Store.Trust;
using ModelIcon = ZeroInstall.Model.Icon;
using SystemIcon = System.Drawing.Icon;

namespace ZeroInstall.Commands.WinForms;

/// <summary>
/// Feed-specific visual branding.
/// </summary>
public class FeedBranding : IDisposable
{
    /// <summary>
    /// The name of the app.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// The Application User Model ID.
    /// </summary>
    public string? AppId { get; }

    /// <summary>
    /// The icon of the app.
    /// </summary>
    public SystemIcon? Icon { get; }

    /// <summary>
    /// An optional splash screen to display during downloads, etc..
    /// </summary>
    public Image? SplashScreen { get; }

    /// <summary>
    /// Loads visual branding information for a specific <paramref name="feedUri"/> or falls back to defaults.
    /// </summary>
    public FeedBranding(FeedUri? feedUri)
    {
        var feed = feedUri?.To(LoadFeed);
        Name = feed?.Name;
        AppId = feed?.GetEntryPoint(Command.NameRun)?.AppId;
        Icon = feed
             ?.Icons.GetIcon(ModelIcon.MimeTypeIco)
             ?.To(TryGetIconPath)
             ?.To(LoadIcon)
            ?? LoadDefaultIcon();
        SplashScreen = feed
                     ?.SplashScreens.GetIcon(ModelIcon.MimeTypePng)
                     ?.To(TryGetIconPath)
                     ?.To(LoadSplashScreen);
    }

    private static Feed? LoadFeed(FeedUri feedUri)
    {
        try
        {
            return FeedCaches.Default(OpenPgp.Verifying()).GetFeed(feedUri);
        }
        #region Error handling
        catch (Exception ex)
        {
            Log.Warn($"Failed to feed {feedUri}", ex);
            return null;
        }
        #endregion
    }

    private static string? TryGetIconPath(Model.Icon icon)
    {
        try
        {
            return IconStores.DesktopIntegration(Config.LoadSafe(), new SilentTaskHandler(), machineWide: false).TryGetCached(icon)
                ?? IconStores.DesktopIntegration(Config.LoadSafe(), new SilentTaskHandler(), machineWide: true).TryGetCached(icon);
        }
        #region Error handling
        catch (Exception ex)
        {
            Log.Warn($"Failed to get path for {icon}", ex);
            return null;
        }
        #endregion
    }

    private static SystemIcon? LoadIcon(string path)
    {
        try
        {
            return new(path);
        }
        #region Error handling
        catch (Exception ex)
        {
            Log.Warn($"Failed to load icon '{path}'", ex);
            return null;
        }
        #endregion
    }

    private static SystemIcon? LoadDefaultIcon()
    {
        try
        {
            return SystemIcon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        #region Error handling
        catch (Exception ex)
        {
            Log.Warn("Failed to load default icon", ex);
            return null;
        }
        #endregion
    }

    private static Image? LoadSplashScreen(string path)
    {
        try
        {
            // Not using Image.FromFile() to ensure eager file validation
            using var stream = File.OpenRead(path);
            return Image.FromStream(stream);
        }
        #region Error handling
        catch (Exception ex)
        {
            Log.Warn($"Failed to load splash screen '{path}'", ex);
            return null;
        }
        #endregion
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Icon?.Dispose();
        SplashScreen?.Dispose();
    }
}
