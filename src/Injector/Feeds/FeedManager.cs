/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.IO;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector.Feeds
{
    /// <summary>
    /// Provides access to remote and local <see cref="Model.Feed"/>s via interface IDs. Handles downloading, signature verification and caching.
    /// </summary>
    public class FeedManager : IEquatable<FeedManager>
    {
        #region Properties
        /// <summary>
        /// The cache to retreive from <see cref="Model.Feed"/>s to and store downloaded <see cref="Model.Feed"/>s to.
        /// </summary>
        public IFeedCache Cache { get; private set; }

        /// <summary>
        /// Set to <see langword="true"/> to update already cached <see cref="Model.Feed"/>s. 
        /// </summary>
        public bool Refresh { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new cache based on the given path to a cache directory.
        /// </summary>
        /// <param name="cache">The disk-based cache to store downloaded <see cref="Model.Feed"/>s.</param>
        public FeedManager(IFeedCache cache)
        {
            #region Sanity checks
            if (cache == null) throw new ArgumentNullException("cache");
            #endregion

            Cache = cache;
        }
        #endregion

        //--------------------//

        #region Get feeds
        /// <summary>
        /// Returns a list of all <see cref="Model.Feed"/>s applicable to a specific interface URI.
        /// </summary>
        /// <param name="interfaceID">The ID used to identify the interface (and primary feed; additional ones may be registered). May be an HTTP(S) URL or an absolute local path.</param>
        /// <param name="policy">Combines UI access, preferences and resources used to solve dependencies and download implementations.</param>
        /// <param name="staleFeeds">Indicates that one or more of the selected <see cref="Model.Feed"/>s should be updated.</param>
        /// <returns>The parsed <see cref="Model.Feed"/> objects.</returns>
        /// <remarks><see cref="Model.Feed"/>s are always served from the <see cref="Cache"/> if possible, unless <see cref="Refresh"/> is set to <see langword="true"/>.</remarks>
        // ToDo: Add exceptions (file not found, GPG key invalid, ...)
        public IEnumerable<Model.Feed> GetFeeds(string interfaceID, Policy policy, out bool staleFeeds)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            // ToDo: Get other registered feeds as well

            if (Refresh)
            {
                // ToDo: Download, verify and cache feed
            }

            // Try to load cached feed
            if (Cache.Contains(interfaceID))
            {
                // ToDo: Detect when feeds get out-of-date
                staleFeeds = false;

                return new[] {Cache.GetFeed(interfaceID)};
            }

            if (policy.Preferences.NetworkLevel == NetworkLevel.Offline)
                throw new FileNotFoundException(string.Format(Resources.FeedNotInCache, interfaceID), interfaceID);

            // ToDo: Download, verify and cache feed
            throw new FileNotFoundException(string.Format(Resources.FeedNotInCache, interfaceID), interfaceID);
        }
        #endregion
        
        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a shallow copy of this <see cref="FeedManager"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="FeedManager"/>.</returns>
        public FeedManager CloneFeedManager()
        {
            return new FeedManager(Cache) {Refresh = Refresh};
        }
        
        /// <summary>
        /// Creates a shallow copy of this <see cref="FeedManager"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="FeedManager"/>.</returns>
        public object Clone()
        {
            return CloneFeedManager();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(FeedManager other)
        {
            if (other == null) return false;

            return Refresh == other.Refresh && Equals(other.Cache, Cache);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(FeedManager) && Equals((FeedManager)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Cache != null ? Cache.GetHashCode() : 0) * 397) ^ Refresh.GetHashCode();
            }
        }
        #endregion
    }
}
