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
using System.IO;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector.Feeds
{
    /// <summary>
    /// Provides access to remote and local <see cref="Model.Feed"/>s. Handles downloading, signature verification and caching.
    /// </summary>
    public class FeedManager : IEquatable<FeedManager>
    {
        #region Properties
        /// <summary>
        /// The cache to retreive <see cref="Model.Feed"/>s from and store downloaded <see cref="Model.Feed"/>s to.
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

        #region Get feed
        /// <summary>
        /// Returns a specific <see cref="Model.Feed"/>.
        /// </summary>
        /// <param name="feedID">The ID used to identify the feed. May be an HTTP(S) URL or an absolute local path.</param>
        /// <param name="policy">Combines UI access, configuration and resources used to solve dependencies and download implementations.</param>
        /// <param name="stale">Indicates that the returned <see cref="Model.Feed"/> should be updated.</param>
        /// <returns>The parsed <see cref="Model.Feed"/> object.</returns>
        /// <remarks><see cref="Model.Feed"/>s are always served from the <see cref="Cache"/> if possible, unless <see cref="Refresh"/> is set to <see langword="true"/>.</remarks>
        // ToDo: Add exceptions (file not found, GPG key invalid, ...)
        public Model.Feed GetFeed(string feedID, Policy policy, out bool stale)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            if (Refresh)
            {
                // ToDo: Download, verify and cache feed
            }

            // Try to load cached feed
            if (Cache.Contains(feedID))
            {
                // ToDo: Detect when feeds get out-of-date
                stale = false;

                return Cache.GetFeed(feedID);
            }

            if (policy.Config.NetworkUse == NetworkLevel.Offline)
                throw new FileNotFoundException(string.Format(Resources.FeedNotInCache, feedID), feedID);

            // ToDo: Download, verify and cache feed
            throw new FileNotFoundException(string.Format(Resources.FeedNotInCache, feedID), feedID);
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
