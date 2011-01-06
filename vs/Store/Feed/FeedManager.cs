/*
 * Copyright 2010 Bastian Eicher
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
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Feed
{
    #region Enumerations
    /// <summary>
    /// Controls how liberally network access is attempted.
    /// </summary>
    /// <see cref="FeedManager.NetworkLevel"/>
    public enum NetworkLevel
    {
        /// <summary>Do not access network at all.</summary>
        Offline,

        /// <summary>Only access network when there are no safe implementations available.</summary>
        Minimal,

        /// <summary>Always use network to get the newest available versions.</summary>
        Full
    }
    #endregion

    /// <summary>
    /// Provides access to remote and local <see cref="Model.Feed"/>s via interface URIs.
    /// Downloading, signature verification and caching are handled automatically.
    /// </summary>
    public class FeedManager
    {
        #region Properties
        /// <summary>
        /// The disk-based cache to store downloaded <see cref="Model.Feed"/>s.
        /// </summary>
        public IFeedCache Cache { get; private set; }

        private NetworkLevel _networkLevel = NetworkLevel.Full;
        /// <summary>
        /// Controls how liberally network access is attempted.
        /// </summary>
        public NetworkLevel NetworkLevel
        {
            get { return _networkLevel; }
            set
            {
                #region Sanity checks
                if (!Enum.IsDefined(typeof(NetworkLevel), value)) throw new ArgumentOutOfRangeException("value");
                #endregion

                _networkLevel = value;
            }
        }

        /// <summary>
        /// The maximum age a cached <see cref="Model.Feed"/> may have until it is considered stale (needs to be updated).
        /// </summary>
        /// <remarks>This will be ignored if <see cref="NetworkLevel"/> is set to <see cref="Store.Feed.NetworkLevel.Offline"/>.</remarks>
        public TimeSpan MaximumAge { get; set; }

        /// <summary>
        /// Set to <see langword="true"/> to force all cached <see cref="Model.Feed"/>s to be updated, event if <see cref="MaximumAge"/> hasn't been reached yet.
        /// </summary>
        /// <remarks>This will be ignored if <see cref="NetworkLevel"/> is set to <see cref="Feed.NetworkLevel.Offline"/>.</remarks>
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
        /// <param name="interfaceUri">The URI used to identify the interface. May be an HTTP(S) URL or a local path.</param>
        /// <param name="handler">A callback object used if the the user needs to be asked any questions (such as whether to trust a certain GPG key).</param>
        /// <returns>The parsed <see cref="Model.Feed"/> objects.</returns>
        /// <remarks>
        /// <paramref name="interfaceUri"/> is used to locate the primary <see cref="Model.Feed"/> for the interface.
        /// Aditional feed locations may be specified within the <see cref="Model.Feed"/> or by user preferences.
        /// </remarks>
        // ToDo: Add exceptions (file not found, GPG key invalid, ...)
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Allow for local paths as well")]
        public IEnumerable<Model.Feed> GetFeeds(string interfaceUri, IFeedHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceUri)) throw new ArgumentNullException("interfaceUri");
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
    }
}
