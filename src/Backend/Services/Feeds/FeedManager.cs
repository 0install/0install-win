/*
 * Copyright 2010-2015 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
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
        private readonly ITaskHandler _handler;

        /// <summary>
        /// Creates a new feed manager.
        /// </summary>
        /// <param name="config">User settings controlling network behaviour, solving, etc.</param>
        /// <param name="feedCache">The disk-based cache to store downloaded <see cref="Feed"/>s.</param>
        /// <param name="trustManager">Methods for verifying signatures and user trust.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        public FeedManager([NotNull] Config config, [NotNull] IFeedCache feedCache, [NotNull] ITrustManager trustManager, [NotNull] ITaskHandler handler)
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
        #endregion

        #region Properties
        /// <inheritdoc/>
        public bool Refresh { get; set; }

        /// <inheritdoc/>
        public bool Stale { get; set; }

        /// <inheritdoc/>
        public bool ShouldRefresh { get { return Stale && _config.NetworkUse == NetworkLevel.Full; } }
        #endregion

        //--------------------//

        #region Get feed
        /// <inheritdoc/>
        public Feed GetFeed(FeedUri feedUri)
        {
            #region Sanity checks
            if (feedUri == null) throw new ArgumentNullException("feedUri");
            #endregion

            if (feedUri.IsFile) return LoadLocal(feedUri);
            else
            {
                try
                {
                    if (Refresh) Download(feedUri);
                    else if (!_feedCache.Contains(feedUri))
                    {
                        // Do not download in offline mode
                        if (_config.NetworkUse == NetworkLevel.Offline)
                            throw new IOException(string.Format(Resources.FeedNotCachedOffline, feedUri));

                        // Try to download missing feed
                        Download(feedUri);
                    }

                    return LoadCached(feedUri);
                }
                    #region Error handling
                catch (KeyNotFoundException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    throw new IOException(ex.Message, ex);
                }
                #endregion
            }
        }
        #endregion

        #region Local
        /// <summary>
        /// Loads a <see cref="Feed"/> from a local file.
        /// </summary>
        /// <param name="feedUri">The ID used to identify the feed. Must be an absolute local path.</param>
        /// <returns>The parsed <see cref="Feed"/> object.</returns>
        /// <exception cref="IOException">A problem occured while reading the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the cache is not permitted.</exception>
        [NotNull]
        private Feed LoadLocal(FeedUri feedUri)
        {
            if (File.Exists(feedUri.LocalPath))
            {
                // Use cache even for local files since there may be in-memory caching
                try
                {
                    return _feedCache.GetFeed(feedUri);
                }
                    #region Error handling
                catch (InvalidDataException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    throw new IOException(ex.Message, ex);
                }
                #endregion
            }
            throw new FileNotFoundException(string.Format(Resources.FileNotFound, feedUri.LocalPath), feedUri.LocalPath);
        }
        #endregion

        #region Cached
        /// <summary>
        /// Loads a <see cref="Feed"/> from the <see cref="_feedCache"/>.
        /// </summary>
        /// <param name="feedUri">The ID used to identify the feed. Must be an HTTP(S) URL.</param>
        /// <returns>The parsed <see cref="Feed"/> object.</returns>
        /// <exception cref="KeyNotFoundException">The requested <paramref name="feedUri"/> was not found in the cache.</exception>
        /// <exception cref="IOException">A problem occured while reading the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the cache is not permitted.</exception>
        [NotNull]
        private Feed LoadCached(FeedUri feedUri)
        {
            try
            {
                var feed = _feedCache.GetFeed(feedUri);
                Stale |= IsStale(feedUri);
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

        /// <inheritdoc/>
        public bool IsStale(FeedUri feedUri)
        {
            #region Sanity checks
            if (feedUri == null) throw new ArgumentNullException("feedUri");
            #endregion

            var preferences = FeedPreferences.LoadForSafe(feedUri);
            TimeSpan lastChecked = DateTime.UtcNow - preferences.LastChecked;
            TimeSpan lastCheckAttempt = DateTime.UtcNow - GetLastCheckAttempt(feedUri);
            return (lastChecked > _config.Freshness && lastCheckAttempt > _failedCheckDelay);
        }

        /// <summary>
        /// Minimum amount of time between stale feed update attempts.
        /// </summary>
        private static readonly TimeSpan _failedCheckDelay = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Determines the most recent point in time an attempt was made to download a particular feed.
        /// </summary>
        private static DateTime GetLastCheckAttempt(FeedUri feedUri)
        {
            // Determine timestamp file path
            var file = new FileInfo(Path.Combine(
                Locations.GetCacheDirPath("0install.net", false, "injector", "last-check-attempt"),
                feedUri.PrettyEscape()));

            // Check last modification time
            return file.Exists ? file.LastWriteTimeUtc : new DateTime();
        }
        #endregion

        #region Download
        /// <summary>
        /// Downloads a <see cref="Feed"/> into the <see cref="_feedCache"/> validating its signatures. Automatically falls back to the mirror server.
        /// </summary>
        /// <param name="feedUri">The URL of the feed to download.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="WebException">A problem occured while fetching the feed file.</exception>
        /// <exception cref="IOException">A problem occured while writing the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the cache is not permitted.</exception>
        /// <exception cref="SignatureException">The signature data of the feed file could not be handled or no signatures were trusted.</exception>
        /// <exception cref="UriFormatException"><see cref="Feed.Uri"/> is missing or does not match <paramref name="feedUri"/> or <paramref name="feedUri"/> is a local file.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2200:RethrowToPreserveStackDetails", Justification = "Rethrow the outer instead of the inner exception")]
        private void Download(FeedUri feedUri)
        {
            SetLastCheckAttempt(feedUri);

            try
            {
                DownloadFeed(feedUri);
            }
            catch (WebException ex)
            {
                if (feedUri.IsLoopback) throw;
                Log.Warn(string.Format(Resources.FeedDownloadError, feedUri) + " " + Resources.TryingFeedMirror);
                try
                {
                    DownloadFeed(feedUri, downloadUrl: GetMirrorUrl(feedUri));
                }
                catch (WebException)
                {
                    // Report the original problem instead of mirror errors
                    ex.Rethrow();
                }
            }
        }

        [NotNull]
        private Uri GetMirrorUrl([NotNull] FeedUri feedUri)
        {
            return new Uri(_config.FeedMirror.EnsureTrailingSlash().AbsoluteUri + "feeds/" + feedUri.Scheme + "/" + feedUri.Host + "/" + string.Concat(feedUri.Segments).TrimStart('/').Replace("/", "%23") + "/latest.xml");
        }

        /// <summary>
        /// Notes the current time as an attempt to download a particular feed.
        /// </summary>
        private static void SetLastCheckAttempt(FeedUri feedUri)
        {
            // Determine timestamp file path
            string path = Path.Combine(
                Locations.GetCacheDirPath("0install.net", false, "injector", "last-check-attempt"),
                feedUri.PrettyEscape());

            // Set modification time to now
            FileUtils.Touch(path);
        }

        /// <summary>
        /// Downloads a <see cref="Feed"/> into the <see cref="_feedCache"/> validating its signatures.
        /// </summary>
        /// <param name="feedUri">The original URL the feed came from.</param>
        /// <param name="downloadUrl">The URL to download the fed from. Leave <see langword="null"/> to use <paramref name="feedUri"/>.</param>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="WebException">A problem occured while fetching the feed file.</exception>
        /// <exception cref="IOException">A problem occured while writing the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the cache is not permitted.</exception>
        /// <exception cref="SignatureException">The signature data of the feed file could not be handled or no signatures were trusted.</exception>
        /// <exception cref="UriFormatException"><see cref="Feed.Uri"/> is missing or does not match <paramref name="feedUri"/> or <paramref name="feedUri"/> is a local file.</exception>
        private void DownloadFeed([NotNull] FeedUri feedUri, [CanBeNull] Uri downloadUrl = null)
        {
            if (downloadUrl == null) downloadUrl = feedUri;

            using (var feedFile = new TemporaryFile("0install-feed"))
            {
                _handler.RunTask(new DownloadFile(downloadUrl, feedFile));
                ImportFeed(File.ReadAllBytes(feedFile), feedUri);
            }
        }
        #endregion

        #region Import feed
        /// <inheritdoc/>
        public void ImportFeed(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            var feed = XmlStorage.LoadXml<Feed>(path);
            if (feed.Uri == null) throw new InvalidDataException(Resources.ImportNoSource);
            ImportFeed(File.ReadAllBytes(path), feed.Uri, path);
        }

        /// <summary>
        /// Imports a <see cref="Feed"/> into the <see cref="IFeedCache"/> after verifying its signature.
        /// </summary>
        /// <param name="data">The content of the feed.</param>
        /// <param name="feedUri">The URI the feed originally came from.</param>
        /// <param name="localPath">The local file path the feed data came from. May be <see langword="null"/> for in-memory data.</param>
        /// <exception cref="IOException">A problem occured while reading the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the feed file or the cache is not permitted.</exception>
        /// <exception cref="SignatureException">The signature data of the feed file could not be handled or no signatures were trusted.</exception>
        /// <exception cref="UriFormatException"><see cref="Feed.Uri"/> is missing or does not match <paramref name="feedUri"/> or <paramref name="feedUri"/> is a local file.</exception>
        private void ImportFeed([NotNull] byte[] data, [NotNull] FeedUri feedUri, [CanBeNull] string localPath = null)
        {
            Log.Debug("Importing feed " + feedUri.ToStringRfc() + " from " + (localPath ?? "web"));

            CheckFeed(data, feedUri);
            CheckTrust(data, feedUri, localPath);
            AddToCache(data, feedUri);
        }

        private void AddToCache(byte[] data, FeedUri feedUri)
        {
            _feedCache.Add(feedUri, data);

            var preferences = FeedPreferences.LoadForSafe(feedUri);
            preferences.LastChecked = DateTime.UtcNow;
            preferences.Normalize();
            preferences.SaveFor(feedUri);
        }

        private static void CheckFeed(byte[] data, FeedUri feedUri)
        {
            // Detect feed substitution
            var feed = XmlStorage.LoadXml<Feed>(new MemoryStream(data));
            if (feed.Uri == null) throw new InvalidDataException(string.Format(Resources.FeedUriMissing, feedUri));
            if (feed.Uri != feedUri) throw new InvalidDataException(string.Format(Resources.FeedUriMismatch, feed.Uri, feedUri));
        }

        private void CheckTrust(byte[] data, FeedUri feedUri, string localPath)
        {
            // Detect replay attacks
            var newSignature = _trustManager.CheckTrust(data, feedUri, localPath);
            try
            {
                var oldSignature = _feedCache.GetSignatures(feedUri).OfType<ValidSignature>().FirstOrDefault();
                if (oldSignature != null && newSignature.Timestamp < oldSignature.Timestamp) throw new ReplayAttackException(feedUri, oldSignature.Timestamp, newSignature.Timestamp);
            }
            catch (KeyNotFoundException)
            {
                // No existing feed to be replaced
            }
        }
        #endregion
    }
}
