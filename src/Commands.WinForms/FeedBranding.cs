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
        var feedCache = FeedCaches.Default(OpenPgp.Verifying());
        var feed = feedUri?.To(feedCache.GetFeed);

        Name = feed?.Name;
        AppId = feed?.GetEntryPoint(Command.NameRun)?.AppId;

        var iconStore = IconStores.DesktopIntegration(Config.LoadSafe(), new SilentTaskHandler(), machineWide: false);
        Icon = feed
             ?.Icons.GetIcon(ModelIcon.MimeTypeIco)
             ?.To(iconStore.GetCached)
             ?.To(LoadIcon)
            ?? LoadDefaultIcon();

        SplashScreen = feed
                     ?.SplashScreens.GetIcon(ModelIcon.MimeTypePng)
                     ?.To(iconStore.GetCached)
                     ?.To(LoadSplashScreen);
    }

    private static SystemIcon? LoadIcon(string path)
    {
        try
        {
            return new(path);
        }
        catch (Exception ex)
        {
            Log.Warn($"Failed to load icon '{path}'", ex);
            return null;
        }
    }

    private static SystemIcon? LoadDefaultIcon()
    {
        try
        {
            return SystemIcon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch (Exception ex)
        {
            Log.Warn("Failed to load default icon", ex);
            return null;
        }
    }

    private static Image? LoadSplashScreen(string path)
    {
        try
        {
            // Not using Image.FromFile() to ensure eager file validation
            using var stream = File.OpenRead(path);
            return Image.FromStream(stream);
        }
        catch (Exception ex)
        {
            Log.Warn($"Failed to load splash screen '{path}'", ex);
            return null;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Icon?.Dispose();
        SplashScreen?.Dispose();
    }
}
