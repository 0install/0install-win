/*
 * Copyright 2010-2014 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Diagnostics;
using System.IO;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Provides access to an encryption/signature system compatible with the OpenPGP standard.
    /// </summary>
    /// <remarks>Implementations of this interface are immutable and thread-safe.</remarks>
    public interface IOpenPgp
    {
        /// <summary>
        /// Imports a key into the keyring.
        /// </summary>
        /// <param name="data">The key data to be imported.</param>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched.</exception>
        void ImportKey(byte[] data);

        /// <summary>
        /// Returns information about the secret key for specific keypair.
        /// </summary>
        /// <param name="keySpecifier">The key ID, fingerprint or any part of a user ID that identifies the keypair; <see langword="null"/> to use the default key.</param>
        /// <exception cref="KeyNotFoundException">The specified key could not be found on the system.</exception>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched.</exception>
        OpenPgpSecretKey GetSecretKey(string keySpecifier);

        /// <summary>
        /// Returns a list of information about available secret keys.
        /// </summary>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched.</exception>
        OpenPgpSecretKey[] ListSecretKeys();

        /// <summary>
        /// Returns the public key for specific keypair.
        /// </summary>
        /// <param name="keySpecifier">The key ID, fingerprint or any part of a user ID that identifies the keypair.</param>
        /// <returns>The public key in ASCII Armored format.</returns>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched.</exception>
        string GetPublicKey(string keySpecifier);

        /// <summary>
        /// Launches an interactive process for generating a new keypair.
        /// </summary>
        /// <returns>A handle that can be used to wait for the process to finish.</returns>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched.</exception>
        Process GenerateKey();

        /// <summary>
        /// Creates a detached signature for a stream using a specific secret key.
        /// </summary>
        /// <param name="stream">The data to sign.</param>
        /// <param name="keySpecifier">The key ID, fingerprint or any part of a user ID that identifies the keypair.</param>
        /// <param name="passphrase">The passphrase to use to unlock the secret key.</param>
        /// <returns>The signature as a Base64 encoded string.</returns>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched.</exception>
        /// <exception cref="WrongPassphraseException">Passphrase was incorrect.</exception>
        string DetachSign(Stream stream, string keySpecifier, string passphrase);

        /// <summary>
        /// Validates data signed by one or more keys.
        /// </summary>
        /// <param name="data">The data that is signed as a byte array.</param>
        /// <param name="signature">The signature data as a byte array.</param>
        /// <returns>A list of signatures found, both valid and invalid.</returns>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched.</exception>
        IEnumerable<OpenPgpSignature> Verify(byte[] data, byte[] signature);
    }
}
