/*
 * Copyright 2010 Bastian Eicher
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
using System.IO;
using Common;
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
        protected override string AppBinaryName { get { return "gpg"; } }
        #endregion

        //--------------------//

        /// <summary>
        /// Returns a list of all secret keys available to the current user.
        /// </summary>
        /// <exception cref="IOException">Thrown if the GnuPG could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        public string[] ListSecretKeys()
        {
            string result = Execute("--batch --list-secret-keys", null, ErrorHandler);

            // ToDo: Parse properly
            return result.Split(new[] {Environment.NewLine + Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Creates a detached signature for a specific file using the user's default key.
        /// </summary>
        /// <param name="path">The file to create the signature for.</param>
        /// <param name="user">The GnuPG ID of the user whose signture to use for signing the file.</param>
        /// <param name="passphrase">The passphrase to use to unlock the user's default key.</param>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        /// <exception cref="IOException">Thrown if the GnuPG could not be launched.</exception>
        /// <exception cref="FileNotFoundException"></exception>
        public void DetachSign(string path, string user, string passphrase)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(user)) throw new ArgumentNullException("user");
            if (string.IsNullOrEmpty(passphrase)) throw new ArgumentNullException("passphrase");
            if (!File.Exists(path)) throw new FileNotFoundException(Resources.FileToSignNotFound, path);
            #endregion

            Execute("--batch --passphrase-fd 0 --local-user" + user + "--detach-sign " + path, passphrase, ErrorHandler);
        }

        /// <summary>
        /// Provides error handling for GnuPG stderr.
        /// </summary>
        /// <param name="line">The error line written to stderr.</param>
        /// <returns>Always <see langword="null"/>.</returns>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        private static string ErrorHandler(string line)
        {
            if (line.StartsWith("gpg: NOTE:")) Log.Info(line);
            else throw new UnhandledErrorsException(line);
            return null;
        }
    }
}
