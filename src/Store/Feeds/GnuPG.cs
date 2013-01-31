/*
 * Copyright 2010-2013 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Text.RegularExpressions;
using Common;
using Common.Cli;
using Common.Collections;
using Common.Storage;
using Common.Utils;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Provides access to the signature functions of GnuPG.
    /// </summary>
    /// <remarks>This class is immutable (excpet for the <see cref="Verbose"/> option) and thread-safe.</remarks>
    public class GnuPG : BundledCliAppControl, IOpenPgp
    {
        #region Properties
        /// <inheritdoc/>
        public bool Verbose { get; set; }

        /// <inheritdoc/>
        protected override string AppBinary { get { return "gpg"; } }

        /// <inheritdoc/>
        protected override string AppDirName { get { return "GnuPG"; } }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        protected override ProcessStartInfo GetStartInfo(string arguments)
        {
            var startInfo = base.GetStartInfo(arguments);

            // Pass-through portable mode
            if (Locations.IsPortable) startInfo.EnvironmentVariables["GNUPGHOME"] = Locations.GetSaveConfigPath("GnuPG", false, "gnupg");

            return startInfo;
        }
        #endregion

        #region Keys
        /// <inheritdoc/>
        public void ImportKey(byte[] data)
        {
            Execute("--batch --no-secmem-warning --quiet --import", writer =>
            {
                writer.BaseStream.Write(data, 0, data.Length);
                writer.Close();
            }, ErrorHandlerException);
        }

        /// <inheritdoc/>
        public OpenPgpSecretKey GetSecretKey(string keySpecifier)
        {
            // Get all available secret keys
            var secretKeys = ListSecretKeys();

            if (secretKeys.Length == 0) throw new KeyNotFoundException(Resources.UnableToFindSecretKey);
            if (string.IsNullOrEmpty(keySpecifier)) return secretKeys[0];

            // Find the first secret key that matches the key specifier
            return secretKeys.First(
                key => key.Fingerprint == keySpecifier || key.KeyID == keySpecifier || key.UserID.ContainsIgnoreCase(keySpecifier),
                () => new KeyNotFoundException(Resources.UnableToFindSecretKey));
        }

        /// <inheritdoc/>
        public OpenPgpSecretKey[] ListSecretKeys()
        {
            string result = Execute("--batch --no-secmem-warning --list-secret-keys --with-colons --fixed-list-mode --fingerprint", null, ErrorHandlerException);
            string[] lines = result.SplitMultilineText();

            // Each secret key is represented by 4 lines of encoded information
            var keys = new List<OpenPgpSecretKey>(lines.Length / 4);
            for (int i = 0; i + 4 < lines.Length; i += 4)
            {
                string secLine = lines[i + 0];
                string fprLine = lines[i + 1];
                string uidLine = lines[i + 2];
                //string ssbLine = lines[i + 3];
                try
                {
                    keys.Add(OpenPgpSecretKey.Parse(secLine, fprLine, uidLine));
                }
                    #region Error handling
                catch (FormatException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    throw new UnhandledErrorsException(ex.Message, ex);
                }
                #endregion
            }

            return keys.ToArray();
        }

        /// <inheritdoc/>
        public string GetPublicKey(string keySpecifier)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(keySpecifier)) throw new ArgumentNullException("keySpecifier");
            #endregion

            string arguments = "--batch --no-secmem-warning --armor --export";
            if (!string.IsNullOrEmpty(keySpecifier)) arguments += " --local-user " + keySpecifier.EscapeArgument();

            return Execute(arguments, null, ErrorHandlerException);
        }
        #endregion

        #region Sign
        /// <inheritdoc/>
        public void DetachSign(string path, string keySpecifier, string passphrase)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (!File.Exists(path)) throw new FileNotFoundException(Resources.FileToSignNotFound, path);
            if (string.IsNullOrEmpty(keySpecifier)) throw new ArgumentNullException("keySpecifier");
            if (passphrase == null) throw new ArgumentNullException("passphrase");
            #endregion

            string arguments = "--batch --no-secmem-warning --passphrase-fd 0";
            if (!string.IsNullOrEmpty(keySpecifier)) arguments += " --local-user " + keySpecifier.EscapeArgument();
            arguments += " --detach-sign " + path.EscapeArgument();

            if (string.IsNullOrEmpty(passphrase)) passphrase = "\n";
            Execute(arguments, writer => writer.WriteLine(passphrase), ErrorHandlerException);
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
                File.WriteAllBytes(signatureFile.Path, signature);
                string arguments = "--batch --no-secmem-warning --status-fd 1 --verify " + signatureFile.Path.EscapeArgument() + " -";
                result = Execute(arguments, writer =>
                {
                    writer.BaseStream.Write(data, 0, data.Length);
                    writer.Close();
                }, ErrorHandlerLog);
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
                    throw new UnhandledErrorsException(ex.Message, ex);
                }
                #endregion
            }

            return signatures;
        }
        #endregion

        #region Error handler
        /// <summary>
        /// Provides error handling for GnuPG stderr with exceptions.
        /// </summary>
        /// <param name="line">The error line written to stderr.</param>
        /// <returns>Always <see langword="null"/>.</returns>
        /// <exception cref="WrongPassphraseException">Thrown if passphrase was incorrect.</exception>
        private static string ErrorHandlerException(string line)
        {
            if (new Regex("gpg: skipped \"[\\w\\W]*\": bad passphrase").IsMatch(line)) throw new WrongPassphraseException();
            if (line.StartsWith("gpg: signing failed: bad passphrase")) throw new WrongPassphraseException();
            if (line.StartsWith("gpg: signing failed: file exists")) throw new IOException(Resources.SignatureAldreadyExists);
            throw new SignatureException(line);
        }

        /// <summary>
        /// Provides error handling for GnuPG stderr with loggiing.
        /// </summary>
        /// <param name="line">The error line written to stderr.</param>
        /// <returns>Always <see langword="null"/>.</returns>
        private string ErrorHandlerLog(string line)
        {
            if (Verbose) Log.Info(line);
            return null;
        }
        #endregion
    }
}
