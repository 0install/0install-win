/*
 * Copyright 2010-2012 Bastian Eicher
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
using Common.Cli;
using Common.Collections;
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
        public override Feed GetFeed(string feedID, Policy policy, out bool stale)
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
            var preferences = FeedPreferences.LoadForSafe(feedID);
            TimeSpan feedAge = DateTime.UtcNow - preferences.LastChecked;
            stale = (feedAge > policy.Config.Freshness);

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
        private readonly WebClient _webClient = new WebClient();

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
        private void DownloadFeed(Uri url, Policy policy)
        {
            // ToDo: Add mirror support
            ImportFeed(url, _webClient.DownloadData(url), policy);
        }
        #endregion

        #region Import feed
        /// <inheritdoc/>
        public override void ImportFeed(Uri uri, byte[] data, Policy policy)
        {
            #region Sanity checks
            if (uri == null) throw new ArgumentNullException("uri");
            if (data == null) throw new ArgumentNullException("data");
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            var newSignature = CheckFeedTrust(uri, data, policy);
            DetectAttacks(uri, data, policy, newSignature);

            // Add to cache and remember time
            Cache.Add(uri.ToString(), data);
            var preferences = FeedPreferences.LoadForSafe(uri.ToString());
            preferences.LastChecked = DateTime.UtcNow;
            preferences.SaveFor(uri.ToString());
        }

        /// <summary>
        /// Checks whether a remote <see cref="Feed"/> has a a valid and trusted signature. Downloads missing GPG keys for verification and interactivley asks the user to trust new keys.
        /// </summary>
        /// <param name="uri">The URI the feed originally came from.</param>
        /// <param name="data">The data of the feed.</param>
        /// <param name="policy">Provides additional class dependencies.</param>
        /// <exception cref="SignatureException">Thrown if no trusted signature was found.</exception>
        private ValidSignature CheckFeedTrust(Uri uri, byte[] data, Policy policy)
        {
            var trustDB = TrustDB.LoadSafe();
            var domain = new Domain(uri.Host);
            KeyImported:
            var signatures = FeedUtils.GetSignatures(policy.OpenPgp, data);

            // Try to find already trusted key
            var validSignatures = EnumerableUtils.OfType<ValidSignature>(signatures);
            var trustedSignature = EnumerableUtils.First(validSignatures, sig => trustDB.IsTrusted(sig.Fingerprint, domain));
            if (trustedSignature != null) return trustedSignature;

            // Try to find valid key and ask user to trust it
            trustedSignature = EnumerableUtils.First(validSignatures, sig => CheckSignatureTrust(sig, policy));
            if (trustedSignature != null)
            {
                trustDB.TrustKey(trustedSignature.Fingerprint, domain);
                trustDB.Save();
                return trustedSignature;
            }

            // Download missing key file
            var missingKey = EnumerableUtils.First(EnumerableUtils.OfType<MissingKeySignature>(signatures));
            if (missingKey != null)
            {
                try
                {
                    policy.OpenPgp.ImportKey(_webClient.DownloadData(new Uri(uri, missingKey.KeyID + ".gpg")));
                    goto KeyImported;
                }
                    #region Error handling
                catch (WebException ex)
                {
                    Log.Error("Failed to download key file for " + missingKey.KeyID + "\n" + ex.Message);
                }
                catch (UnhandledErrorsException ex)
                {
                    Log.Error("Failed to import key file for " + missingKey.KeyID + "\n" + ex.Message);
                }
                #endregion
            }

            throw new SignatureException(string.Format(Resources.FeedNoTrustedSignatures, uri));
        }

        /// <summary>
        /// Checks whether a specific signature is deemed as trusted. May automatically trust new signatures or ask the user, based on the current <see cref="Config"/>.
        /// </summary>
        /// <param name="signature">The signature to check.</param>
        /// <param name="policy">Provides additional class dependencies.</param>
        private bool CheckSignatureTrust(ValidSignature signature, Policy policy)
        {
            // ToDo: Communicate with key server
            return policy.Handler.AskQuestion("Trust?", Resources.UntrustedKeys);
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
            var feed = Feed.Load(new MemoryStream(data));
            if (feed.Uri == null) throw new InvalidInterfaceIDException(string.Format(Resources.FeedUriMissing, uri));
            if (feed.Uri != uri) throw new InvalidInterfaceIDException(string.Format(Resources.FeedUriMismatch, feed.Uri, uri));

            // Detect replay attacks
            try
            {
                var oldSignature = EnumerableUtils.First(EnumerableUtils.OfType<ValidSignature>(Cache.GetSignatures(uri.ToString(), policy.OpenPgp)));
                if (oldSignature != null && signature.Timestamp < oldSignature.Timestamp) throw new ReplayAttackException(uri, oldSignature.Timestamp, signature.Timestamp);
            }
            catch (KeyNotFoundException)
            {
                // No existing feed to be replaced
            }
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
