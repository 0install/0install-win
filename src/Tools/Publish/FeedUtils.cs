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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NanoByte.Common;
using NanoByte.Common.Streams;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Helper methods for manipulating <see cref="Feed"/>s.
    /// </summary>
    public static class FeedUtils
    {
        /// <summary>
        /// Writes the default XSL stylesheet with its accompanying CSS file unless there is already an XSL in place.
        /// </summary>
        /// <param name="path">The directory to write the stylesheet files to.</param>
        /// <exception cref="IOException">Failed to write the sytelsheet files.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the directory is not permitted.</exception>
        public static void DeployStylesheet(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            if (!File.Exists(Path.Combine(path, "feed.xsl")))
            {
                var assembly = Assembly.GetAssembly(typeof(FeedUtils));
                using (var stream = assembly.GetManifestResourceStream(typeof(FeedUtils), "feed.xsl"))
                    stream.WriteTo(Path.Combine(path, "feed.xsl"));
                using (var stream = assembly.GetManifestResourceStream(typeof(FeedUtils), "feed.css"))
                    stream.WriteTo(Path.Combine(path, "feed.css"));
            }
        }

        /// <summary>
        /// Adds a Base64 signature to a feed or catalog stream.
        /// </summary>
        /// <param name="stream">The feed or catalog to sign.</param>
        /// <param name="secretKey">The secret key to use for signing the file.</param>
        /// <param name="passphrase">The passphrase to use to unlock the key.</param>
        /// <param name="openPgp">The OpenPGP-compatible system used to create signatures.</param>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched or the file could not be read or written.</exception>
        /// <exception cref="WrongPassphraseException">Passphrase was incorrect.</exception>
        /// <remarks>
        /// The file is not parsed before signing; invalid XML files are signed as well.
        /// The existing file must end with a line break.
        /// Old signatures are not removed.
        /// </remarks>
        public static void SignFeed(Stream stream, OpenPgpSecretKey secretKey, string passphrase, IOpenPgp openPgp)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            if (secretKey == null) throw new ArgumentNullException("secretKey");
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            #endregion

            // Calculate the signature in-memory
            string signature = openPgp.DetachSign(stream, secretKey.Fingerprint, passphrase);

            // Add the signature to the end of the file
            var writer = new StreamWriter(stream, encoding: Store.Feeds.FeedUtils.Encoding) {NewLine = "\n"};
            writer.Write(Store.Feeds.FeedUtils.SignatureBlockStart);
            writer.WriteLine(signature);
            writer.Write(Store.Feeds.FeedUtils.SignatureBlockEnd);
            writer.Flush();
        }

        /// <summary>
        /// Exports an OpenPGP public key to a key file.
        /// </summary>
        /// <param name="path">The directory to write the key file to.</param>
        /// <param name="secretKey">The secret key to get the public kyey for.</param>
        /// <param name="openPgp">The OpenPGP-compatible system used to create signatures.</param>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched or the file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the directory is not permitted.</exception>
        public static void DeployPublicKey(string path, OpenPgpSecretKey secretKey, IOpenPgp openPgp)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (secretKey == null) throw new ArgumentNullException("secretKey");
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            #endregion

            File.WriteAllText(
                path: Path.Combine(path, secretKey.KeyID + ".gpg"),
                contents: openPgp.GetPublicKey(secretKey.Fingerprint),
                encoding: Encoding.ASCII);
        }

        /// <summary>
        /// Determines the key used to sign a feed or catalog file. Only uses the first signature if more than one is present.
        /// </summary>
        /// <param name="path">The feed or catalog file to check for signatures.</param>
        /// <param name="openPgp">The OpenPGP-compatible system used to validate the signatures.</param>
        /// <returns>The key used to sign the file; <see langword="null"/> if the file was not signed.</returns>
        /// <exception cref="FileNotFoundException">The file file could not be found.</exception>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched or the file could not be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        public static OpenPgpSecretKey GetKey(string path, IOpenPgp openPgp)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            #endregion

            try
            {
                var signatures = Store.Feeds.FeedUtils.GetSignatures(openPgp, File.ReadAllBytes(path));

                foreach (var signature in signatures.OfType<ValidSignature>())
                    return openPgp.GetSecretKey(signature.Fingerprint);
            }
                #region Error handling
            catch (KeyNotFoundException)
            {
                Log.Info(Resources.SecretKeyNotInKeyring);
            }
            catch (SignatureException ex)
            {
                // Unable to parse the signature
                Log.Error(ex);
            }
            #endregion

            return null;
        }
    }
}
