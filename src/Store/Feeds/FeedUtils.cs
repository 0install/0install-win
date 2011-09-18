/*
 * Copyright 2010-2011 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Text;
using C5;
using Common;
using Common.Cli;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Provides utiltity methods for managing <see cref="Feed"/>s.
    /// </summary>
    public static class FeedUtils
    {
        #region Constants
        private static readonly byte[] _signatureCommentStart = Encoding.UTF8.GetBytes("<!-- Base64 Signature\n");
        private static readonly byte[] _signatureCommentEnd = Encoding.UTF8.GetBytes("\n-->");
        private static readonly byte _newLine = Encoding.UTF8.GetBytes("\n")[0];
        #endregion

        #region Cache
        /// <summary>
        /// Loads all <see cref="Feed"/>s stored in <see cref="IFeedCache"/> into memory.
        /// </summary>
        /// <param name="cache">The <see cref="IFeedCache"/> to load <see cref="Feed"/>s from.</param>
        /// <returns>The parsed <see cref="Feed"/>s.</returns>
        public static IEnumerable<Feed> GetFeeds(IFeedCache cache)
        {
            #region Sanity checks
            if (cache == null) throw new ArgumentNullException("cache");
            #endregion

            var feeds = new System.Collections.Generic.LinkedList<Feed>();
            foreach (string id in cache.ListAll())
            {
                try
                {
                    var feed = cache.GetFeed(id);
                    feed.Simplify();
                    feeds.AddLast(feed);
                }
                #region Error handling
                catch (IOException ex) { Log.Error(ex.Message); }
                catch (UnauthorizedAccessException ex) { Log.Error(ex.Message); }
                catch (InvalidDataException ex) { Log.Error(ex.Message); }
                #endregion
            }
            return feeds;
        }
        #endregion

        #region Signatures
        /// <summary>
        /// Determines which signatures a feed is signed with.
        /// </summary>
        /// <param name="openPgp">The OpenPGP-compatible system used to validate the signatures.</param>
        /// <param name="feedData">The feed data containing an embedded signature.</param>
        /// <returns>A list of signatures found, both valid and invalid.</returns>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched.</exception>
        /// <exception cref="SignatureException">Thrown if the signature data could not be handled.</exception>
        public static IEnumerable<OpenPgpSignature> GetSignatures(IOpenPgp openPgp, byte[] feedData)
        {
            #region Sanity checks
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            if (feedData == null) throw new ArgumentNullException("feedData");
            #endregion

            if (feedData.Length == 0) return new OpenPgpSignature[0];

            int signatureCommentStartIndex = FindSignatureCommentStartingPoint(ref feedData);
            if (signatureCommentStartIndex == -1) return new OpenPgpSignature[0];

            // check signature comment start for validity
            if (!IsSignatureInNewLine(ref feedData, signatureCommentStartIndex)) throw new SignatureException("The signature must be in a new line.");

            // get signature
            var signatureStartIndex = GetSignatureStartIndex(signatureCommentStartIndex);
            var signatureEndIndex = FindSignatureEndIndex(ref feedData, signatureStartIndex);
            if (signatureEndIndex == signatureStartIndex) throw new SignatureException("Signature does not end with a new line.");
            var signature = GetSignature(ref feedData, signatureStartIndex, signatureEndIndex);

            //check signature comment end for validity
            if(!HasEmptyLineBetweenSignatureAndSignatureComment(ref feedData, signatureEndIndex)) throw new SignatureException("There must be an empty line between the signature and the signature comment end.");
            var signatureCommentEndIndex = GetSignatureCommentEndIndex(signatureEndIndex);
            if (IsSignatureCommentEndTooShort(ref feedData, signatureCommentEndIndex)) throw new SignatureException("Bad signature block: last signature comment line is too short.");
            if (!IsSignatureCommentEndValid(ref feedData, signatureCommentEndIndex)) throw new SignatureException("Bad signature block: last signature comment line is not \"-->\".");
            if (AnyDataAfterSignatureBlock(ref feedData, signatureCommentEndIndex)) throw new SignatureException("Bad signature block: there is some extra data after the signature block.");

            byte[] feed = SeperateFeedFromSignatureBlock(ref feedData, signatureCommentStartIndex);

            try { return openPgp.Verify(feed, signature); }
            #region Error handling
            catch (UnhandledErrorsException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new SignatureException(ex.Message, ex);
            }
            #endregion}
        }

        private static int FindSignatureCommentStartingPoint(ref byte[] feedData)
        {           
            int signatureStartIndex = -1;

            for (int currentFeedDataIndex = 0; currentFeedDataIndex < feedData.Length; currentFeedDataIndex++)
            {
                bool validStartingPoint = true;
                for(int i = 0, j = currentFeedDataIndex; j < feedData.Length && i < _signatureCommentStart.Length; i++, j++)
                {
                    if (feedData[j] == _signatureCommentStart[i]) continue;
                    
                    validStartingPoint = false;
                    break;
                }
                if (validStartingPoint) signatureStartIndex = currentFeedDataIndex;
            }

            return signatureStartIndex;
        }

        private static bool IsSignatureInNewLine(ref byte[] feedData, int signatureCommentStartIndex)
        {
            if (signatureCommentStartIndex <= 0) return false;

            return feedData[signatureCommentStartIndex - 1] == _newLine;
        }

        private static int GetSignatureStartIndex(int signatureCommentStartIndex)
        {
            return signatureCommentStartIndex + _signatureCommentStart.Length;
        }

        private static byte[] GetSignature(ref byte[] feedData, int signatureStartIndex, int signatureEndIndex)
        {
            var base64Signature = Encoding.UTF8.GetChars(feedData, signatureStartIndex, signatureEndIndex - signatureStartIndex);
            return Convert.FromBase64CharArray(base64Signature, 0, base64Signature.Length);
        }

        private static int FindSignatureEndIndex(ref byte[] feedData, int signatureStartIndex)
        {
            int signatureEndIndex = signatureStartIndex;
            for (int currentSignaturePosition = signatureStartIndex; currentSignaturePosition < feedData.Length; currentSignaturePosition++)
            {
                if (feedData[currentSignaturePosition] == _newLine)
                {
                    signatureEndIndex = currentSignaturePosition;
                    break;
                }
            }

            return signatureEndIndex;
        }

        private static bool HasEmptyLineBetweenSignatureAndSignatureComment(ref byte[] feedData, int signatureEndIndex)
        {
            if (signatureEndIndex + 2 > feedData.Length) return false;
            return feedData[signatureEndIndex+1] == _newLine && feedData[signatureEndIndex+2] == _newLine;
        }

        private static int GetSignatureCommentEndIndex(int signatureEndIndex)
        {
            return signatureEndIndex + 2;
        }

        private static bool IsSignatureCommentEndTooShort(ref byte[] feedData, int signatureCommentEndIndex)
        {
            return signatureCommentEndIndex + _signatureCommentEnd.Length > feedData.Length;
        }

        private static bool IsSignatureCommentEndValid(ref byte[] feedData, int signatureCommentEndIndex)
        {
            for(int j = signatureCommentEndIndex, i = 0; i < _signatureCommentEnd.Length; j++, i++)
            {
                if (feedData[j] != _signatureCommentEnd[i]) return false;
            }

            return true;
        }

        private static bool AnyDataAfterSignatureBlock(ref byte[] feedData, int signatureCommentEndIndex)
        {
            return feedData.Length != signatureCommentEndIndex + _signatureCommentEnd.Length;
        }

        private static byte[] SeperateFeedFromSignatureBlock(ref byte[] feedData, int signatureCommentStartIndex)
        {
            int signatureLength = feedData.Length - signatureCommentStartIndex;
            int feedLength = feedData.Length - signatureLength;
            
            var feed = new byte[feedLength];
            Array.Copy(feedData, 0, feed, 0, feedLength);

            return feed;
        }
        #endregion
    }
}
