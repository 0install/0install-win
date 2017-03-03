/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
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
        /// Writes an XSL stylesheet with its accompanying CSS file unless there is already an XSL in place.
        /// </summary>
        /// <param name="path">The directory to write the stylesheet files to.</param>
        /// <param name="name">The name of the stylesheet to deploy. Must be "feed" or "catalog".</param>
        /// <exception cref="IOException">Failed to write the sytelsheet files.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the directory is not permitted.</exception>
        public static void DeployStylesheet([NotNull] string path, [NotNull] string name)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            #endregion

            if (!File.Exists(Path.Combine(path, name + ".xsl")))
            {
                DeployEmbeddedFile(name + ".xsl", path);
                DeployEmbeddedFile(name + ".css", path);
                switch (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                {
                    case "de":
                        DeployEmbeddedFile(name + ".xsl.de", path);
                        break;
                }
            }
        }

        private static void DeployEmbeddedFile(string fileName, string targetDir)
            => typeof(FeedUtils).CopyEmbeddedToFile(fileName, Path.Combine(targetDir, fileName));

        /// <summary>
        /// Adds a Base64 signature to a feed or catalog stream.
        /// </summary>
        /// <param name="stream">The feed or catalog to sign.</param>
        /// <param name="secretKey">The secret key to use for signing the file.</param>
        /// <param name="passphrase">The passphrase to use to unlock the key.</param>
        /// <param name="openPgp">The OpenPGP-compatible system used to create signatures.</param>
        /// <exception cref="IOException">The file could not be read or written.</exception>
        /// <exception cref="UnauthorizedAccessException">Read or write access to the file is not permitted.</exception>
        /// <exception cref="KeyNotFoundException">The specified <paramref name="secretKey"/> could not be found on the system.</exception>
        /// <exception cref="WrongPassphraseException"><paramref name="passphrase"/> was incorrect.</exception>
        /// <remarks>
        /// The file is not parsed before signing; invalid XML files are signed as well.
        /// The existing file must end with a line break.
        /// Old signatures are not removed.
        /// </remarks>
        public static void SignFeed([NotNull] Stream stream, [NotNull] OpenPgpSecretKey secretKey, [CanBeNull] string passphrase, [NotNull] IOpenPgp openPgp)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (secretKey == null) throw new ArgumentNullException(nameof(secretKey));
            if (openPgp == null) throw new ArgumentNullException(nameof(openPgp));
            #endregion

            // Calculate the signature in-memory
            var signature = openPgp.Sign(stream.ToArray(), secretKey, passphrase);

            // Add the signature to the end of the file
            var writer = new StreamWriter(stream, Store.Feeds.FeedUtils.Encoding) {NewLine = "\n"};
            writer.Write(Store.Feeds.FeedUtils.SignatureBlockStart);
            writer.WriteLine(Convert.ToBase64String(signature));
            writer.Write(Store.Feeds.FeedUtils.SignatureBlockEnd);
            writer.Flush();
        }

        /// <summary>
        /// Determines the key used to sign a feed or catalog file. Only uses the first signature if more than one is present.
        /// </summary>
        /// <param name="path">The feed or catalog file to check for signatures.</param>
        /// <param name="openPgp">The OpenPGP-compatible system used to validate the signatures.</param>
        /// <returns>The key used to sign the file; <c>null</c> if the file was not signed.</returns>
        /// <exception cref="FileNotFoundException">The file file could not be found.</exception>
        /// <exception cref="IOException">The file could not be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        [CanBeNull]
        public static OpenPgpSecretKey GetKey([NotNull] string path, [NotNull] IOpenPgp openPgp)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            if (openPgp == null) throw new ArgumentNullException(nameof(openPgp));
            #endregion

            try
            {
                var signature = Store.Feeds.FeedUtils.GetSignatures(openPgp, File.ReadAllBytes(path))
                    .OfType<ValidSignature>().FirstOrDefault();
                if (signature == null) return null;

                return openPgp.GetSecretKey(signature);
            }
                #region Error handling
            catch (KeyNotFoundException)
            {
                Log.Info(Resources.SecretKeyNotInKeyring);
                return null;
            }
            catch (SignatureException ex)
            {
                // Unable to parse the signature
                Log.Error(ex);
                return null;
            }
            #endregion
        }
    }
}
