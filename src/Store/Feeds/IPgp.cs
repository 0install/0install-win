/*
 * Copyright 2011 Simon E. Silva Lauinger
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

namespace ZeroInstall.Store.Feeds
{
    public interface IPgp
    {
        #region Keys
        /// <summary>
        /// Returns a specific public key.
        /// </summary>
        /// <param name="name">The name of the user or the ID of the private key to use for signing the file; <see langword="null"/> for default key.</param>
        /// <returns>The public key in the ASCII Armored format.</returns>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        public string GetPublicKey(string name);

        /// <summary>
        /// Returns information about a specific secret key.
        /// </summary>
        /// <param name="name">The name of the user or the ID of the private key to get information about; <see langword="null"/> for default key.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the couldn't be found on the system.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        public GnuPGSecretKey GetSecretKey(string name);

        /// <summary>
        /// Returns a list of information about available secret keys.
        /// </summary>
        /// <exception cref="UnhandledErrorsException">Thrown if GnuPG reported a problem.</exception>
        public GnuPGSecretKey[] ListSecretKeys();
        #endregion

    }
}
