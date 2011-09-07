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
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Common.Cli;
using Common.Storage;
using Common.Utils;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Provides access to the signature functions of GnuPG.
    /// </summary>
    public class GnuPG : BundledCliAppControl, IOpenPgp
    {
        #region Properties
        /// <inheritdoc/>
        protected override string AppBinary { get { return "gpg"; } }

        /// <inheritdoc/>
        protected override string AppDirName { get { return "GnuPG"; } }
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Runs GnuPG, processes its output and waits until it has terminated.
        /// </summary>
        /// <param name="arguments">Command-line arguments to launch the application with.</param>
        /// <param name="defaultInput">Data to write to the application's stdin-stream right after startup; <see langword="null"/> for none.</param>
        /// <returns>The application's complete output to the stdout-stream.</returns>
        /// <exception cref="IOException">Thrown if GnuPG could not be launched.</exception>
        private string Execute(string arguments, string defaultInput)
        {
            return Execute(arguments, defaultInput, ErrorHandler);
        }

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
        public void ImportKey(Stream stream)
        {
            // ToDo: Implement
        }

        /// <inheritdoc/>
        public string GetPublicKey(string name)
        {
            string arguments = "--batch --no-secmem-warning --armor --export";
            if (!string.IsNullOrEmpty(name)) arguments += " --local-user " + StringUtils.EscapeArgument(name);

            return Execute(arguments, null);
        }

        /// <inheritdoc/>
        public OpenPgpSecretKey GetSecretKey(string name)
        {
            var secretKeys = ListSecretKeys();

            if (secretKeys.Length == 0) throw new KeyNotFoundException(Resources.UnableToFindSecretKey);
            if (string.IsNullOrEmpty(name)) return secretKeys[0];

            foreach (var key in secretKeys)
            {
                if (key.KeyID == name || StringUtils.Contains(key.UserID, name))
                    return key;
            }
            throw new KeyNotFoundException(Resources.UnableToFindSecretKey);
        }

        /// <inheritdoc/>
        public OpenPgpSecretKey[] ListSecretKeys()
        {
            string result = Execute("--batch --no-secmem-warning --list-secret-keys --with-colons", null);
            string[] lines = StringUtils.SplitMultilineText(result);
            var keys = new List<OpenPgpSecretKey>(lines.Length / 2);

            foreach (var line in lines)
                if (line.StartsWith("sec")) keys.Add(OpenPgpSecretKey.Parse(line));

            return keys.ToArray();
        }

        /// <inheritdoc />
        public bool IsPassphraseCorrect(string name, string passphrase)
        {
            if (string.IsNullOrEmpty(passphrase)) return false;

            string tempFilePath = FileUtils.GetTempFile("gpg");

            try
            {
                DetachSign(tempFilePath, name, passphrase);
            }
            catch(WrongPassphraseException)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Sign
        /// <inheritdoc/>
        public void DetachSign(string path, string name, string passphrase)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (!File.Exists(path)) throw new FileNotFoundException(Resources.FileToSignNotFound, path);
            #endregion

            string arguments = "--batch --no-secmem-warning --passphrase-fd 0";
            if (!string.IsNullOrEmpty(name)) arguments += " --local-user \"" + name.Replace("\"", "\\\"") + "\"";
            arguments += " --detach-sign \"" + path.Replace("\"", "\\\")" + "\"");

            if (string.IsNullOrEmpty(passphrase)) passphrase = "\n";
            Execute(arguments, passphrase);
        }
        #endregion

        #region Verify
        /// <inheritdoc/>
        public OpenPgpSignature[] Verify(Stream data, byte[] signature)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            if (signature == null) throw new ArgumentNullException("signature");
            #endregion

            // ToDo: Implement
            //string result;
            //using (var signatureFile = new TemporaryFile("0install-sig"))
            //{
            //    string arguments = "--batch --no-secmem-warning --status-fd 1 --verify \"" + signatureFile.Path + "\" -";
            //    result = Execute(arguments, data);
            //}
            throw new NotImplementedException();
        }
        #endregion

        #region Error handler
        /// <summary>
        /// Provides error handling for GnuPG stderr.
        /// </summary>
        /// <param name="line">The error line written to stderr.</param>
        /// <returns>Always <see langword="null"/>.</returns>
        /// <exception cref="WrongPassphraseException">Thrown if passphrase was incorrect.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        private static string ErrorHandler(string line)
        {
            if (new Regex("gpg: skipped \"[\\w\\W]*\": bad passphrase").IsMatch(line)) throw new WrongPassphraseException();
            if (line.StartsWith("gpg: signing failed: bad passphrase")) throw new WrongPassphraseException();
            if (line.StartsWith("gpg: signing failed: file exists")) throw new IOException(Resources.SignatureAldreadyExists);
            throw new UnhandledErrorsException(line);
        }
        #endregion
    }
}
