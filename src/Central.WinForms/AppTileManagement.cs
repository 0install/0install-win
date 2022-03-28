// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NanoByte.Common;
using NanoByte.Common.Dispatch;
using NanoByte.Common.Tasks;
using NanoByte.Common.Threading;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Model;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.Icons;
using ZeroInstall.Store.Trust;
using Icon = ZeroInstall.Model.Icon;

namespace ZeroInstall.Central.WinForms;

/// <summary>
/// Helper methods for keeping <see cref="AppTileList"/>s in sync with <see cref="AppList"/>s and <see cref="Catalog"/>s.
/// </summary>
public class AppTileManagement
{
    private readonly bool _machineWide;

    /// <summary>
    /// Creates a new tile manager.
    /// </summary>
    /// <param name="tileListMyApps">The <see cref="AppTileList"/> used to represent the "my apps" <see cref="AppList"/>.</param>
    /// <param name="tileListCatalog">The <see cref="AppTileList"/> used to represent the merged <see cref="Catalog"/>.</param>
    /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
    /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
    public AppTileManagement(AppTileList tileListMyApps, AppTileList tileListCatalog, bool machineWide, ITaskHandler handler)
    {
        var services = new ServiceProvider(handler);

        _tileListMyApps = tileListMyApps;
        _tileListCatalog = tileListCatalog;
        _tileListMyApps.MachineWide = _tileListCatalog.MachineWide = _machineWide = machineWide;

        _feedManager = services.FeedManager;
        _catalogManager = services.CatalogManager;
        _iconStore = IconStores.Cache(services.Config, services.Handler);
        _iconUpdateQueue = new(handler.CancellationToken);
    }

    private readonly AppTileList _tileListMyApps;
    private readonly IFeedManager _feedManager;
    private AppList _appList = new();

    /// <summary>
    /// Indicates whether the <see cref="AppList"/> currently contains no elements.
    /// </summary>
    public bool IsMyAppsEmpty => _appList.Entries.Count == 0;

    /// <summary>
    /// Loads the current <see cref="AppList"/> from the disk and updates the "My Apps" <see cref="AppTileList"/>.
    /// </summary>
    /// <returns>A list of <see cref="AppTile"/>s that need to be injected with <see cref="Feed"/> data.</returns>
    public async void UpdateMyApps()
    {
        var newAppList = AppList.LoadSafe(_machineWide);
        var newTiles = new List<AppTile>();

        // Update the displayed AppList based on changes detected between the current and the new AppList
        Merge.TwoWay(
            theirs: newAppList.Entries,
            mine: _appList.Entries,
            added: entry =>
            {
                try
                {
                    var status = (entry.AccessPoints == null) ? AppTileStatus.Added : AppTileStatus.Integrated;
                    var tile = _tileListMyApps.QueueNewTile(entry.InterfaceUri, entry.Name, status);
                    newTiles.Add(tile);

                    // Update "added" status of tile in catalog list
                    var catalogTile = _tileListCatalog.GetTile(entry.InterfaceUri);
                    if (catalogTile != null) catalogTile.Status = tile.Status;
                }
                #region Error handling
                catch (KeyNotFoundException)
                {
                    Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, entry.InterfaceUri));
                }
                catch (InvalidOperationException)
                {
                    Log.Warn(string.Format(Resources.IgnoringDuplicateAppListEntry, entry.InterfaceUri));
                }
                #endregion
            },
            removed: entry =>
            {
                _tileListMyApps.RemoveTile(entry.InterfaceUri);

                // Update "added" status of tile in catalog list
                var catalogTile = _tileListCatalog.GetTile(entry.InterfaceUri);
                if (catalogTile != null) catalogTile.Status = AppTileStatus.Candidate;
            });
        _tileListMyApps.AddQueuedTiles();
        _appList = newAppList;

        foreach (var tile in newTiles)
        {
            var feed = await Task.Run(() =>
            {
                try
                {
                    return _feedManager.GetFresh(tile.InterfaceUri);
                }
                #region Error handling
                catch (OperationCanceledException)
                {
                    return null;
                }
                catch (Exception ex) when (ex is UriFormatException or IOException or WebException or WebException or UnauthorizedAccessException or SignatureException or InvalidDataException)
                {
                    Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, tile.InterfaceUri));
                    Log.Warn(ex);
                    return null;
                }
                #endregion
            });
            if (feed != null)
            {
                tile.SetFeed(feed);
                SetIcon(tile, feed);
            }
        }
    }

    private readonly AppTileList _tileListCatalog;
    private readonly ICatalogManager _catalogManager;
    private Catalog _catalog = new();

    /// <summary>
    /// Loads a cached version of the catalog from the disk.
    /// </summary>
    public void LoadCachedCatalog()
        => SetCatalog(_catalogManager.GetCachedSafe());

    /// <summary>
    /// Updates the catalog from the web.
    /// </summary>
    /// <exception cref="IOException">A problem occurred while reading a local catalog file.</exception>
    /// <exception cref="UnauthorizedAccessException">Access to a local catalog file was not permitted.</exception>
    /// <exception cref="WebException">A problem occurred while fetching a remote catalog file.</exception>
    /// <exception cref="InvalidDataException">A problem occurred while deserializing the XML data.</exception>
    /// <exception cref="SignatureException">The signature data of a remote catalog file could not be verified.</exception>
    /// <exception cref="UriFormatException">An invalid catalog source is specified in the configuration file.</exception>
    public async Task UpdateCatalogAsync()
        => SetCatalog(await Task.Run(() => _catalogManager.GetOnline()));

    private void SetCatalog(Catalog newCatalog)
    {
        var newTiles = new List<(AppTile tile, Feed feed)>();

        Merge.TwoWay(
            theirs: newCatalog.Feeds,
            mine: _catalog.Feeds,
            added: feed =>
            {
                if (feed.Uri == null) return;
                try
                {
                    var appEntry = _appList.GetEntry(feed.Uri);

                    var status = (appEntry == null)
                        ? AppTileStatus.Candidate
                        : appEntry.AccessPoints == null ? AppTileStatus.Added : AppTileStatus.Integrated;
                    var tile = _tileListCatalog.QueueNewTile(feed.Uri, feed.Name, status);
                    tile.SetFeed(feed);
                    newTiles.Add((tile, feed));
                }
                #region Error handling
                catch (InvalidOperationException)
                {
                    Log.Warn(string.Format(Resources.IgnoringDuplicateAppListEntry, feed.Uri));
                }
                #endregion
            },
            removed: feed =>
            {
                if (feed.Uri != null) _tileListCatalog.RemoveTile(feed.Uri);
            });
        _tileListCatalog.AddQueuedTiles();
        _catalog = newCatalog;

        foreach (var (tile, feed) in newTiles)
        {
            SetIcon(tile, feed);
        }
    }

    private readonly IIconStore _iconStore;
    private readonly JobQueue _iconUpdateQueue;
    private readonly SemaphoreSlim _iconDownloadSemaphore = new(initialCount: 5);

    private async void SetIcon(AppTile tile, IIconContainer iconContainer)
    {
        var icon = iconContainer.Icons.GetIcon(Icon.MimeTypePng)
                ?? iconContainer.Icons.GetIcon(Icon.MimeTypeIco);

        if (icon == null)
        {
            tile.SetIcon(ImageResources.AppIcon);
            return;
        }

        await _iconDownloadSemaphore.WaitAsync();
        try
        {
            bool stale = false;
            string path = await Task.Run(() => _iconStore.Get(icon, out stale));

            if (stale)
            {
                // Copy icon into memory to avoid conflicts with background update
                tile.SetIcon(await Task.Run(() => Image.FromStream(new MemoryStream(File.ReadAllBytes(path)))));

                _iconUpdateQueue.Enqueue(() =>
                {
                    try
                    {
                        tile.BeginInvoke(
                            (Action<string>)tile.SetIcon,
                            _iconStore.GetFresh(icon));
                    }
                    #region Error handling
                    catch (OperationCanceledException)
                    {}
                    catch (WebException ex)
                    {
                        Log.Info(ex);
                    }
                    catch (InvalidOperationException) // AppTile already disposed
                    {}
                    catch (Exception ex)
                    {
                        Log.Warn(ex);
                    }
                    #endregion
                });
            }
            else tile.SetIcon(path);
        }
        #region Error handling
        catch (OperationCanceledException)
        {}
        catch (WebException ex)
        {
            Log.Info(ex);
        }
        catch (Exception ex)
        {
            Log.Warn(ex);
        }
        finally
        {
            _iconDownloadSemaphore.Release();
        }
        #endregion
    }
}
