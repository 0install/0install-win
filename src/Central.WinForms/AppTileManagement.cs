// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using NanoByte.Common;
using NanoByte.Common.Dispatch;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Model;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.Icons;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// Helper methods for keeping <see cref="IAppTileList"/>s in sync with <see cref="AppList"/>s and <see cref="Catalog"/>s.
    /// </summary>
    public class AppTileManagement
    {
        #region Dependencies
        private readonly IFeedManager _feedManager;
        private readonly ICatalogManager _catalogManager;
        private readonly IIconStore _iconStore;

        private readonly IAppTileList _tileListMyApps, _tileListCatalog;
        private readonly bool _machineWide;

        /// <summary>
        /// Creates a new tile manager.
        /// </summary>
        /// <param name="feedManager">Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.</param>
        /// <param name="catalogManager">Provides access to remote and local <see cref="Catalog"/>s. Handles downloading, signature verification and caching.</param>
        /// <param name="iconStore">The icon store used by newly created <see cref="IAppTile"/>s to retrieve application icons.</param>
        /// <param name="tileListMyApps">The <see cref="IAppTileList"/> used to represent the "my apps" <see cref="AppList"/>.</param>
        /// <param name="tileListCatalog">The <see cref="IAppTileList"/> used to represent the merged <see cref="Catalog"/>.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        public AppTileManagement(IFeedManager feedManager, ICatalogManager catalogManager, IIconStore iconStore, IAppTileList tileListMyApps, IAppTileList tileListCatalog, bool machineWide)
        {
            _feedManager = feedManager ?? throw new ArgumentNullException(nameof(feedManager));
            _catalogManager = catalogManager ?? throw new ArgumentNullException(nameof(catalogManager));
            _iconStore = iconStore ?? throw new ArgumentNullException(nameof(iconStore));

            _tileListMyApps = tileListMyApps ?? throw new ArgumentNullException(nameof(tileListMyApps));
            _tileListCatalog = tileListCatalog ?? throw new ArgumentNullException(nameof(tileListCatalog));
            _machineWide = machineWide;
        }
        #endregion

        /// <summary>
        /// Stores the data currently displayed in <see cref="_tileListMyApps"/>.
        /// Used for comparison/merging when updating the list.
        /// </summary>
        public AppList AppList { get; private set; } = new();

        /// <summary>
        /// Loads the current <see cref="AppList"/> from the disk and updates the "My Apps" <see cref="IAppTileList"/>.
        /// </summary>
        /// <returns>A list of <see cref="IAppTile"/>s that need to be injected with <see cref="Feed"/> data.</returns>
        public IEnumerable<IAppTile> UpdateMyApps()
        {
            var newAppList = AppList.LoadSafe(_machineWide);
            var tiles = new List<IAppTile>();

            // Update the displayed AppList based on changes detected between the current and the new AppList
            Merge.TwoWay(
                theirs: newAppList.Entries, mine: AppList.Entries,
                added: entry =>
                {
                    try
                    {
                        var status = (entry.AccessPoints == null) ? AppStatus.Added : AppStatus.Integrated;
                        var tile = _tileListMyApps.QueueNewTile(entry.InterfaceUri, entry.Name, status, _iconStore, _machineWide);
                        tiles.Add(tile);

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
                    if (catalogTile != null) catalogTile.Status = AppStatus.Candidate;
                });
            _tileListMyApps.AddQueuedTiles();
            AppList = newAppList;

            return tiles;
        }

        /// <summary>
        /// Calls <see cref="FeedManagerExtensions.GetFresh"/>.
        /// </summary>
        /// <returns>The loaded <see cref="Feed"/>; <c>null</c> on error.</returns>
        public Feed? LoadFeedSafe(FeedUri feedUri)
        {
            try
            {
                return _feedManager.GetFresh(feedUri);
            }
            #region Error handling
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex) when (ex is UriFormatException or IOException or WebException or WebException or UnauthorizedAccessException or SignatureException or InvalidDataException)
            {
                Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, feedUri));
                Log.Warn(ex);
                return null;
            }
            #endregion
        }

        /// <summary>
        /// Stores the data currently displayed in <see cref="_tileListCatalog"/>.
        /// Used for comparison/merging when updating the list.
        /// </summary>
        public Catalog Catalog { get; private set; } = new();

        /// <summary>
        /// Loads a cached version of the catalog from the disk and passes it to <see cref="SetCatalog"/>.
        /// </summary>
        public void LoadCachedCatalog() => SetCatalog(_catalogManager.GetCachedSafe());

        /// <summary>
        /// Runs <see cref="ICatalogManager.GetOnline"/> and returns the result.
        /// </summary>
        /// <remarks>This should be executed on a background worker thread and the result passed to <see cref="SetCatalog"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performs network IO and has side-effects")]
        public Catalog GetCatalogOnline() => _catalogManager.GetOnline();

        /// <summary>
        /// Updates the displayed catalog list based on changes detected between the current and the new catalog.
        /// </summary>
        public void SetCatalog(Catalog newCatalog)
        {
            #region Sanity checks
            if (newCatalog == null) throw new ArgumentNullException(nameof(newCatalog));
            #endregion

            Merge.TwoWay(
                theirs: newCatalog.Feeds, mine: Catalog.Feeds,
                added: QueueCatalogTile,
                removed: feed =>
                {
                    if (feed.Uri != null) _tileListCatalog.RemoveTile(feed.Uri);
                });
            _tileListCatalog.AddQueuedTiles();
            _tileListCatalog.ShowCategories();

            Catalog = newCatalog;
        }

        /// <summary>
        /// Creates a new tile for the <paramref name="feed"/> and queues it for adding on the <see cref="_tileListCatalog"/>.
        /// </summary>
        private void QueueCatalogTile(Feed feed)
        {
            if (feed.Uri == null) return;
            try
            {
                var appEntry = AppList.GetEntry(feed.Uri);

                var status = (appEntry == null)
                    ? AppStatus.Candidate
                    : ((appEntry.AccessPoints == null) ? AppStatus.Added : AppStatus.Integrated);
                var tile = _tileListCatalog.QueueNewTile(feed.Uri, feed.Name, status, _iconStore, _machineWide);
                tile.Feed = feed;
            }
            #region Error handling
            catch (InvalidOperationException)
            {
                Log.Warn(string.Format(Resources.IgnoringDuplicateAppListEntry, feed.Uri));
            }
            #endregion
        }
    }
}
