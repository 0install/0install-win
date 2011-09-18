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
using System.Net;
using Common;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector.Feeds
{
    /// <summary>
    /// Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.
    /// </summary>
    public class FeedManager : IEquatable<FeedManager>, ICloneable
    {
        #region Properties
        /// <summary>
        /// The cache to retreive <see cref="Feed"/>s from and store downloaded <see cref="Feed"/>s to.
        /// </summary>
        public IFeedCache Cache { get; private set; }

        /// <summary>
        /// The OpenPGP-compatible system used to validate new <see cref="Feed"/>s signatures.
        /// </summary>
        public IOpenPgp OpenPgp { get; private set; }

        /// <summary>
        /// Set to <see langword="true"/> to update already cached <see cref="Feed"/>s. 
        /// </summary>
        public bool Refresh { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new cache based on the given path to a cache directory.
        /// </summary>
        /// <param name="cache">The disk-based cache to store downloaded <see cref="Feed"/>s.</param>
        /// <param name="openPgp">The OpenPGP-compatible system used to validate new <see cref="Feed"/>s signatures.</param>
        public FeedManager(IFeedCache cache,  IOpenPgp openPgp)
        {
            #region Sanity checks
            if (cache == null) throw new ArgumentNullException("cache");
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            #endregion

            Cache = cache;
            OpenPgp = openPgp;
        }
        #endregion

        //--------------------//

        #region Get feed
        /// <summary>
        /// Returns a specific <see cref="Feed"/>.
        /// </summary>
        /// <param name="feedID">The canonical ID used to identify the feed.</param>
        /// <param name="policy">Combines UI access, configuration and resources used to solve dependencies and download implementations.</param>
        /// <param name="stale">Indicates that the returned <see cref="Feed"/> should be updated.</param>
        /// <returns>The parsed <see cref="Feed"/> object.</returns>
        /// <remarks><see cref="Feed"/>s are always served from the <see cref="Cache"/> if possible, unless <see cref="Refresh"/> is set to <see langword="true"/>.</remarks>
        /// <exception cref="UserCancelException">Thrown if the user canceled the process.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown if <paramref name="feedID"/> is an invalid interface ID.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while reading the feed file.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while fetching the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the cache is not permitted.</exception>
        /// <exception cref="SignatureException">Thrown if the signature data of a feed file could not be handled.</exception>
        public Feed GetFeed(string feedID, Policy policy, out bool stale)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            // Assume invalid URIs are local paths
            Uri feedUrl;
            if (!ModelUtils.TryParseUri(feedID, out feedUrl))
            {
                stale = false;
                return LoadLocalFeed(feedID);
            }

            try
            {
                if (Refresh) DownloadFeed(feedUrl, policy);
                else if (!Cache.Contains(feedID))
                {
                    // Do not download in offline mode
                    if (policy.Config.NetworkUse == NetworkLevel.Offline)
                        throw new IOException(string.Format(Resources.FeedNotCachedOffline, feedID));

                    // Try to download missing feed
                    DownloadFeed(feedUrl, policy);
                }

                return LoadCachedFeed(feedID, policy, out stale);
            }
            #region Error handling
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(ex.Message, ex);
            }
            #endregion
        }
        #endregion

        #region Local
        /// <summary>
        /// Loads a <see cref="Feed"/> from a local file.
        /// </summary>
        /// <param name="feedID">The ID used to identify the feed. Must be an absolute local path.</param>
        /// <returns>The parsed <see cref="Feed"/> object.</returns>
        /// <exception cref="IOException">Thrown if a problem occured while reading the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the cache is not permitted.</exception>
        private Feed LoadLocalFeed(string feedID)
        {
            if (File.Exists(feedID))
            {
                // Use cache even for local files since there may be in-memory caching
                try { return Cache.GetFeed(feedID); }
                #region Error handling
                catch (InvalidDataException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    throw new IOException(ex.Message, ex);
                }
                #endregion
            }
            throw new FileNotFoundException(string.Format(Resources.FileNotFound, feedID), feedID);
        }
        #endregion

        #region Cached
        /// <summary>
        /// Loads a <see cref="Feed"/> from the <see cref="Cache"/>.
        /// </summary>
        /// <param name="feedID">The ID used to identify the feed. Must be an HTTP(S) URL.</param>
        /// <param name="policy">Combines UI access, configuration and resources used to solve dependencies and download implementations.</param>
        /// <param name="stale">Indicates that the returned <see cref="Feed"/> should be updated.</param>
        /// <returns>The parsed <see cref="Feed"/> object.</returns>
        /// <exception cref="InvalidInterfaceIDException">Thrown if <paramref name="feedID"/> is an invalid interface ID.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the requested <paramref name="feedID"/> was not found in the cache.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while reading the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the cache is not permitted.</exception>
        private Feed LoadCachedFeed(string feedID, Policy policy, out bool stale)
        {
            // Detect when feeds get out-of-date
            var preferences = FeedPreferences.LoadForSafe(feedID);
            TimeSpan feedAge = DateTime.UtcNow - preferences.LastChecked;
            stale = (feedAge > policy.Config.Freshness);

            try { return Cache.GetFeed(feedID); }
            #region Error handling
            catch (InvalidDataException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(ex.Message, ex);
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(ex.Message, ex);
            }
            #endregion
        }
        #endregion

        #region Download
        private static readonly WebClient _webClient = new WebClient();

        /// <summary>
        /// Downloads a <see cref="Feed"/> into the <see cref="Cache"/> validating its signatures.
        /// </summary>
        /// <param name="feedUrl">The URL of the feed to download.</param>
        /// <param name="policy">Combines UI access, configuration and resources used to solve dependencies and download implementations.</param>
        /// <exception cref="UserCancelException">Thrown if the user canceled the process.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown if <paramref name="feedUrl"/> is an invalid interface ID.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the requested <paramref name="feedUrl"/> was not found in the cache.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while reading the feed file.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while fetching the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the cache is not permitted.</exception>
        private void DownloadFeed(Uri feedUrl, Policy policy)
        {
            // HACK: Use the external solver to download feeds to the cache
            bool stale;
            try
            {
                policy.Solver.Solve(new Requirements {InterfaceID = feedUrl.ToString()}, policy, out stale);
            }
            catch (SolverException)
            {
                // Solver exceptions do not matter, since we only want the feed to get cached and do not care about selections
            }
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
            return new FeedManager(Cache, OpenPgp) {Refresh = Refresh};
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
