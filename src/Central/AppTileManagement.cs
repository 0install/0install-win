/*
 * Copyright 2010-2014 Bastian Eicher
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
using Common;
using Common.Collections;
using Common.Storage;
using ZeroInstall.Backend;
using ZeroInstall.Central.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;
using ZeroInstall.Model;
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
        private readonly CatalogManager _catalogManager;

        /// <summary>
        /// Creates a new tile manager.
        /// </summary>
        /// <param name="feedManager">Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.</param>
        /// <param name="catalogManager">Provides access to remote and local <see cref="Catalog"/>s. Handles downloading, signature verification and caching.</param>
        /// <param name="tileListMyApps">The <see cref="IAppTileList"/> used to represent the "my apps" <see cref="AppList"/>.</param>
        /// <param name="tileListCatalog">The <see cref="IAppTileList"/> used to represent the merged <see cref="Catalog"/>.</param>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        public AppTileManagement(IFeedManager feedManager, CatalogManager catalogManager, IAppTileList tileListMyApps, IAppTileList tileListCatalog, bool machineWide)
        {
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
        public IEnumerable<IAppTile> UpdateMyApps()
        {
            var newAppList = LoadAppListSafe();
            var tiles = new List<IAppTile>();

            // Update the displayed AppList based on changes detected between the current and the new AppList
            Merge.TwoWay(
                theirs: newAppList.Entries, mine: _appList.Entries,
                added: entry =>
                {
                    if (string.IsNullOrEmpty(entry.InterfaceID) || entry.Name == null) return;
                    try
                    {
                        var status = (entry.AccessPoints == null) ? AppStatus.Added : AppStatus.Integrated;
                        var tile = _tileListMyApps.QueueNewTile(entry.InterfaceID, entry.Name, status, _machineWide);
                        tiles.Add(tile);

                        // Update "added" status of tile in catalog list
                        var catalogTile = _tileListCatalog.GetTile(entry.InterfaceID);
                        if (catalogTile != null) catalogTile.Status = tile.Status;
                    }
                        #region Error handling
                    catch (KeyNotFoundException)
                    {
                        Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, entry.InterfaceID));
                    }
                    catch (C5.DuplicateNotAllowedException)
                    {
                        Log.Warn(string.Format(Resources.IgnoringDuplicateAppListEntry, entry.InterfaceID));
                    }
                    #endregion
                },
                removed: entry =>
                {
                    _tileListMyApps.RemoveTile(entry.InterfaceID);

                    // Update "added" status of tile in catalog list
                    var catalogTile = _tileListCatalog.GetTile(entry.InterfaceID);
                    if (catalogTile != null) catalogTile.Status = AppStatus.Candidate;
                });
            _tileListMyApps.AddQueuedTiles();
            _appList = newAppList;

            return tiles;
        }

        /// <summary>
        /// Loads the current <see cref="AppList"/> from the disk.
        /// </summary>
        /// <returns>The loaded <see cref="AppList"/>; an empty <see cref="AppList"/> on error.</returns>
        private AppList LoadAppListSafe()
        {
            try
            {
                return XmlStorage.LoadXml<AppList>(AppList.GetDefaultPath(_machineWide));
            }
                #region Error handling
            catch (FileNotFoundException)
            {
                return new AppList();
            }
            catch (IOException ex)
            {
                Log.Warn(Resources.UnableToLoadAppList);
                Log.Warn(ex);
                return new AppList();
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn(Resources.UnableToLoadAppList);
                Log.Warn(ex);
                return new AppList();
            }
            catch (InvalidDataException ex)
            {
                Log.Warn(Resources.UnableToLoadAppList);
                Log.Warn(ex);
                return new AppList();
            }
            #endregion
        }

        /// <summary>
        /// Calls <see cref="IFeedManager.GetFeed"/>.
        /// </summary>
        /// <returns>The loaded <see cref="Feed"/>; <see langword="null"/> on error.</returns>
        public Feed LoadFeedSafe(string feedID)
        {
            try
            {
                return _feedManager.GetFeedFresh(feedID);
            }
                #region Error handling
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (InvalidInterfaceIDException ex)
            {
                Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, feedID));
                Log.Warn(ex);
                return null;
            }
            catch (IOException ex)
            {
                Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, feedID));
                Log.Warn(ex);
                return null;
            }
            catch (WebException ex)
            {
                Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, feedID));
                Log.Warn(ex);
                return null;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, feedID));
                Log.Warn(ex);
                return null;
            }
            catch (SignatureException ex)
            {
                Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, feedID));
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
            SetCatalog(_catalogManager.GetCached());
        }

        /// <summary>
        /// Runs <see cref="CatalogManager.GetOnline"/> and returns the result.
        /// </summary>
        /// <remarks>This should be executed on a background worker thread and the result passed to <see cref="SetCatalog"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performs network IO and has side-effects")]
        public Catalog GetCatalogOnline()
        {
            return _catalogManager.GetOnline();
        }

        /// <summary>
        /// Updates the displayed catalog list based on changes detected between the current and the new catalog.
        /// </summary>
        public void SetCatalog(Catalog newCatalog)
        {
            #region Sanity checks
            if (newCatalog == null) throw new ArgumentNullException("newCatalog");
            #endregion

            Merge.TwoWay(
                theirs: newCatalog.Feeds, mine: _catalog.Feeds,
                added: QueueCatalogTile,
                removed: feed => _tileListCatalog.RemoveTile(feed.UriString));
            _tileListCatalog.AddQueuedTiles();
            _tileListCatalog.ShowCategories();

            _catalog = newCatalog;
        }

        /// <summary>
        /// Creates a new tile for the <paramref name="feed"/> and queues it for adding on the <see cref="_tileListCatalog"/>.
        /// </summary>
        private void QueueCatalogTile(Feed feed)
        {
            if (string.IsNullOrEmpty(feed.UriString) || feed.Name == null) return;
            try
            {
                string interfaceID = feed.UriString;
                var status = _appList.Contains(interfaceID)
                    ? ((_appList[interfaceID].AccessPoints == null) ? AppStatus.Added : AppStatus.Integrated)
                    : AppStatus.Candidate;
                var tile = _tileListCatalog.QueueNewTile(interfaceID, feed.Name, status, _machineWide);
                tile.Feed = feed;
            }
                #region Error handling
            catch (C5.DuplicateNotAllowedException)
            {
                Log.Warn(string.Format(Resources.IgnoringDuplicateAppListEntry, feed.Uri));
            }
            #endregion
        }
        #endregion
    }
}
