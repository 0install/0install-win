﻿/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Net;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector.Feeds
{
    /// <summary>
    /// A common base class for feed managers. Implements properties, cloning and equating.
    /// Does not implement <see cref="GetFeed(string)"/> and <see cref="ImportFeed"/>.
    /// </summary>
    public abstract class FeedManagerBase : IFeedManager, ICloneable, IEquatable<FeedManagerBase>
    {
        #region Dependencies
        /// <summary>
        /// User settings controlling network behaviour, solving, etc.
        /// </summary>
        protected readonly Config Config;

        /// <summary>
        /// The disk-based cache to store downloaded <see cref="Feed"/>s.
        /// </summary>
        protected readonly IFeedCache Cache;

        /// <summary>
        /// Creates a new cache based on the given path to a cache directory.
        /// </summary>
        /// <param name="config">User settings controlling network behaviour, solving, etc.</param>
        /// <param name="cache">The disk-based cache to store downloaded <see cref="Feed"/>s.</param>
        protected FeedManagerBase(Config config, IFeedCache cache)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            if (cache == null) throw new ArgumentNullException("cache");
            #endregion

            Config = config;
            Cache = cache;
        }
        #endregion

        #region Properties
        /// <inheritdoc/>
        public bool Refresh { get; set; }
        #endregion

        //--------------------//

        #region Get feed
        /// <inheritdoc/>
        public abstract Feed GetFeed(string feedID, ref bool stale);

        /// <inheritdoc/>
        public Feed GetFeed(string feedID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            #endregion

            bool stale = false;
            var feed = GetFeed(feedID, ref stale);

            // Detect outdated feed
            if (stale && !Refresh && Config.EffectiveNetworkUse == NetworkLevel.Full)
            {
                Refresh = true;
                try
                {
                    feed = GetFeed(feedID, ref stale);
                }
                catch (WebException)
                {
                    // Ignore missing internet connection and just keep outdated feed for now
                }
                Refresh = false;
            }

            return feed;
        }
        #endregion

        #region Import feed
        /// <inheritdoc/>
        public abstract void ImportFeed(Uri uri, Uri mirrorUri, byte[] data);
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a shallow copy of this feed manager.
        /// </summary>
        /// <returns>The new copy of the feed manager.</returns>
        public IFeedManager Clone()
        {
            return (IFeedManager)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(FeedManagerBase other)
        {
            if (other == null) return false;
            return Refresh == other.Refresh && Equals(other.Cache, Cache);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(FeedManagerBase) && Equals((FeedManagerBase)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = Refresh.GetHashCode();
                result = (result * 397) ^ Cache.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
