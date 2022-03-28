// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Drawing;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Model;
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
    /// The title of the window.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// The icon of the window.
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

        Title = feed?.To(x => $"{x.Name} (powered by Zero Install)")
             ?? "Zero Install";
        if (Locations.IsPortable) Title += @" - " + Resources.PortableMode;

        var iconStore = IconStores.DesktopIntegration(Config.LoadSafe(), new SilentTaskHandler(), machineWide: false);
        Icon = feed
             ?.Icons.GetIcon(ModelIcon.MimeTypeIco)
             ?.To(iconStore.GetCached)
             ?.To(path => new SystemIcon(path))
            ?? GetDefaultIcon();

        SplashScreen = feed
                     ?.SplashScreens.GetIcon(ModelIcon.MimeTypePng)
                     ?.To(iconStore.GetCached)
                     ?.To(Image.FromFile);
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
