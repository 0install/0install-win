﻿/*
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
using System.Linq;
using System.Net;
using Common;
using Common.Controls;
using Common.Storage;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Services.Feeds
{
    /// <summary>
    /// Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.
    /// </summary>
    public class FeedManager : IFeedManager
    {
        #region Dependencies
        private readonly Config _config;
        private readonly IFeedCache _feedCache;
        private readonly ITrustManager _trustManager;
        private readonly IInteractionHandler _handler;

        /// <summary>
        /// Creates a new feed manager.
        /// </summary>
        /// <param name="config">User settings controlling network behaviour, solving, etc.</param>
        /// <param name="feedCache">The disk-based cache to store downloaded <see cref="Feed"/>s.</param>
        /// <param name="trustManager">Methods for verifying signatures and user trust.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        public FeedManager(Config config, IFeedCache feedCache, ITrustManager trustManager, IInteractionHandler handler)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            if (trustManager == null) throw new ArgumentNullException("trustManager");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            _config = config;
            _feedCache = feedCache;
            _trustManager = trustManager;
            _handler = handler;
        }

        /// <inheritdoc/>
        public void Flush()
        {
            _feedCache.Flush();
        }
        #endregion

        #region Properties
        /// <inheritdoc/>
        public bool Refresh { get; set; }

        /// <inheritdoc/>
        public bool Stale { get; set; }
        #endregion

        //--------------------//

        #region Get feed
        /// <inheritdoc/>
        public Feed GetFeed(string feedID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            #endregion

            // Assume invalid URIs are local paths
            Uri feedUrl;
            if (!ModelUtils.TryParseUri(feedID, out feedUrl))
                return LoadLocal(feedID);

            try
            {
                if (Refresh) Download(feedUrl);
                else if (!_feedCache.Contains(feedID))
                {
                    // Do not download in offline mode
                    if (_config.NetworkUse == NetworkLevel.Offline)
                        throw new IOException(string.Format(Resources.FeedNotCachedOffline, feedID));

                    // Try to download missing feed
                    Download(feedUrl);
                }

                return LoadCached(feedID);
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
        private Feed LoadLocal(string feedID)
        {
            if (File.Exists(feedID))
            {
                // Use cache even for local files since there may be in-memory caching
                try
                {
                    return _feedCache.GetFeed(feedID);
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
        /// Loads a <see cref="Feed"/> from the <see cref="_feedCache"/>.
        /// </summary>
        /// <param name="feedID">The ID used to identify the feed. Must be an HTTP(S) URL.</param>
        /// <returns>The parsed <see cref="Feed"/> object.</returns>
        /// <exception cref="InvalidInterfaceIDException">Thrown if <paramref name="feedID"/> is an invalid interface ID.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the requested <paramref name="feedID"/> was not found in the cache.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while reading the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the cache is not permitted.</exception>
        private Feed LoadCached(string feedID)
        {
            try
            {
                var feed = _feedCache.GetFeed(feedID);
                Stale |= IsStale(feedID);
                return feed;
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

        private bool IsStale(string feedID)
        {
            if (_config.NetworkUse != NetworkLevel.Full) return false;

            var preferences = FeedPreferences.LoadForSafe(feedID);
            TimeSpan lastChecked = DateTime.UtcNow - preferences.LastChecked;
            TimeSpan lastCheckAttempt = DateTime.UtcNow - GetLastCheckAttempt(feedID);
            return (lastChecked > _config.Freshness && lastCheckAttempt > _failedCheckDelay);
        }
        #endregion

        #region Download
        /// <summary>
        /// Downloads a <see cref="Feed"/> into the <see cref="_feedCache"/> validating its signatures.
        /// </summary>
        /// <param name="url">The URL of download the feed from.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the process.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown if <paramref name="url"/> is an invalid interface ID.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the requested <paramref name="url"/> was not found in the cache.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while reading the feed file.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while fetching the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the cache is not permitted.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2200:RethrowToPreserveStackDetails", Justification = "Rethrow the outer instead of the inner exception")]
        private void Download(Uri url)
        {
            SetLastCheckAttempt(url.ToString());

            // TODO: Add tracking and better cancellation support
            _handler.CancellationToken.ThrowIfCancellationRequested();
            using (var webClient = new WebClientTimeout())
            {
                try
                {
                    ImportFeed(url, webClient.DownloadData(url));
                    _handler.CancellationToken.ThrowIfCancellationRequested();
                }
                catch (WebException ex)
                {
                    Log.Warn(string.Format(Resources.FeedDownloadError, url));

                    if (url.IsLoopback) throw;
                    Log.Info(Resources.TryingFeedMirror);
                    try
                    {
                        DownloadMirror(url);
                    }
                    catch (WebException)
                    {
                        // Report the original problem instead of mirror errors
                        throw ex;
                    }
                }
            }
            _handler.CancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Downloads a <see cref="Feed"/> from the <see cref="Config.FeedMirror"/>.
        /// </summary>
        /// <param name="url">The URL of download the feed from.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the process.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown if <paramref name="url"/> is an invalid interface ID.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the requested <paramref name="url"/> was not found in the cache.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while reading the feed file.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while fetching the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the cache is not permitted.</exception>
        private void DownloadMirror(Uri url)
        {
            _handler.CancellationToken.ThrowIfCancellationRequested();

            var mirrorUrl = new Uri(_config.FeedMirror, string.Format(
                "feeds/{0}/{1}/{2}/latest.xml",
                url.Scheme,
                url.Host,
                string.Concat(url.Segments).TrimStart('/').Replace("/", "%23")));

            using (var webClient = new WebClientTimeout())
                ImportFeed(url, webClient.DownloadData(mirrorUrl), mirrorUrl);

            _handler.CancellationToken.ThrowIfCancellationRequested();
        }
        #endregion

        #region Import feed
        /// <inheritdoc/>
        public void ImportFeed(Uri uri, byte[] data, Uri mirrorUri = null)
        {
            #region Sanity checks
            if (uri == null) throw new ArgumentNullException("uri");
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            var newSignature = _trustManager.CheckTrust(uri, data, mirrorUri);
            DetectAttacks(uri, data, newSignature);

            // Add to cache and remember time
            _feedCache.Add(uri.ToString(), data);
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
        /// <param name="signature">The first trusted signature for the feed.</param>
        /// <exception cref="InvalidInterfaceIDException">Thrown if feed substitution or another interface URI-related problem was detected.</exception>
        /// <exception cref="ReplayAttackException">Thrown if a replay attack was detected.</exception>
        private void DetectAttacks(Uri uri, byte[] data, ValidSignature signature)
        {
            // Detect feed substitution 
            var feed = XmlStorage.LoadXml<Feed>(new MemoryStream(data));
            if (feed.Uri == null) throw new InvalidInterfaceIDException(string.Format(Resources.FeedUriMissing, uri));
            if (feed.Uri != uri) throw new InvalidInterfaceIDException(string.Format(Resources.FeedUriMismatch, feed.Uri, uri));

            // Detect replay attacks
            try
            {
                var oldSignature = _feedCache.GetSignatures(uri.ToString()).OfType<ValidSignature>().FirstOrDefault();
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
                ModelUtils.PrettyEscape(feedID)));

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
                ModelUtils.PrettyEscape(feedID));

            // Set modification time to now
            File.WriteAllText(path, "");
        }
        #endregion
    }
}
