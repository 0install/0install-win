/*
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using Common;
using Common.Storage;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector.Feeds
{
    /// <summary>
    /// Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.
    /// </summary>
    public class FeedManager : FeedManagerBase, IEquatable<FeedManager>
    {
        #region Constructor
        /// <summary>
        /// Creates a new cache based on the given path to a cache directory.
        /// </summary>
        /// <param name="cache">The disk-based cache to store downloaded <see cref="Feed"/>s.</param>
        public FeedManager(IFeedCache cache) : base(cache)
        {}
        #endregion

        //--------------------//

        #region Get feed
        /// <inheritdoc/>
        public override Feed GetFeed(string feedID, Policy policy, ref bool stale)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            // Assume invalid URIs are local paths
            Uri feedUrl;
            if (!ModelUtils.TryParseUri(feedID, out feedUrl))
                return LoadLocalFeed(feedID);

            try
            {
                if (Refresh) DownloadFeed(feedUrl, policy);
                else if (!Cache.Contains(feedID))
                {
                    // Do not download in offline mode
                    if (policy.Config.EffectiveNetworkUse == NetworkLevel.Offline)
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
                try
                {
                    return Cache.GetFeed(feedID);
                }
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
        /// Loads a <see cref="Feed"/> from the <see cref="FeedManagerBase.Cache"/>.
        /// </summary>
        /// <param name="feedID">The ID used to identify the feed. Must be an HTTP(S) URL.</param>
        /// <param name="policy">Provides additional class dependencies.</param>
        /// <param name="stale">Indicates that the returned <see cref="Feed"/> should be updated.</param>
        /// <returns>The parsed <see cref="Feed"/> object.</returns>
        /// <exception cref="InvalidInterfaceIDException">Thrown if <paramref name="feedID"/> is an invalid interface ID.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the requested <paramref name="feedID"/> was not found in the cache.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while reading the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the cache is not permitted.</exception>
        private Feed LoadCachedFeed(string feedID, Policy policy, out bool stale)
        {
            // Detect when feeds get out-of-date
            // ToDo: Evaluate caching the last check value somewhere
            var preferences = FeedPreferences.LoadForSafe(feedID);
            TimeSpan lastChecked = DateTime.UtcNow - preferences.LastChecked;
            TimeSpan lastCheckAttempt = DateTime.UtcNow - GetLastCheckAttempt(feedID);
            stale = (lastChecked > policy.Config.Freshness && lastCheckAttempt > _failedCheckDelay);

            try
            {
                return Cache.GetFeed(feedID);
            }
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
        /// <summary>
        /// Downloads a <see cref="Feed"/> into the <see cref="FeedManagerBase.Cache"/> validating its signatures.
        /// </summary>
        /// <param name="url">The URL of download the feed from.</param>
        /// <param name="policy">Provides additional class dependencies.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the process.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown if <paramref name="url"/> is an invalid interface ID.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the requested <paramref name="url"/> was not found in the cache.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while reading the feed file.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while fetching the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the cache is not permitted.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2200:RethrowToPreserveStackDetails", Justification = "Rethrow the outer instead of the inner exception")]
        private void DownloadFeed(Uri url, Policy policy)
        {
            SetLastCheckAttempt(url.ToString());

            // ToDo: Add tracking and better cancellation support
            policy.Handler.CancellationToken.ThrowIfCancellationRequested();
            using (var webClient = new WebClientTimeout())
            {
                try
                {
                    ImportFeed(url, null, webClient.DownloadData(url), policy);
                    policy.Handler.CancellationToken.ThrowIfCancellationRequested();
                }
                catch (WebException ex)
                {
                    if (url.Host == "localhost" || url.Host == "127.0.0.1") throw;
                    policy.Handler.CancellationToken.ThrowIfCancellationRequested();
                    Log.Warn(string.Format(Resources.FeedDownloadErrorSwitchToMirror, url, ex.Message));

                    var mirrorUrl = new Uri(policy.Config.FeedMirror, string.Format(
                        "feeds/{0}/{1}/{2}/latest.xml",
                        url.Scheme,
                        url.Host,
                        string.Concat(url.Segments).TrimStart('/').Replace("/", "%23")));
                    try
                    {
                        ImportFeed(url, mirrorUrl, webClient.DownloadData(mirrorUrl), policy);
                    }
                    catch (WebException)
                    {
                        // Report the original problem instead of mirror errors
                        throw ex;
                    }

                    policy.Handler.CancellationToken.ThrowIfCancellationRequested();
                }
            }
            policy.Handler.CancellationToken.ThrowIfCancellationRequested();
        }
        #endregion

        #region Import feed
        /// <inheritdoc/>
        public override void ImportFeed(Uri uri, Uri mirrorUri, byte[] data, Policy policy)
        {
            #region Sanity checks
            if (uri == null) throw new ArgumentNullException("uri");
            if (data == null) throw new ArgumentNullException("data");
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            var newSignature = TrustUtils.CheckTrust(uri, mirrorUri, data, policy);
            DetectAttacks(uri, data, policy, newSignature);

            // Add to cache and remember time
            Cache.Add(uri.ToString(), data);
            var preferences = FeedPreferences.LoadForSafe(uri.ToString());
            preferences.LastChecked = DateTime.UtcNow;
            preferences.Normalize();
            preferences.SaveFor(uri.ToString());
        }

        /// <summary>
        /// Detects attacks such as feed substitution or replay attacks.
        /// </summary>
        /// <param name="uri">The URI the feed originally came from.</param>
        /// <param name="data">The data of the feed.</param>
        /// <param name="policy">Provides additional class dependencies.</param>
        /// <param name="signature">The first trusted signature for the feed.</param>
        /// <exception cref="InvalidInterfaceIDException">Thrown if feed substitution or another interface URI-related problem was detected.</exception>
        /// <exception cref="ReplayAttackException">Thrown if a replay attack was detected.</exception>
        private void DetectAttacks(Uri uri, byte[] data, Policy policy, ValidSignature signature)
        {
            // Detect feed substitution 
            var feed = XmlStorage.LoadXml<Feed>(new MemoryStream(data));
            if (feed.Uri == null) throw new InvalidInterfaceIDException(string.Format(Resources.FeedUriMissing, uri));
            if (feed.Uri != uri) throw new InvalidInterfaceIDException(string.Format(Resources.FeedUriMismatch, feed.Uri, uri));

            // Detect replay attacks
            try
            {
                var oldSignature = Cache.GetSignatures(uri.ToString(), policy.OpenPgp).OfType<ValidSignature>().FirstOrDefault();
                if (oldSignature != null && signature.Timestamp < oldSignature.Timestamp) throw new ReplayAttackException(uri, oldSignature.Timestamp, signature.Timestamp);
            }
            catch (KeyNotFoundException)
            {
                // No existing feed to be replaced
            }
        }
        #endregion

        #region Check attempt
        /// <summary>
        /// Minimum amount of time between stale feed update attempts.
        /// </summary>
        private static readonly TimeSpan _failedCheckDelay = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Determines the most recent point in time an attempt was made to download a particular feed.
        /// </summary>
        private static DateTime GetLastCheckAttempt(string feedID)
        {
            // Determine timestamp file path
            var file = new FileInfo(Path.Combine(
                Locations.GetCacheDirPath("0install.net", false, "injector", "last-check-attempt"),
                ModelUtils.Escape(feedID).Replace("%2f", "#")));

            // Check last modification time
            return file.Exists ? file.LastWriteTimeUtc : new DateTime();
        }

        /// <summary>
        /// Notes the current time as an attempt to download a particular feed.
        /// </summary>
        private static void SetLastCheckAttempt(string feedID)
        {
            // Determine timestamp file path
            string path = Path.Combine(
                Locations.GetCacheDirPath("0install.net", false, "injector", "last-check-attempt"),
                ModelUtils.Escape(feedID).Replace("%2f", "#"));

            // Set modification time to now
            File.WriteAllText(path, "");
        }
        #endregion

        //--------------------//

        #region Equality
        /// <inheritdoc/>
        public bool Equals(FeedManager other)
        {
            return base.Equals(other);
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
            return base.GetHashCode();
        }
        #endregion
    }
}
