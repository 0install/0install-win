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
        /// Returns a specific public key.
        /// </summary>
        /// <param name="name">The name of the user or the ID of the private key to use for signing the file; <see langword="null"/> for default key.</param>
        /// <returns>The public key in the ASCII Armored format.</returns>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        string GetPublicKey(string name);

        /// <summary>
        /// Returns information about a specific secret key.
        /// </summary>
        /// <param name="name">The name of the user or the ID of the private key to get information about; <see langword="null"/> for default key.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the specified key could not be found on the system.</exception>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        OpenPgpSecretKey GetSecretKey(string name);

        /// <summary>
        /// Returns a list of information about available secret keys.
        /// </summary>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        OpenPgpSecretKey[] ListSecretKeys();

        /// <summary>
        /// Checks the passphrase of the given user for correctness.
        /// </summary>
        /// <param name="name">The name of the user or the ID of the private key. <see langword="null"/> for default key.</param>
        /// <param name="passphrase">The passphrase to check for correctness.</param>
        /// <returns><see langword="true"/> if the passphrase is correct, else <see langword="false"/>.</returns>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        bool IsPassphraseCorrect(string name, string passphrase);

        /// <summary>
        /// Creates a detached signature for a specific file using the user's default key.
        /// </summary>
        /// <param name="path">The file to create the signature for.</param>
        /// <param name="name">The name of the user or the ID of the private key to use for signing the file; <see langword="null"/> for default key.</param>
        /// <param name="passphrase">The passphrase to use to unlock the user's default key.</param>
        /// <exception cref="FileNotFoundException">Thrown if the file to be signed could not be found.</exception>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched.</exception>
        /// <exception cref="WrongPassphraseException">Thrown if passphrase was incorrect.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        void DetachSign(string path, string name, string passphrase);

        /// <summary>
        /// Validates data signed by one or more keys.
        /// </summary>
        /// <param name="data">The data that is signed.</param>
        /// <param name="signature">The signature data.</param>
        /// <returns>A list of signatures found, both valid and invalid.</returns>
        /// <exception cref="IOException">Thrown if the OpenPGP implementation could not be launched.</exception>
        /// <exception cref="UnhandledErrorsException">Thrown if the OpenPGP implementation reported a problem.</exception>
        OpenPgpSignature[] Verify(Stream data, string signature);
    }
}
