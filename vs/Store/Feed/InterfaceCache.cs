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
using System.IO;
using System.Web;
using Common.Storage;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Feed
{
    #region Enumerations
    /// <summary>
    /// Controls how liberally network access is attempted.
    /// </summary>
    /// <see cref="InterfaceCache.NetworkLevel"/>
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
    /// Manages the interface cache.
    /// </summary>
    public class InterfaceCache
    {
        #region Properties
        /// <summary>
        /// The default directory in the user-profile to use for storing the cache.
        /// </summary>
        public static string UserProfileDirectory
        {
            get { return Path.Combine(Locations.GetUserCacheDir("0install.net"), "interfaces"); }
        }
        
        /// <summary>
        /// The directory containing the cached <see cref="Feed"/>s.
        /// </summary>
        public string DirectoryPath { get; private set; }

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
        /// Set to <see langword="true"/> to force all cached <see cref="Feed"/>s to be updated, event if <see cref="MaximumAge"/> hasn't been reached yet.
        /// </summary>
        /// <remarks>This will be ignored if <see cref="NetworkLevel"/> is set to <see cref="Store.Feed.NetworkLevel.Offline"/>.</remarks>
        public bool Refresh { get; set; }

        /// <summary>
        /// The maximum age a cached <see cref="Feed"/> may have until it is considered stale (needs to be updated).
        /// </summary>
        /// <remarks>This will be ignored if <see cref="NetworkLevel"/> is set to <see cref="Store.Feed.NetworkLevel.Offline"/>.</remarks>
        public int MaximumAge { get; set; }

        /// <summary>
        /// A callback object used if the the user needs to be asked any questions (such as whether to trust a certain GPG key).
        /// </summary>
        public IFeedHandler Handler { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new cache based on the given path to a cache directory.
        /// </summary>
        /// <param name="handler">A callback object used if the the user needs to be asked any questions (such as whether to trust a certain GPG key).</param>
        /// <param name="path">A fully qualified directory path. The directory will be created if it doesn't exist yet.</param>
        public InterfaceCache(IFeedHandler handler, string path)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion
            
            Handler = handler;

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            DirectoryPath = path;
        }

        /// <summary>
        /// Creates a new cache using a directory in the user-profile.
        /// </summary>
        /// <param name="handler">A callback object used if the the user needs to be asked any questions (such as whether to trust a certain GPG key).</param>
        public InterfaceCache(IFeedHandler handler) : this(handler, UserProfileDirectory)
        {}
        #endregion

        //--------------------//

        #region Get feed
        /// <summary>
        /// Gets a <see cref="Feed"/> from the local cache or downloads it.
        /// </summary>
        /// <param name="feed">The URI used to identify (and potentially download) the <see cref="Feed"/> or a local path to directly load the file from.</param>
        /// <returns>The parsed <see cref="Feed"/> object.</returns>
        // ToDo: Add exceptions (file not found, GPG key invalid, ...)
        public Model.Feed GetFeed(string feed)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feed)) throw new ArgumentNullException("feed");
            #endregion

            if (Uri.IsWellFormedUriString(feed, UriKind.Absolute))
            {
                // Get from cache or download from internet
                string urlEncoded = HttpUtility.UrlEncode(feed);
                if (string.IsNullOrEmpty(urlEncoded)) throw new ArgumentException(Resources.InvalidUrl, "feed");

                string path = Path.Combine(DirectoryPath, urlEncoded);

                // ToDo: Implement downloading
                if (!File.Exists(path)) throw new FileNotFoundException(string.Format(Resources.FeedNotInCache, feed), path);

                return Model.Feed.Load(path);
            }

            // Load local file
            return Model.Feed.Load(feed);
        }
        #endregion

        #region List interfaces
        /// <summary>
        /// Returns a list of all interfaces for <see cref="Feed"/>s stored in this cache.
        /// </summary>
        /// <returns>A list of interface URIs (e.g. "http://somedomain.net/interface.xml") in C-sorted order (ordinal comparison, increasing).</returns>
        public IEnumerable<string> ListAllInterfaces()
        {
            // Find all files whose names begin with an URL protocol
            string[] files = Directory.GetFiles(DirectoryPath, "http*");

            for (int i = 0; i < files.Length; i++)
                // Take the file name itself and use URL encoding to get the original URI
                files[i] = HttpUtility.UrlDecode(Path.GetFileName(files[i]));

            // Return as a C-sorted array
            Array.Sort(files, StringComparer.Ordinal);
            return files;
        }
        #endregion

        /// <summary>
        /// Loads all <see cref="Feed"/>s currently in this cache.
        /// </summary>
        /// <returns>A list of <see cref="Feed"/>s in no guaranteed order.</returns>
        public IEnumerable<Model.Feed> GetAllFeeds()
        {
            // Find all files whose names begin with an URL protocol
            string[] files = Directory.GetFiles(DirectoryPath, "http*");

            var feeds = new Model.Feed[files.LongLength];
            for (int i = 0; i < files.Length; i++)
                feeds[i] = Model.Feed.Load(files[i]);

            return feeds;
        }
    }
}
