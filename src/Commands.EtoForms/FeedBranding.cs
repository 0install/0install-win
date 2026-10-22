// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;
using NanoByte.Common.Streams;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store.Configuration;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Icons;
using ZeroInstall.Store.Trust;
using Command = ZeroInstall.Model.Command;
using EtoIcon = Eto.Drawing.Icon;
using ModelIcon = ZeroInstall.Model.Icon;

namespace ZeroInstall.Commands.EtoForms;

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
    public EtoIcon? Icon { get; }

    /// <summary>
    /// An optional splash screen to display during downloads, etc..
    /// </summary>
    public Bitmap? SplashScreen { get; }

    /// <summary>
    /// Loads visual branding information for a specific <paramref name="feedUri"/> or falls back to defaults.
    /// </summary>
    public FeedBranding(FeedUri? feedUri)
    {
        var feed = feedUri?.To(LoadFeed);
        Name = feed?.Name;
        AppId = feed?.GetEntryPoint(Command.NameRun)?.AppId;

        // .ico files are only reliably supported on Windows
        string iconMimeType = WindowsUtils.IsWindows ? ModelIcon.MimeTypeIco : ModelIcon.MimeTypePng;
        Icon = feed
             ?.Icons.GetIcon(iconMimeType)
             ?.To(TryGetIconPath)
             ?.To(path => LoadIcon(path, iconMimeType))
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

    private static string? TryGetIconPath(ModelIcon icon)
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

    private static EtoIcon? LoadIcon(string path, string mimeType)
    {
        try
        {
            return mimeType == ModelIcon.MimeTypeIco
                ? new EtoIcon(path)
                : new EtoIcon(1, new Bitmap(path));
        }
        #region Error handling
        catch (Exception ex)
        {
            Log.Warn($"Failed to load icon '{path}'", ex);
            return null;
        }
        #endregion
    }

    private static EtoIcon? LoadDefaultIcon()
    {
        try
        {
            return new(typeof(FeedBranding).GetEmbeddedStream("icon.ico"));
        }
        #region Error handling
        catch (Exception ex)
        {
            Log.Warn("Failed to load default icon", ex);
            return null;
        }
        #endregion
    }

    private static Bitmap? LoadSplashScreen(string path)
    {
        try
        {
            // Not passing a file path or stream to ensure eager file validation
            return new(File.ReadAllBytes(path));
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
