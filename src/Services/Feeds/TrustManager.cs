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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using Common;
using Common.Controls;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Services.Feeds
{
    /// <summary>
    /// Methods for verifying signatures and user trust.
    /// </summary>
    public class TrustManager : ITrustManager
    {
        #region Dependencies
        private readonly Config _config;
        private readonly IOpenPgp _openPgp;
        private readonly IFeedCache _feedCache;
        private readonly IInteractionHandler _handler;

        /// <summary>
        /// Creates a new trust manager.
        /// </summary>
        /// <param name="config">User settings controlling network behaviour, solving, etc.</param>
        /// <param name="openPgp">The OpenPGP-compatible system used to validate the signatures.</param>
        /// <param name="feedCache">Provides access to a cache of <see cref="Feed"/>s that were downloaded via HTTP(S).</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions.</param>
        public TrustManager(Config config, IOpenPgp openPgp, IFeedCache feedCache, IInteractionHandler handler)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            _config = config;
            _openPgp = openPgp;
            _feedCache = feedCache;
            _handler = handler;
        }
        #endregion

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
        public ValidSignature CheckTrust(Uri uri, byte[] data, Uri mirrorUri = null)
        {
            #region Sanity checks
            if (uri == null) throw new ArgumentNullException("uri");
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            var domain = new Domain(uri.Host);
            KeyImported:
            var trustDB = TrustDB.LoadSafe();
            var signatures = FeedUtils.GetSignatures(_openPgp, data);

            // Try to find already trusted key
            // ReSharper disable PossibleMultipleEnumeration
            // ReSharper disable AccessToModifiedClosure
            var validSignatures = signatures.OfType<ValidSignature>().ToList();
            var trustedSignature = validSignatures.FirstOrDefault(sig => trustDB.IsTrusted(sig.Fingerprint, domain));
            if (trustedSignature != null) return trustedSignature;
            // ReSharper restore PossibleMultipleEnumeration
            // ReSharper restore AccessToModifiedClosure

            // Try to find valid key and ask user to approve it
            trustedSignature = validSignatures.FirstOrDefault(signature => IsKeyApproved(uri, signature, domain));
            if (trustedSignature != null)
            {
                // Add newly approved key to trust database
                trustDB.TrustKey(trustedSignature.Fingerprint, domain);
                trustDB.Save();

                return trustedSignature;
            }

            // Download missing key file
            // ReSharper disable PossibleMultipleEnumeration
            var missingKey = signatures.OfType<MissingKeySignature>().FirstOrDefault();
            // ReSharper restore PossibleMultipleEnumeration
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
                    try
                    {
                        // TODO: Add tracking and better cancellation support
                        _handler.CancellationToken.ThrowIfCancellationRequested();
                        using (var webClient = new WebClientTimeout())
                            keyData = webClient.DownloadData(keyUri);
                    }
                        #region Error handling
                    catch (WebException ex)
                    {
                        // Wrap exception to add context information
                        throw new SignatureException(string.Format(Resources.UnableToLoadKeyFile, uri) + "\n" + ex.Message, ex);
                    }
                    #endregion
                }
                _handler.CancellationToken.ThrowIfCancellationRequested();
                _openPgp.ImportKey(keyData);
                goto KeyImported; // Re-evaluate signatures after importing new key material
            }

            throw new SignatureException(string.Format(Resources.FeedNoTrustedSignatures, uri));
        }

        private bool IsKeyApproved(Uri uri, ValidSignature signature, Domain domain)
        {
            bool goodVote;
            var keyInformation = GetKeyInformation(signature.Fingerprint, out goodVote) ?? Resources.NoKeyInfoServerData;

            // Automatically trust key for _new_ feeds if  voted good by key server
            if (_config.AutoApproveKeys && goodVote && !_feedCache.Contains(uri.ToString())) return true;

            // Otherwise ask user
            return _handler.AskQuestion(string.Format(Resources.AskKeyTrust, uri, signature.Fingerprint, keyInformation, domain), Resources.UntrustedKeys);
        }

        /// <summary>
        /// Retrieves information about a OpenPGP key from the <see cref="Config.KeyInfoServer"/>.
        /// </summary>
        /// <param name="fingerprint">The fingerprint of the key to check.</param>
        /// <param name="goodVote">Returns <see langword="true"/> if the server indicated that the key is trustworthy.</param>
        /// <returns>Human-readable information about the key or <see langword="null"/> if the server failed to provide a response.</returns>
        private string GetKeyInformation(string fingerprint, out bool goodVote)
        {
            if (_config.KeyInfoServer == null)
            {
                goodVote = false;
                return null;
            }

            try
            {
                // TODO: Add better cancellation support
                var keyInfoUri = new Uri(_config.KeyInfoServer, "key/" + fingerprint);
                var xmlReader = XmlReader.Create(keyInfoUri.ToString());
                _handler.CancellationToken.ThrowIfCancellationRequested();
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
                Log.Error(string.Format(Resources.UnableToParseKeyInfo, fingerprint));
                Log.Error(ex);
                goodVote = false;
                return null;
            }
            catch (WebException ex)
            {
                Log.Error(string.Format(Resources.UnableToRetrieveKeyInfo, fingerprint));
                Log.Error(ex);
                goodVote = false;
                return null;
            }
            #endregion
        }
    }
}
