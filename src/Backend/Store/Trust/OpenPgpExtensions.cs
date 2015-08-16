/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Provides extension methods for <see cref="IOpenPgp"/> implementations.
    /// </summary>
    public static class OpenPgpExtensions
    {
        /// <summary>
        /// Returns a specific secret key in the keyring.
        /// </summary>
        /// <param name="openPgp">The <see cref="IOpenPgp"/> implementation.</param>
        /// <param name="keyIDContainer">An object containing the key ID that identifies the keypair.</param>
        /// <exception cref="KeyNotFoundException">The specified key could not be found on the system.</exception>
        /// <seealso cref="IOpenPgp.Sign"/>
        /// <seealso cref="IOpenPgp.ExportKey"/>
        [NotNull]
        public static OpenPgpSecretKey GetSecretKey([NotNull] this IOpenPgp openPgp, [NotNull] IKeyIDContainer keyIDContainer)
        {
            #region Sanity checks
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            if (keyIDContainer == null) throw new ArgumentNullException("keyIDContainer");
            #endregion

            var secretKeys = openPgp.ListSecretKeys().ToList();
            if (secretKeys.Count == 0)
                throw new KeyNotFoundException(Resources.UnableToFindSecretKey);

            try
            {
                return secretKeys.First(x => x.KeyID == keyIDContainer.KeyID);
            }
            catch (InvalidOperationException)
            {
                throw new KeyNotFoundException(Resources.UnableToFindSecretKey);
            }
        }

        /// <summary>
        /// Returns a specific secret key in the keyring.
        /// </summary>
        /// <param name="openPgp">The <see cref="IOpenPgp"/> implementation.</param>
        /// <param name="keySpecifier">The key ID, fingerprint or any part of a user ID that identifies the keypair; <see langword="null"/> to use the default key.</param>
        /// <exception cref="KeyNotFoundException">The specified key could not be found on the system.</exception>
        /// <seealso cref="IOpenPgp.Sign"/>
        /// <seealso cref="IOpenPgp.ExportKey"/>
        [NotNull]
        public static OpenPgpSecretKey GetSecretKey([NotNull] this IOpenPgp openPgp, [CanBeNull] string keySpecifier = null)
        {
            #region Sanity checks
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            #endregion

            var secretKeys = openPgp.ListSecretKeys().ToList();
            if (secretKeys.Count == 0)
                throw new KeyNotFoundException(Resources.UnableToFindSecretKey);

            if (string.IsNullOrEmpty(keySpecifier))
                return secretKeys[0];

            try
            {
                var keyID = OpenPgpUtils.ParseKeyID(keySpecifier);
                return secretKeys.First(x => x.KeyID == keyID);
            }
            catch (FormatException)
            {}
            catch (InvalidOperationException)
            {}

            try
            {
                var fingerprint = OpenPgpUtils.ParseFingerpint(keySpecifier);
                return secretKeys.First(x => x.GetFingerprint().SequenceEqual(fingerprint));
            }
            catch (FormatException)
            {}
            catch (InvalidOperationException)
            {}

            try
            {
                return secretKeys.First(x => x.UserID.ContainsIgnoreCase(keySpecifier));
            }
            catch
            {
                throw new KeyNotFoundException(Resources.UnableToFindSecretKey);
            }
        }
    }
}
