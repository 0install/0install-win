/*
 * Copyright 2010-2015 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Properties;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Provides utiltity methods for managing <see cref="Feed"/>s.
    /// </summary>
    public static class FeedUtils
    {
        #region Cache
        /// <summary>
        /// Loads all <see cref="Feed"/>s stored in <see cref="IFeedCache"/> into memory.
        /// </summary>
        /// <param name="cache">The <see cref="IFeedCache"/> to load <see cref="Feed"/>s from.</param>
        /// <returns>The parsed <see cref="Feed"/>s. Damaged files are logged and skipped.</returns>
        /// <exception cref="IOException">A problem occured while reading from the cache.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the cache is not permitted.</exception>
        public static IEnumerable<Feed> GetAll([NotNull] this IFeedCache cache)
        {
            #region Sanity checks
            if (cache == null) throw new ArgumentNullException("cache");
            #endregion

            var feeds = new List<Feed>();
            foreach (var feedUri in cache.ListAll())
            {
                try
                {
                    feeds.Add(cache.GetFeed(feedUri));
                }
                    #region Error handling
                catch (KeyNotFoundException)
                {
                    // Feed file no longer exists
                }
                catch (InvalidDataException ex)
                {
                    Log.Error(ex);
                }
                #endregion
            }
            return feeds;
        }
        #endregion

        #region Signatures
        /// <summary>
        /// The string signifying the start of a signature block.
        /// </summary>
        public const string SignatureBlockStart = "<!-- Base64 Signature\n";

        /// <summary>
        /// The string signifying the end of a signature block.
        /// </summary>
        public const string SignatureBlockEnd = "\n-->\n";

        /// <summary>
        /// The encoding used when looking for signature blocks in feed files.
        /// </summary>
        public static readonly Encoding Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        /// <summary>
        /// <see cref="SignatureBlockStart"/> encoded with <see cref="Encoding"/>.</summary>
        private static readonly byte[] _signatureBlockStartEncoded = Encoding.GetBytes(SignatureBlockStart);

        /// <summary>
        /// Determines which signatures a feed is signed with.
        /// </summary>
        /// <param name="openPgp">The OpenPGP-compatible system used to validate the signatures.</param>
        /// <param name="feedData">The feed data containing an embedded signature.</param>
        /// <returns>A list of signatures found, both valid and invalid.</returns>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched.</exception>
        /// <exception cref="SignatureException">The signature data could not be handled.</exception>
        public static IEnumerable<OpenPgpSignature> GetSignatures([NotNull] IOpenPgp openPgp, [NotNull] byte[] feedData)
        {
            #region Sanity checks
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            if (feedData == null) throw new ArgumentNullException("feedData");
            #endregion

            if (feedData.Length == 0) return Enumerable.Empty<OpenPgpSignature>();

            int signatureStartIndex = GetSignatureStartIndex(feedData);
            if (signatureStartIndex == -1) return Enumerable.Empty<OpenPgpSignature>();

            return openPgp.Verify(
                IsolateFeed(feedData, signatureStartIndex),
                IsolateAndDecodeSignature(feedData, signatureStartIndex));
        }

        /// <summary>
        /// Finds the point in a data array where the signature block starts.
        /// </summary>
        /// <param name="feedData">The feed data containing a signature block.</param>
        /// <returns>The index of the first byte of the signature block; -1 if none was found.</returns>
        private static int GetSignatureStartIndex(byte[] feedData)
        {
            int signatureStartIndex = -1;

            for (int currentFeedDataIndex = 0; currentFeedDataIndex < feedData.Length; currentFeedDataIndex++)
            {
                bool validStartingPoint = true;
                for (int i = 0, j = currentFeedDataIndex; j < feedData.Length && i < _signatureBlockStartEncoded.Length; i++, j++)
                {
                    if (feedData[j] == _signatureBlockStartEncoded[i]) continue;

                    validStartingPoint = false;
                    break;
                }
                if (validStartingPoint) signatureStartIndex = currentFeedDataIndex;
            }

            return signatureStartIndex;
        }

        /// <summary>
        /// Isolates the actual feed from the signature block.
        /// </summary>
        /// <param name="feedData">The feed data containing a signature block.</param>
        /// <param name="signatureStartIndex">The index of the first byte of the signature block.</param>
        /// <returns>The isolated feed.</returns>
        /// <exception cref="SignatureException">The signature block does not start on a new line.</exception>
        private static byte[] IsolateFeed(byte[] feedData, int signatureStartIndex)
        {
            if (signatureStartIndex <= 0 || feedData[signatureStartIndex - 1] != Encoding.GetBytes("\n")[0])
                throw new SignatureException(Resources.XmlSignatureMissingNewLine);

            var feed = new byte[signatureStartIndex];
            Array.Copy(feedData, 0, feed, 0, signatureStartIndex);
            return feed;
        }

        /// <summary>
        /// Isolates and decodes the Base64-econded signature.
        /// </summary>
        /// <param name="feedData">The feed data containing a signature block.</param>
        /// <param name="signatureStartIndex">The index of the first byte of the signature block.</param>
        /// <returns>The decoded signature data.</returns>
        /// <exception cref="SignatureException">The signature contains invalid characters.</exception>
        private static byte[] IsolateAndDecodeSignature(byte[] feedData, int signatureStartIndex)
        {
            // Isolate and decode signature string
            var signatureString = Encoding.GetString(feedData, signatureStartIndex, feedData.Length - signatureStartIndex);
            if (!signatureString.EndsWith(SignatureBlockEnd)) throw new SignatureException(Resources.XmlSignatureInvalidEnd);

            // Concatenate Base64 lines and decode
            var base64Charachters = signatureString.Substring(SignatureBlockStart.Length, signatureString.Length - SignatureBlockStart.Length - SignatureBlockEnd.Length).Replace("\n", "");
            try
            {
                return Convert.FromBase64String(base64Charachters);
            }
                #region Error handling
            catch (FormatException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new SignatureException(Resources.XmlSignatureNotBase64 + ex.Message, ex);
            }
            #endregion
        }
        #endregion
    }
}
