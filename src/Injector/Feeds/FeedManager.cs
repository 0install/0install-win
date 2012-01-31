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
using System.Xml;
using Common;
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
            // ToDo: Add tracking and better cancellation support
            policy.Handler.CancellationToken.ThrowIfCancellationRequested();
            using (var webClient = new WebClient())
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
                    Log.Warn("Error while downloading feed '" + url + "':\n" + ex.Message + "\nSwitching to mirror.");

                    var mirrorUrl = new Uri(policy.Config.FeedMirror, string.Format(
                        "feeds/{0}/{1}/{2}/latest.xml",
                        url.Scheme,
                        url.Host,
                        string.Concat(url.Segments).TrimStart('/').Replace("/", "%23")));
                    ImportFeed(url, mirrorUrl, webClient.DownloadData(mirrorUrl), policy);
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

            var newSignature = CheckFeedTrust(uri, mirrorUri, data, policy);
            DetectAttacks(uri, data, policy, newSignature);

            // Add to cache and remember time
            Cache.Add(uri.ToString(), data);
            var preferences = FeedPreferences.LoadForSafe(uri.ToString());
            preferences.LastChecked = DateTime.UtcNow;
            preferences.SaveFor(uri.ToString());
        }

        /// <summary>
        /// Checks whether a remote <see cref="Feed"/> has a a valid and trusted signature. Downloads missing GPG keys for verification and interactivley asks the user to approve new keys.
        /// </summary>
        /// <param name="uri">The URI the feed originally came from.</param>
        /// <param name="mirrorUri">The URI or local file path the feed was actually loaded from; <see langword="null"/> if it is identical to <paramref name="uri"/>.</param>
        /// <param name="data">The data of the feed.</param>
        /// <param name="policy">Provides additional class dependencies.</param>
        /// <exception cref="SignatureException">Thrown if no trusted signature was found.</exception>
        private static ValidSignature CheckFeedTrust(Uri uri, Uri mirrorUri, byte[] data, Policy policy)
        {
            var domain = new Domain(uri.Host);
            KeyImported:
            var trustDB = TrustDB.LoadSafe();
            var signatures = FeedUtils.GetSignatures(policy.OpenPgp, data);

            // Try to find already trusted key
            var validSignatures = EnumerableUtils.OfType<ValidSignature>(signatures);
            // ReSharper disable AccessToModifiedClosure
            var trustedSignature = EnumerableUtils.First(validSignatures, sig => trustDB.IsTrusted(sig.Fingerprint, domain));
            // ReSharper restore AccessToModifiedClosure
            if (trustedSignature != null) return trustedSignature;

            // Try to find valid key and ask user to approve it
            trustedSignature = EnumerableUtils.First(validSignatures, sig =>
            {
                bool goodVote;
                var keyInformation = GetKeyInformation(sig.Fingerprint, out goodVote, policy) ?? Resources.NoKeyInfoServerData;
                return (policy.Config.AutoApproveKeys && goodVote) ||
                    policy.Handler.AskQuestion(string.Format(Resources.AskKeyTrust, uri, sig.Fingerprint, keyInformation, domain), Resources.UntrustedKeys);
            });
            if (trustedSignature != null)
            {
                // Add newly approved key to trust database
                trustDB.TrustKey(trustedSignature.Fingerprint, domain);
                trustDB.Save();

                return trustedSignature;
            }

            // Download missing key file
            var missingKey = EnumerableUtils.First(EnumerableUtils.OfType<MissingKeySignature>(signatures));
            if (missingKey != null)
            {
                var keyUri = new Uri(mirrorUri ?? uri, missingKey.KeyID + ".gpg");
                byte[] keyData;
                if (keyUri.IsFile)
                { // Load key file from local file
                    keyData = File.ReadAllBytes(keyUri.LocalPath);
                }
                else
                { // Load key file from server
                    using (var webClient = new WebClient())
                        keyData = webClient.DownloadData(keyUri);
                }
                policy.Handler.CancellationToken.ThrowIfCancellationRequested();
                policy.OpenPgp.ImportKey(keyData);
                goto KeyImported; // Re-evaluate signatures after importing new key material
            }

            throw new SignatureException(string.Format(Resources.FeedNoTrustedSignatures, uri));
        }

        /// <summary>
        /// retrieves information about a OpenPGP key from the <see cref="Config.KeyInfoServer"/>.
        /// </summary>
        /// <param name="fingerprint">The fingerprint of the key to check.</param>
        /// <param name="goodVote">Returns <see langword="true"/> if the server indicated that the key is trustworthy.</param>
        /// <param name="policy">Provides additional class dependencies.</param>
        /// <returns>Human-readable information about the key or <see langword="null"/> if the server failed to provide a response.</returns>
        private static string GetKeyInformation(string fingerprint, out bool goodVote, Policy policy)
        {
            try
            {
                // ToDo: Add better cancellation support
                var xmlReader = XmlReader.Create(policy.Config.KeyInfoServer + "key/" + fingerprint);
                policy.Handler.CancellationToken.ThrowIfCancellationRequested();
                if (!xmlReader.ReadToFollowing("item"))
                {
                    goodVote = false;
                    return null;
                }

                goodVote = xmlReader.MoveToAttribute("vote") && (xmlReader.Value == "good");
                xmlReader.MoveToContent();
                return xmlReader.ReadString();
            }
                #region Error handling
            catch (XmlException ex)
            {
                Log.Error("Unable to parse key information for: " + fingerprint + "\n" + ex.Message);
                goodVote = false;
                return null;
            }
            catch (WebException ex)
            {
                Log.Error("Unable to retrieve key information for: " + fingerprint + "\n" + ex.Message);
                goodVote = false;
                return null;
            }
            #endregion
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
