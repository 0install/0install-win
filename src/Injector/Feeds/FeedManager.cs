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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector.Feeds
{
    /// <summary>
    /// Provides access to remote and local <see cref="Model.Feed"/>s via interface URIs.
    /// Downloading, signature verification and caching are handled automatically.
    /// </summary>
    public class FeedManager : ICloneable, IEquatable<FeedManager>
    {
        #region Properties
        /// <summary>
        /// The disk-based cache to store downloaded <see cref="Model.Feed"/>s.
        /// </summary>
        public IFeedCache Cache { get; private set; }

        /// <summary>
        /// Set to <see langword="true"/> to prevent any new <see cref="Model.Feed"/>s from being downloaded, even if <see cref="Preferences.Freshness"/> has been exceeded.
        /// </summary>
        public bool Offline { get; set; }
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
        /// <param name="interfaceUri">The URI used to identify the interface. May be an HTTP(S) URL or a local path.</param>
        /// <param name="preferences">User-preferences controlling network behaviour, etc.</param>
        /// <param name="handler">A callback object used if the the user needs to be asked any questions (such as whether to trust a certain GPG key).</param>
        /// <returns>The parsed <see cref="Model.Feed"/> objects.</returns>
        /// <remarks>
        /// <paramref name="interfaceUri"/> is used to locate the primary <see cref="Model.Feed"/> for the interface.
        /// Aditional feed locations may be specified within the <see cref="Model.Feed"/> or by user preferences.
        /// </remarks>
        // ToDo: Add exceptions (file not found, GPG key invalid, ...)
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Allow for local paths as well")]
        public IEnumerable<Model.Feed> GetFeeds(string interfaceUri, Preferences preferences, IFeedHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceUri)) throw new ArgumentNullException("interfaceUri");
            if (preferences == null) throw new ArgumentNullException("preferences");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Try to load local file
            if (File.Exists(interfaceUri)) return new[] {Model.Feed.Load(interfaceUri)};

            // Try to load cached feed
            var url = new Uri(interfaceUri);
            if (/*!Refresh &&*/ Cache.Contains(url)) return new[] {Cache.GetFeed(url)};

            // ToDo: Download, verify and cache feed
            throw new FileNotFoundException(string.Format(Resources.FeedNotInCache, interfaceUri, "unknown"), interfaceUri);
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
            return new FeedManager(Cache) {Offline = Offline};
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

            return Offline == other.Offline && Equals(other.Cache, Cache);
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
                return ((Cache != null ? Cache.GetHashCode() : 0) * 397) ^ Offline.GetHashCode();
            }
        }
        #endregion
    }
}
