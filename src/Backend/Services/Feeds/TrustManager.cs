/*
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
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
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
        private readonly ITaskHandler _handler;

        /// <summary>
        /// Creates a new trust manager.
        /// </summary>
        /// <param name="config">User settings controlling network behaviour, solving, etc.</param>
        /// <param name="openPgp">The OpenPGP-compatible system used to validate the signatures.</param>
        /// <param name="feedCache">Provides access to a cache of <see cref="Feed"/>s that were downloaded via HTTP(S).</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions.</param>
        public TrustManager([NotNull] Config config, [NotNull] IOpenPgp openPgp, [NotNull] IFeedCache feedCache, [NotNull] ITaskHandler handler)
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
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public ValidSignature CheckTrust(byte[] data, FeedUri uri, FeedUri mirrorUrl = null)
        {
            #region Sanity checks
            if (uri == null) throw new ArgumentNullException("uri");
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            if (uri.IsFile) throw new UriFormatException(Resources.FeedUriLocal);

            var domain = new Domain(uri.Host);
            KeyImported:
            var trustDB = TrustDB.LoadSafe();
            var signatures = FeedUtils.GetSignatures(_openPgp, data);

            foreach (var signature in signatures.OfType<ValidSignature>())
                if (trustDB.IsTrusted(signature.Fingerprint, domain)) return signature;

            foreach (var signature in signatures.OfType<ValidSignature>())
                if (TrustNew(trustDB, uri, signature, domain)) return signature;

            foreach (var signature in signatures.OfType<MissingKeySignature>())
            {
                DownloadMissingKey(uri, mirrorUrl, signature);
                goto KeyImported;
            }

            throw new SignatureException(string.Format(Resources.FeedNoTrustedSignatures, uri));
        }

        private bool TrustNew(TrustDB trustDB, FeedUri uri, ValidSignature signature, Domain domain)
        {
            if (AskKeyApproval(uri, signature, domain))
            {
                trustDB.TrustKey(signature.Fingerprint, domain);
                trustDB.Save();
                return true;
            }
            else return false;
        }

        private bool AskKeyApproval(FeedUri uri, ValidSignature signature, Domain domain)
        {
            bool goodVote;
            var keyInformation = GetKeyInformation(signature.Fingerprint, out goodVote) ?? Resources.NoKeyInfoServerData;

            // Automatically trust key for _new_ feeds if  voted good by key server
            if (_config.AutoApproveKeys && goodVote && !_feedCache.Contains(uri)) return true;

            // Otherwise ask user
            return _handler.AskQuestion(
                string.Format(Resources.AskKeyTrust, uri, signature.Fingerprint, keyInformation, domain),
                batchInformation: Resources.UntrustedKeys);
        }

        private void DownloadMissingKey(FeedUri uri, FeedUri mirrorUrl, MissingKeySignature signature)
        {
            var keyUri = new Uri(mirrorUrl ?? uri, signature.KeyID + ".gpg");
            byte[] keyData;
            if (keyUri.IsFile)
            { // Load key file from local file
                keyData = File.ReadAllBytes(keyUri.LocalPath);
            }
            else
            { // Load key file from server
                try
                {
                    using (var keyFile = new TemporaryFile("0install-key"))
                    {
                        _handler.RunTask(new DownloadFile(keyUri, keyFile));
                        keyData = File.ReadAllBytes(keyFile);
                    }
                }
                    #region Error handling
                catch (WebException ex)
                {
                    // Wrap exception to add context information
                    throw new SignatureException(string.Format(Resources.UnableToLoadKeyFile, uri), ex);
                }
                #endregion
            }
            _openPgp.ImportKey(keyData);
        }

        /// <summary>
        /// Retrieves information about a OpenPGP key from the <see cref="Config.KeyInfoServer"/>.
        /// </summary>
        /// <param name="fingerprint">The fingerprint of the key to check.</param>
        /// <param name="goodVote">Returns <see langword="true"/> if the server indicated that the key is trustworthy.</param>
        /// <returns>Human-readable information about the key or <see langword="null"/> if the server failed to provide a response.</returns>
        [CanBeNull]
        private string GetKeyInformation([NotNull] string fingerprint, out bool goodVote)
        {
            if (_config.KeyInfoServer == null)
            {
                goodVote = false;
                return null;
            }

            try
            {
                var keyInfoUri = new Uri(_config.KeyInfoServer, "key/" + fingerprint);
                var xmlReader = XmlReader.Create(keyInfoUri.AbsoluteUri);
                _handler.CancellationToken.ThrowIfCancellationRequested();
                if (!xmlReader.ReadToFollowing("item"))
                {
                    goodVote = false;
                    return null;
                }

                goodVote = xmlReader.MoveToAttribute("vote") && (xmlReader.Value == "good");
                xmlReader.MoveToContent();
                return xmlReader.ReadElementContentAsString();
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
