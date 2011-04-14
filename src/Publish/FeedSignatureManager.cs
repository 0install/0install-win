/*
 * Copyright 2011 Simon E. Silva Lauinger
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

using System.IO;
using System;
using System.Collections.Generic;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Publish
{
    // TODO: TEST!!!
    public class FeedSignatureManager
    {
        /// <summary>
        /// Returns the decoded signature of a feed.
        /// </summary>
        /// <param name="path">to the feed.</param>
        /// <returns>The decoded signature.</returns>
        /// <exception cref="FormatException">If the signature could't be decoded.</exception>
        public byte[] GetSignature(string path)
        {
            string base64Signature = GetBase64Signature(File.ReadAllText(path));
            return Convert.FromBase64String(base64Signature);
        }

        /// <summary>
        /// Parses the base64 signature out of the signature comment.
        /// </summary>
        /// <param name="feed">to parse out the signature from.</param>
        /// <returns>The base64 signature.</returns>
        /// <exception cref="SignatureException">If there is any defect with the signature.</exception>
        private string GetBase64Signature(string feed)
        {           
            const string signatureStartComment = "<!-- Base64 Signature";

            int signatureBlockStartIndex = feed.LastIndexOf("\n" + signatureStartComment);

            if (signatureBlockStartIndex == -1)
                throw new SignatureException("No signature block in XML. Maybe this file isn't signed?");

            signatureBlockStartIndex += 1; // include new-line in data

            string signatureBlock = feed.Substring(signatureBlockStartIndex);
            string[] signatureBlockLines = signatureBlock.Split('\n');

            if (signatureBlockLines[0].Trim() != signatureStartComment)
                throw new SignatureException("Bad signature block: extra data on comment line.");
            if (signatureBlockLines[signatureBlockLines.Length - 1].Trim() != "-->")
                throw new SignatureException("Bad signature block: last line is not end-of-comment.");

            var signatureLines = new LinkedList<string>(signatureBlockLines);
            // remove start comment
            signatureLines.RemoveFirst();
            // remove end comment
            signatureLines.RemoveLast();

            return "\n" + string.Concat(signatureLines);
        }

        
    }
}

