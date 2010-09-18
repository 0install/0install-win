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
using System.Security;
using Common;

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
            string result = Execute("--batch --list-secret-keys", null, null);

            // ToDo: Parse properly
            return result.Split(new[] {Environment.NewLine + Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Creates a detached signature for a specific file using the user's default key.
        /// </summary>
        /// <param name="path">The file to create the signature for.</param>
        /// <param name="passphrase">The passphrase to use to unlock the user's default key.</param>
        /// <exception cref="IOException">Thrown if the GnuPG could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        public void DetachSign(string path, SecureString passphrase)
        {
            Execute("--batch --passphrase-fd 0 --detach-sign " + path, passphrase.ToString(), null);
        }
    }
}
