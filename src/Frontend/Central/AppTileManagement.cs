/*
 * Copyright 2010-2015 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Dispatch;
using ZeroInstall.Central.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Central
{
    /// <summary>
    /// Helper methods for keeping <see cref="IAppTileList"/>s in sync with <see cref="AppList"/>s and <see cref="Catalog"/>s.
    /// </summary>
    public class AppTileManagement
    {
        #region Variables
        private readonly IAppTileList _tileListMyApps, _tileListCatalog;
        private readonly bool _machineWide;
        #endregion

        #region Properties
        private AppList _appList = new AppList();

        /// <summary>
        /// Stores the data currently displayed in <see cref="_tileListMyApps"/>.
        /// Used for comparison/merging when updating the list.
        /// </summary>
        public AppList AppList { get { return _appList; } }

        private Catalog _catalog = new Catalog();

        /// <summary>
        /// Stores the data currently displayed in <see cref="_tileListCatalog"/>.
        /// Used for comparison/merging when updating the list.
        /// </summary>
        public Catalog Catalog { get { return _catalog; } }
        #endregion

        #region Dependencies
        private readonly IFeedManager _feedManager;
        private readonly ICatalogManager _catalogManager;

        /// <summary>
        /// Creates a new tile manager.
        /// </summary>
        /// <param name="feedManager">Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.</param>
        /// <param name="catalogManager">Provides access to remote and local <see cref="Catalog"/>s. Handles downloading, signature verification and caching.</param>
        /// <param name="tileListMyApps">The <see cref="IAppTileList"/> used to represent the "my apps" <see cref="AppList"/>.</param>
        /// <param name="tileListCatalog">The <see cref="IAppTileList"/> used to represent the merged <see cref="Catalog"/>.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        public AppTileManagement([NotNull] IFeedManager feedManager, [NotNull] ICatalogManager catalogManager, [NotNull] IAppTileList tileListMyApps, [NotNull] IAppTileList tileListCatalog, bool machineWide)
        {
            #region Sanity checks
            if (feedManager == null) throw new ArgumentNullException("feedManager");
            if (catalogManager == null) throw new ArgumentNullException("catalogManager");
            if (tileListMyApps == null) throw new ArgumentNullException("tileListMyApps");
            if (tileListCatalog == null) throw new ArgumentNullException("tileListCatalog");
            #endregion

            _feedManager = feedManager;
            _catalogManager = catalogManager;

            _tileListMyApps = tileListMyApps;
            _tileListCatalog = tileListCatalog;
            _machineWide = machineWide;
        }
        #endregion

        //--------------------//

        #region MyApps
        /// <summary>
        /// Loads the current <see cref="AppList"/> from the disk and updates the "My Apps" <see cref="IAppTileList"/>.
        /// </summary>
        /// <returns>A list of <see cref="IAppTile"/>s that need to be injected with <see cref="Feed"/> data.</returns>
        [NotNull, ItemNotNull]
        public IEnumerable<IAppTile> UpdateMyApps()
        {
            var newAppList = AppList.LoadSafe(_machineWide);
            var tiles = new List<IAppTile>();

            // Update the displayed AppList based on changes detected between the current and the new AppList
            Merge.TwoWay(
                theirs: newAppList.Entries, mine: _appList.Entries,
                added: entry =>
                {
                    if (entry.InterfaceUri == null || entry.Name == null) return;
                    try
                    {
                        var status = (entry.AccessPoints == null) ? AppStatus.Added : AppStatus.Integrated;
                        var tile = _tileListMyApps.QueueNewTile(entry.InterfaceUri, entry.Name, status, _machineWide);
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
            _appList = newAppList;

            return tiles;
        }

        /// <summary>
        /// Calls <see cref="IFeedManager.GetFeed"/>.
        /// </summary>
        /// <returns>The loaded <see cref="Feed"/>; <see langword="null"/> on error.</returns>
        [CanBeNull]
        public Feed LoadFeedSafe([NotNull] FeedUri feedUri)
        {
            try
            {
                return _feedManager.GetFeedFresh(feedUri);
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (UriFormatException ex)
            {
                Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, feedUri));
                Log.Warn(ex);
                return null;
            }
            catch (IOException ex)
            {
                Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, feedUri));
                Log.Warn(ex);
                return null;
            }
            catch (WebException ex)
            {
                Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, feedUri));
                Log.Warn(ex);
                return null;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, feedUri));
                Log.Warn(ex);
                return null;
            }
            catch (SignatureException ex)
            {
                Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, feedUri));
                Log.Warn(ex);
                return null;
            }
            #endregion
        }
        #endregion

        #region Catalog
        /// <summary>
        /// Loads a cached version of the catalog from the disk and passes it to <see cref="SetCatalog"/>.
        /// </summary>
        public void LoadCachedCatalog()
        {
            SetCatalog(_catalogManager.GetCachedSafe());
        }

        /// <summary>
        /// Runs <see cref="CatalogManager.GetOnline"/> and returns the result.
        /// </summary>
        /// <remarks>This should be executed on a background worker thread and the result passed to <see cref="SetCatalog"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performs network IO and has side-effects")]
        [NotNull]
        public Catalog GetCatalogOnline()
        {
            return _catalogManager.GetOnline();
        }

        /// <summary>
        /// Updates the displayed catalog list based on changes detected between the current and the new catalog.
        /// </summary>
        public void SetCatalog([NotNull] Catalog newCatalog)
        {
            #region Sanity checks
            if (newCatalog == null) throw new ArgumentNullException("newCatalog");
            #endregion

            Merge.TwoWay(
                theirs: newCatalog.Feeds, mine: _catalog.Feeds,
                added: QueueCatalogTile,
                removed: feed => _tileListCatalog.RemoveTile(feed.Uri));
            _tileListCatalog.AddQueuedTiles();
            _tileListCatalog.ShowCategories();

            _catalog = newCatalog;
        }

        /// <summary>
        /// Creates a new tile for the <paramref name="feed"/> and queues it for adding on the <see cref="_tileListCatalog"/>.
        /// </summary>
        private void QueueCatalogTile([NotNull] Feed feed)
        {
            if (string.IsNullOrEmpty(feed.UriString) || feed.Name == null) return;
            try
            {
                var appEntry = _appList.GetEntry(feed.Uri);

                var status = (appEntry == null)
                    ? AppStatus.Candidate
                    : ((appEntry.AccessPoints == null) ? AppStatus.Added : AppStatus.Integrated);
                var tile = _tileListCatalog.QueueNewTile(feed.Uri, feed.Name, status, _machineWide);
                tile.Feed = feed;
            }
                #region Error handling
            catch (InvalidOperationException)
            {
                Log.Warn(string.Format(Resources.IgnoringDuplicateAppListEntry, feed.Uri));
            }
            #endregion
        }
        #endregion
    }
}
