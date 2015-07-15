/*
 * Copyright 2010-2015 Bastian Eicher, Simon E. Silva Lauinger
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
using JetBrains.Annotations;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Provides access to an encryption/signature system compatible with the OpenPGP standard.
    /// </summary>
    /// <remarks>Implementations of this interface are thread-safe.</remarks>
    public interface IOpenPgp
    {
        /// <summary>
        /// Verifys a detached OpenPGP signature.
        /// </summary>
        /// <param name="data">The data the signature is for.</param>
        /// <param name="signature">The signature in binary format.</param>
        /// <returns>A list of signatures found, both valid and invalid. <seealso cref="MissingKeySignature"/> results indicate you need to use <seealso cref="ImportKey"/>.</returns>
        /// <exception cref="InvalidDataException"><paramref name="signature"/> does not contain valid signature data.</exception>
        /// <seealso cref="Sign"/>
        [NotNull, ItemNotNull]
        IEnumerable<OpenPgpSignature> Verify([NotNull] byte[] data, [NotNull] byte[] signature);

        /// <summary>
        /// Creates a detached OpenPGP signature using a specific secret key.
        /// </summary>
        /// <param name="data">The data to sign.</param>
        /// <param name="secretKey">The secret key to use for signing.</param>
        /// <param name="passphrase">The passphrase to use to unlock the secret key.</param>
        /// <returns>The signature in binary format).</returns>
        /// <exception cref="KeyNotFoundException">The specified <paramref name="secretKey"/> could not be found on the system.</exception>
        /// <exception cref="WrongPassphraseException"><paramref name="passphrase"/> was incorrect.</exception>
        /// <seealso cref="Verify"/>
        [NotNull]
        byte[] Sign([NotNull] byte[] data, [NotNull] OpenPgpSecretKey secretKey, [CanBeNull] string passphrase = null);

        /// <summary>
        /// Imports a public key into the keyring.
        /// </summary>
        /// <param name="data">The public key in binary or ASCII Armored format.</param>
        /// <exception cref="InvalidDataException"><paramref name="data"/> does not contain a valid public key.</exception>
        /// <seealso cref="ExportKey"/>
        void ImportKey([NotNull] byte[] data);

        /// <summary>
        /// Exports the public key for a specific secret key in the keyring.
        /// </summary>
        /// <param name="secretKey">The secret key to the get the public key for.</param>
        /// <returns>The public key in ASCII Armored format. Always uses Unix-style linebreaks.</returns>
        /// <exception cref="KeyNotFoundException">The specified <paramref name="secretKey"/> could not be found on the system.</exception>
        /// <seealso cref="ImportKey"/>
        [NotNull]
        string ExportKey([NotNull] OpenPgpSecretKey secretKey);

        /// <summary>
        /// Returns a list of secret keys in the keyring.
        /// </summary>
        /// <seealso cref="Sign"/>
        /// <seealso cref="ExportKey"/>
        [NotNull, ItemNotNull]
        IEnumerable<OpenPgpSecretKey> ListSecretKeys();
    }
}
