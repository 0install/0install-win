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
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Provides access to the signature functions of GnuPG.
    /// </summary>
    /// <remarks>This class is immutable and thread-safe.</remarks>
    public partial class GnuPG : IOpenPgp
    {
        #region Keys
        /// <inheritdoc/>
        public void ImportKey(byte[] data)
        {
            new CliControlData(data).Execute("--batch", "--no-secmem-warning", "--quiet", "--import");
        }

        /// <inheritdoc/>
        public OpenPgpSecretKey GetSecretKey(string keySpecifier = null)
        {
            // Get all available secret keys
            var secretKeys = ListSecretKeys();

            // Find the first secret key that matches the key specifier
            try
            {
                return string.IsNullOrEmpty(keySpecifier)
                    ? secretKeys.First()
                    : secretKeys.First(key => key.Fingerprint == keySpecifier || key.KeyID == keySpecifier || key.UserID.ContainsIgnoreCase(keySpecifier));
            }
                #region Error handling
            catch
            {
                throw new KeyNotFoundException(Resources.UnableToFindSecretKey);
            }
            #endregion
        }

        /// <inheritdoc/>
        public IEnumerable<OpenPgpSecretKey> ListSecretKeys()
        {
            string result = new CliControl().Execute("--batch", "--no-secmem-warning", "--list-secret-keys", "--with-colons", "--fixed-list-mode", "--fingerprint");

            string[] sec = null, fpr = null, uid = null;
            foreach (string line in result.SplitMultilineText())
            {
                var parts = line.Split(':');
                switch (parts[0])
                {
                    case "sec":
                        // New element starting
                        if (sec != null && fpr != null && uid != null) yield return ParseSecretKey(sec, fpr, uid);
                        sec = parts;
                        fpr = null;
                        uid = null;
                        break;

                    case "fpr":
                        fpr = parts;
                        break;

                    case "uid":
                        uid = parts;
                        break;
                }
            }

            if (sec != null && fpr != null && uid != null) yield return ParseSecretKey(sec, fpr, uid);
        }

        [NotNull]
        private static OpenPgpSecretKey ParseSecretKey([NotNull] string[] sec, [NotNull] string[] fpr, [NotNull] string[] uid)
        {
            return new OpenPgpSecretKey(
                fpr[9], sec[4], uid[9],
                FileUtils.FromUnixTime(long.Parse(sec[5])), (OpenPgpAlgorithm)int.Parse(sec[3]), int.Parse(sec[2]));
        }

        /// <inheritdoc/>
        public string GetPublicKey(string keySpecifier)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(keySpecifier)) throw new ArgumentNullException("keySpecifier");
            #endregion

            var arguments = new[] {"--batch", "--no-secmem-warning", "--armor", "--export"};
            if (!string.IsNullOrEmpty(keySpecifier)) arguments = arguments.Append(keySpecifier);

            return new CliControl().Execute(arguments);
        }

        /// <summary>
        /// Launches an interactive process for generating a new keypair.
        /// </summary>
        /// <returns>A handle that can be used to wait for the process to finish.</returns>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched.</exception>
        public static Process GenerateKey()
        {
            return new CliControl().StartInteractive("--gen-key");
        }
        #endregion

        #region Sign
        /// <inheritdoc/>
        public string DetachSign(Stream stream, string keySpecifier, string passphrase = null)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            if (string.IsNullOrEmpty(keySpecifier)) throw new ArgumentNullException("keySpecifier");
            #endregion

            var arguments = string.IsNullOrEmpty(keySpecifier)
                ? new[] {"--batch", "--no-secmem-warning", "--passphrase-fd", "0", "--detach-sign", "--armor", "--output", "-", "-"}
                : new[] {"--batch", "--no-secmem-warning", "--passphrase-fd", "0", "--local-user", keySpecifier, "--detach-sign", "--armor", "--output", "-", "-"};

            string output = new CliControlStream(passphrase, stream).Execute(arguments);

            return output
                .GetRightPartAtFirstOccurrence(Environment.NewLine + Environment.NewLine)
                .GetLeftPartAtLastOccurrence(Environment.NewLine + "=")
                .Replace(Environment.NewLine, "\n");
        }
        #endregion

        #region Verify
        /// <inheritdoc/>
        public IEnumerable<OpenPgpSignature> Verify(byte[] data, byte[] signature)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            if (signature == null) throw new ArgumentNullException("signature");
            #endregion

            string result;
            using (var signatureFile = new TemporaryFile("0install-sig"))
            {
                File.WriteAllBytes(signatureFile, signature);
                result = new CliControlData(data).Execute("--batch", "--no-secmem-warning", "--status-fd", "1", "--verify", signatureFile.Path, "-");
            }
            string[] lines = result.SplitMultilineText();

            // Each signature is represented by one line of encoded information
            var signatures = new List<OpenPgpSignature>(lines.Length);
            foreach (var line in lines)
            {
                try
                {
                    var parsedSignature = OpenPgpSignature.Parse(line);
                    if (parsedSignature != null) signatures.Add(parsedSignature);
                }
                    #region Error handling
                catch (FormatException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    throw new IOException(ex.Message, ex);
                }
                #endregion
            }

            return signatures;
        }
        #endregion
    }
}
