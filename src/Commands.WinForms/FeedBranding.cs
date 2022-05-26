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
             ?.To(TryParseIcon)
            ?? GetDefaultIcon();

        SplashScreen = feed
                     ?.SplashScreens.GetIcon(ModelIcon.MimeTypePng)
                     ?.To(iconStore.GetCached)
                     ?.To(Image.FromFile);
    }

    private static SystemIcon? TryParseIcon(string path)
    {
        try
        {
            return new(path);
        }
        catch (Exception ex) when (ex is ArgumentException or Win32Exception)
        {
            return null;
        }
    }

    private static SystemIcon? GetDefaultIcon()
    {
        try
        {
            return SystemIcon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch (ArgumentException) // Running from network path, can't extract icon
        {
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
