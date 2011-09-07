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
    /// Provides access to an encryption/signature system compatible with the OpenPGP standard.
    /// </summary>
    /// <remarks>This is an application of the strategy pattern. Implementations of this interface are immutable.</remarks>
    public interface IOpenPgp
    {
        /// <summary>
        /// Imports a key into the keyring.
        /// </summary>
        /// <param name="stream">The key data to be imported.</param>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        void ImportKey(Stream stream);

        /// <summary>
        /// Returns the public key for specific keypair.
        /// </summary>
        /// <param name="keySpecifier">The key ID, fingerprint or any part of a user ID that identifies the keypair; <see langword="null"/> to use the default key.</param>
        /// <returns>The public key in the ASCII Armored format.</returns>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        string GetPublicKey(string keySpecifier);

        /// <summary>
        /// Returns information about the secret key for specific keypair.
        /// </summary>
        /// <param name="keySpecifier">The key ID, fingerprint or any part of a user ID that identifies the keypair; <see langword="null"/> to use the default key.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the specified key could not be found on the system.</exception>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        OpenPgpSecretKey GetSecretKey(string keySpecifier);

        /// <summary>
        /// Returns a list of information about available secret keys.
        /// </summary>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        OpenPgpSecretKey[] ListSecretKeys();

        /// <summary>
        /// Checks whether a passphrase is valid for a specific secret key.
        /// </summary>
        /// <param name="keySpecifier">The key ID, fingerprint or any part of a user ID that identifies the keypair; <see langword="null"/> to use the default key.</param>
        /// <param name="passphrase">The passphrase to check for correctness.</param>
        /// <returns><see langword="true"/> if the passphrase is correct, else <see langword="false"/>.</returns>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        bool IsPassphraseCorrect(string keySpecifier, string passphrase);

        /// <summary>
        /// Creates a detached signature for a file using a specific secret key.
        /// </summary>
        /// <param name="path">The file to sign.</param>
        /// <param name="keySpecifier">The key ID, fingerprint or any part of a user ID that identifies the keypair; <see langword="null"/> to use the default key.</param>
        /// <param name="passphrase">The passphrase to use to unlock the secret key.</param>
        /// <exception cref="FileNotFoundException">Thrown if the file to be signed could not be found.</exception>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched.</exception>
        /// <exception cref="WrongPassphraseException">Thrown if passphrase was incorrect.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        void DetachSign(string path, string keySpecifier, string passphrase);

        /// <summary>
        /// Validates data signed by one or more keys.
        /// </summary>
        /// <param name="data">The data that is signed.</param>
        /// <param name="signature">The signature data.</param>
        /// <returns>A list of signatures found, both valid and invalid.</returns>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        OpenPgpSignature[] Verify(Stream data, byte[] signature);
    }
}
