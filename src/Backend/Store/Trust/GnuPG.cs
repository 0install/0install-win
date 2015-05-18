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
using NanoByte.Common.Cli;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Provides access to the signature functions of GnuPG.
    /// </summary>
    /// <remarks>This class is immutable and thread-safe.</remarks>
    public class GnuPG : BundledCliAppControl, IOpenPgp
    {
        /// <inheritdoc/>
        protected override string AppBinary { get { return "gpg"; } }

        /// <inheritdoc/>
        protected override string AppDirName { get { return "GnuPG"; } }

        #region Keys
        /// <inheritdoc/>
        public void ImportKey(byte[] data)
        {
            Execute("--batch --no-secmem-warning --quiet --import",
                inputCallback: writer =>
                {
                    writer.BaseStream.Write(data, 0, data.Length);
                    writer.Close();
                });
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
            string result = Execute("--batch --no-secmem-warning --list-secret-keys --with-colons --fixed-list-mode --fingerprint");

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

            string arguments = "--batch --no-secmem-warning --armor --export";
            if (!string.IsNullOrEmpty(keySpecifier)) arguments += " " + keySpecifier.EscapeArgument();

            return Execute(arguments);
        }

        /// <inheritdoc/>
        public Process GenerateKey()
        {
            return GetStartInfo("--gen-key").Start();
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

            string arguments = "--batch --no-secmem-warning --passphrase-fd 0";
            if (!string.IsNullOrEmpty(keySpecifier)) arguments += " --local-user " + keySpecifier.EscapeArgument();
            arguments += " --detach-sign --armor --output - -";

            string output = Execute(arguments, inputCallback: writer =>
            {
                writer.WriteLine(passphrase ?? "");
                stream.CopyTo(writer.BaseStream);
                writer.Close();
            });

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
                result = Execute("--batch --no-secmem-warning --status-fd 1 --verify " + signatureFile.Path.EscapeArgument() + " -",
                    inputCallback: writer =>
                    {
                        writer.BaseStream.Write(data, 0, data.Length);
                        writer.Close();
                    });
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

        #region Execute
        private static readonly object _gpgLock = new object();

        /// <inheritdoc/>
        protected override string Execute(string arguments, Action<StreamWriter> inputCallback = null)
        {
            // Run only one gpg instance at a time to prevent file system race conditions
            lock (_gpgLock)
            {
                return base.Execute(arguments, inputCallback);
            }
        }

        /// <inheritdoc/>
        protected override ProcessStartInfo GetStartInfo(string arguments, bool hidden = false)
        {
            var startInfo = base.GetStartInfo(arguments, hidden);
            if (Locations.IsPortable) startInfo.EnvironmentVariables["GNUPGHOME"] = Locations.GetSaveConfigPath("GnuPG", false, "gnupg");
            return startInfo;
        }

        /// <inheritdoc/>
        protected override string HandleStderr(string line)
        {
            #region Sanity checks
            if (line == null) throw new ArgumentNullException("line");
            #endregion

            if (line.StartsWith("gpg: Signature made ") ||
                line.StartsWith("gpg: Good signature from ") ||
                line.StartsWith("gpg:                 aka") ||
                line.StartsWith("gpg: WARNING: This key is not certified") ||
                line.Contains("There is no indication") ||
                line.StartsWith("Primary key fingerprint: ") ||
                line.StartsWith("gpg: Can't check signature: public key not found"))
                return null;

            if (line.StartsWith("gpg: BAD signature from ") ||
                line.StartsWith("gpg: WARNING:") ||
                (line.StartsWith("gpg: renaming ") && line.EndsWith("failed: Permission denied")))
            {
                Log.Warn(line);
                return null;
            }

            if (line.StartsWith("gpg: waiting for lock") ||
                (line.StartsWith("gpg: keyring ") && line.EndsWith(" created")) ||
                (line.StartsWith("gpg: ") && line.EndsWith(": trustdb created")))
            {
                Log.Info(line);
                return null;
            }

            if (line.StartsWith("gpg: skipped ") && line.EndsWith(": bad passphrase")) throw new WrongPassphraseException();
            if (line.StartsWith("gpg: signing failed: bad passphrase")) throw new WrongPassphraseException();
            if (line.StartsWith("gpg: signing failed: file exists")) throw new IOException(Resources.SignatureAldreadyExists);
            if (line.StartsWith("gpg: signing failed: ") ||
                line.StartsWith("gpg: error") ||
                line.StartsWith("gpg: critical"))
                throw new IOException(line);

            // Unknown GnuPG message
            Log.Warn(line);
            return null;
        }
        #endregion
    }
}
