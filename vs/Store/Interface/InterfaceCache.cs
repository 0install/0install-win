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
using System.IO;
using System.Web;
using Common.Storage;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Interface
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
    /// Manages the <see cref="Interface"/> cache.
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
        /// Set to <see langword="true"/> to force all cached <see cref="Interface"/>s to be updated, event if <see cref="MaximumAge"/> hasn't been reached yet.
        /// </summary>
        /// <remarks>This will be ignored if <see cref="NetworkLevel"/> is set to <see cref="Store.Interface.NetworkLevel.Offline"/>.</remarks>
        public bool Refresh { get; set; }

        /// <summary>
        /// The maximum age a cached <see cref="Interface"/> may have until it is considered stale (needs to be updated).
        /// </summary>
        /// <remarks>This will be ignored if <see cref="NetworkLevel"/> is set to <see cref="Store.Interface.NetworkLevel.Offline"/>.</remarks>
        public int MaximumAge { get; set; }
        #endregion

        //--------------------//

        #region Get
        /// <summary>
        /// Gets an <see cref="Interface"/> from the local cache or downloads it.
        /// </summary>
        /// <param name="feed">The URI used to identify (and potentially download) the <see cref="Interface"/> or a local path to directly load the file from.</param>
        /// <returns>The parsed <see cref="Interface"/> object.</returns>
        // ToDo: Add exceptions (file not found, GPG key invalid, ...)
        public Model.Interface GetFeed(string feed)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feed)) throw new ArgumentNullException("feed");
            #endregion

            if (Uri.IsWellFormedUriString(feed, UriKind.Absolute))
            {
                // Get from cache or download from internet
                string urlEncoded = HttpUtility.UrlEncode(feed);
                if (string.IsNullOrEmpty(urlEncoded)) throw new ArgumentException("Invalid URL", "feed");

                string path = Path.Combine(UserProfileDirectory, urlEncoded);

                // ToDo: Implement downloading
                if (!File.Exists(path)) throw new FileNotFoundException("Feed not in cache", "path");

                return Model.Interface.Load(path);
            }

            // Load local file
            return Model.Interface.Load(feed);
        }
        #endregion
    }
}
