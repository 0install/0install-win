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
using Common;
using Common.Helpers;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Feed
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

        /// <summary>
        /// Returns a list of all secret keys available to the current user.
        /// </summary>
        /// <exception cref="IOException">Thrown if the GnuPG could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        /// <exception cref="FormatException">Thrown if GnuPG's output cannot be properly parsed.</exception>
        public IEnumerable<GnuPGSecretKey> ListSecretKeys()
        {
            string result = Execute("--batch --no-secmem-warning --list-secret-keys --with-colons", null, ErrorHandler);
            string[] lines = StringHelper.SplitMultilineText(result);
            var keys = new List<GnuPGSecretKey>(lines.Length / 2);

            foreach (var line in lines)
                if (line.StartsWith("sec")) keys.Add(GnuPGSecretKey.Parse(line));

            return keys;
        }

        /// <summary>
        /// Creates a detached signature for a specific file using the user's default key.
        /// </summary>
        /// <param name="path">The file to create the signature for.</param>
        /// <param name="user">The GnuPG ID of the user whose signture to use for signing the file.</param>
        /// <param name="passphrase">The passphrase to use to unlock the user's default key.</param>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        /// <exception cref="IOException">Thrown if the GnuPG could not be launched.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file to be signed could not be found.</exception>
        public void DetachSign(string path, string user, string passphrase)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(user)) throw new ArgumentNullException("user");
            if (string.IsNullOrEmpty(passphrase)) throw new ArgumentNullException("passphrase");
            if (!File.Exists(path)) throw new FileNotFoundException(Resources.FileToSignNotFound, path);
            #endregion

            Execute("--batch --no-secmem-warning --passphrase-fd 0 --local-user " + user + " --detach-sign \"" + path + "\"", passphrase, ErrorHandler);
        }

        /// <summary>
        /// Returns the public key of a specific user
        /// </summary>
        /// <param name="user">The GnuPG ID of the user whose public key to return.</param>
        /// <returns>The public key in the ASCII Armored format.</returns>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        /// <exception cref="IOException">Thrown if the GnuPG could not be launched.</exception>
        public string GetPublicKey(string user)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(user)) throw new ArgumentNullException("user");
            #endregion

            return Execute("--batch --no-secmem-warning --local-user " + user + " --armor --export", null, ErrorHandler);
        }

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
            if (line.StartsWith("gpg: signing failed: file exists")) throw new IOException(Resources.SignatureExistsException);
            throw new UnhandledErrorsException(line);
        }
    }
}
