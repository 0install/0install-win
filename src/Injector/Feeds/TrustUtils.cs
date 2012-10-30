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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using Common;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector.Feeds
{
    /// <summary>
    /// Utility methods for verifying signatures and user trust.
    /// </summary>
    public static class TrustUtils
    {
        /// <summary>
        /// Checks whether a remote feed or catalog file has a a valid and trusted signature. Downloads missing GPG keys for verification and interactivley asks the user to approve new keys.
        /// </summary>
        /// <param name="uri">The URI the feed or catalog file originally came from.</param>
        /// <param name="mirrorUri">The URI or local file path the file was actually loaded from; <see langword="null"/> if it is identical to <paramref name="uri"/>.</param>
        /// <param name="data">The data of the file.</param>
        /// <param name="policy">Provides additional class dependencies.</param>
        /// <exception cref="SignatureException">Thrown if no trusted signature was found.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
        public static ValidSignature CheckTrust(Uri uri, Uri mirrorUri, byte[] data, Policy policy)
        {
            #region Sanity checks
            if (uri == null) throw new ArgumentNullException("uri");
            if (data == null) throw new ArgumentNullException("data");
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            var domain = new Domain(uri.Host);
            KeyImported:
            var trustDB = TrustDB.LoadSafe();
            var signatures = FeedUtils.GetSignatures(policy.OpenPgp, data);

            // Try to find already trusted key
            // ReSharper disable PossibleMultipleEnumeration
            // ReSharper disable AccessToModifiedClosure
            var validSignatures = signatures.OfType<ValidSignature>().ToList();
            var trustedSignature = validSignatures.FirstOrDefault(sig => trustDB.IsTrusted(sig.Fingerprint, domain));
            if (trustedSignature != null) return trustedSignature;
            // ReSharper restore PossibleMultipleEnumeration
            // ReSharper restore AccessToModifiedClosure

            // Try to find valid key and ask user to approve it
            trustedSignature = validSignatures.FirstOrDefault(sig =>
            {
                bool goodVote;
                var keyInformation = GetKeyInformation(sig.Fingerprint, out goodVote, policy) ?? Resources.NoKeyInfoServerData;

                // Automatically trust key if known and voted good by key server (unless the feed was already seen/cached)
                return (policy.Config.AutoApproveKeys && goodVote && !policy.FeedManager.Cache.Contains(uri.ToString())) ||
                    // Otherwise ask user
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
                        // ToDo: Add tracking and better cancellation support
                        policy.Handler.CancellationToken.ThrowIfCancellationRequested();
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
                policy.Handler.CancellationToken.ThrowIfCancellationRequested();
                policy.OpenPgp.ImportKey(keyData);
                goto KeyImported; // Re-evaluate signatures after importing new key material
            }

            throw new SignatureException(string.Format(Resources.FeedNoTrustedSignatures, uri));
        }

        /// <summary>
        /// Retrieves information about a OpenPGP key from the <see cref="Config.KeyInfoServer"/>.
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
    }
}
