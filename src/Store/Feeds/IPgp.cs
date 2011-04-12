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

using System.Collections.Generic;
using System.IO;
using Common.Cli;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Provides access to an encryption/signature system compatible with the PGP standard.
    /// </summary>
    /// <remarks>This is an application of the strategy pattern. Implementations of this interface are immutable.</remarks>
    public interface IPgp
    {
        /// <summary>
        /// Returns a specific public key.
        /// </summary>
        /// <param name="name">The name of the user or the ID of the private key to use for signing the file; <see langword="null"/> for default key.</param>
        /// <returns>The public key in the ASCII Armored format.</returns>
        /// <exception cref="IOException">Thrown if the PGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the PGP implementation reported a problem.</exception>
        string GetPublicKey(string name);

        /// <summary>
        /// Returns information about a specific secret key.
        /// </summary>
        /// <param name="name">The name of the user or the ID of the private key to get information about; <see langword="null"/> for default key.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the couldn't be found on the system.</exception>
        /// <exception cref="IOException">Thrown if the PGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the PGP implementation reported a problem.</exception>
        PgpSecretKey GetSecretKey(string name);

        /// <summary>
        /// Returns a list of information about available secret keys.
        /// </summary>
        /// <exception cref="UnhandledErrorsException">Thrown if the PGP implementation reported a problem.</exception>
        /// <exception cref="IOException">Thrown if the PGP implementation could not be launched.</exception>
        PgpSecretKey[] ListSecretKeys();

        /// <summary>
        /// Creates a detached signature for a specific file using the user's default key.
        /// </summary>
        /// <param name="path">The file to create the signature for.</param>
        /// <param name="name">The name of the user or the ID of the private key to use for signing the file; <see langword="null"/> for default key.</param>
        /// <param name="passphrase">The passphrase to use to unlock the user's default key.</param>
        /// <exception cref="FileNotFoundException">Thrown if the file to be signed could not be found.</exception>
        /// <exception cref="IOException">Thrown if the PGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the PGP implementation reported a problem.</exception>
        void DetachSign(string path, string name, string passphrase);

        /// <summary>
        /// Checks whether data are correctly signed.
        /// </summary>
        /// <param name="data">The data to validate.</param>
        /// <param name="signature">The signature for <paramref name="data"/>.</param>
        /// <exception cref="IOException">Thrown if the PGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the PGP implementation reported a problem.</exception>
        void Verify(string data, string signature);
    }
}
