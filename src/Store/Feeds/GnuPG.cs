/*
 * Copyright 2010 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Text.RegularExpressions;
using Common.Cli;
using Common.Utils;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Provides access to the signature functions of GnuPG.
    /// </summary>
    public class GnuPG : CliAppControl
    {
        #region Properties
        /// <inheritdoc/>
        protected override string AppName { get { return "GnuPG"; } }

        /// <inheritdoc/>
        protected override string AppBinary { get { return "gpg"; } }
        #endregion

        //--------------------//

        #region Keys
        /// <summary>
        /// Returns a specific public key.
        /// </summary>
        /// <param name="name">The name of the user or the ID of the private key to use for signing the file; <see langword="null"/> for default key.</param>
        /// <returns>The public key in the ASCII Armored format.</returns>
        /// <exception cref="IOException">Thrown if the GnuPG could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        public string GetPublicKey(string name)
        {
            string arguments = "--batch --no-secmem-warning --armor --export";
            if (!string.IsNullOrEmpty(name)) arguments += " --local-user \"" + name.Replace("\"", "\\\"") + "\"";

            return Execute(arguments, null, ErrorHandler);
        }

        /// <summary>
        /// Returns information about a specific secret key.
        /// </summary>
        /// <param name="name">The name of the user or the ID of the private key to get information about; <see langword="null"/> for default key.</param>
        /// <exception cref="IOException">Thrown if the GnuPG could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        /// <exception cref="FormatException">Thrown if GnuPG's output cannot be properly parsed.</exception>
        public GnuPGSecretKey GetSecretKey(string name)
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

        /// <summary>
        /// Returns a list of information about available secret keys.
        /// </summary>
        /// <exception cref="IOException">Thrown if the GnuPG could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        /// <exception cref="FormatException">Thrown if GnuPG's output cannot be properly parsed.</exception>
        public GnuPGSecretKey[] ListSecretKeys()
        {
            string result = Execute("--batch --no-secmem-warning --list-secret-keys --with-colons", null, ErrorHandler);
            string[] lines = StringUtils.SplitMultilineText(result);
            var keys = new List<GnuPGSecretKey>(lines.Length / 2);

            foreach (var line in lines)
                if (line.StartsWith("sec")) keys.Add(GnuPGSecretKey.Parse(line));

            return keys.ToArray();
        }
        #endregion

        #region Signature
        /// <summary>
        /// Creates a detached signature for a specific file using the user's default key.
        /// </summary>
        /// <param name="path">The file to create the signature for.</param>
        /// <param name="name">The name of the user or the ID of the private key to use for signing the file; <see langword="null"/> for default key.</param>
        /// <param name="passphrase">The passphrase to use to unlock the user's default key.</param>
        /// <exception cref="FileNotFoundException">Thrown if the file to be signed could not be found.</exception>
        /// <exception cref="IOException">Thrown if the GnuPG could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        public void DetachSign(string path, string name, string passphrase)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(passphrase)) throw new ArgumentNullException("passphrase");
            if (!File.Exists(path)) throw new FileNotFoundException(Resources.FileToSignNotFound, path);
            #endregion

            string arguments = "--batch --no-secmem-warning --passphrase-fd 0";
            if (!string.IsNullOrEmpty(name)) arguments += " --local-user \"" + name.Replace("\"", "\\\"") + "\"";
            arguments += " --detach-sign \"" + path.Replace("\"", "\\\")" + "\"");

            Execute(arguments, passphrase, ErrorHandler);
        }
        #endregion

        #region Verify
        /// <summary>
        /// ToDo
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="data"></param>
        public void Verify(string signature, string data)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(signature)) throw new ArgumentNullException("signature");
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            // ToDo: Implement
            //string result;
            //using (var signatureFile = new TemporaryFile("0install-sig"))
            //{
            //    string arguments = "--batch --no-secmem-warning --status-fd 1 --verify \"" + signatureFile.Path + "\" -";
            //    result = Execute(arguments, data, ErrorHandler);
            //}
        }
        #endregion

        #region Error handler
        /// <summary>
        /// Provides error handling for GnuPG stderr.
        /// </summary>
        /// <param name="line">The error line written to stderr.</param>
        /// <returns>Always <see langword="null"/>.</returns>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
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
