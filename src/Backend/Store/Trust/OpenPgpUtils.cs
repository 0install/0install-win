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
using JetBrains.Annotations;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Helper methods for <see cref="OpenPgpSecretKey"/>s and <see cref="OpenPgpSignature"/>s.
    /// </summary>
    internal static class OpenPgpUtils
    {
        /// <summary>
        /// Parses a canonical string formatting of a key ID.
        /// </summary>
        /// <exception cref="FormatException">The string format is not valid.</exception>
        public static long ParseKeyID([NotNull] string keyID)
        {
            #region Sanity checks
            if (keyID == null) throw new ArgumentNullException("keyID");
            #endregion

            if (keyID.Length != 16) throw new FormatException("OpenPGP key ID string representation must be 16 characters long.");

            return Convert.ToInt64(keyID, 16);
        }

        /// <summary>
        /// Formats a key ID as a canonical string.
        /// </summary>
        public static string FormatKeyID(long id)
        {
            return id.ToString("x16").ToUpperInvariant();
        }

        /// <summary>
        /// Parses a canonical string formatting of a key fingerprint.
        /// </summary>
        /// <exception cref="FormatException">The string format is not valid.</exception>
        [NotNull]
        public static byte[] ParseFingerpint([NotNull] string fingerprint)
        {
            #region Sanity checks
            if (fingerprint == null) throw new ArgumentNullException("fingerprint");
            #endregion

            var result = new byte[fingerprint.Length / 2];
            for (int i = 0; i < result.Length; i++)
                result[i] = Convert.ToByte(fingerprint.Substring(i * 2, 2), 16);
            return result;
        }

        /// <summary>
        /// Formats a key fingerprint as a canonical string.
        /// </summary>
        public static string FormatFingerprint([NotNull] byte[] fingerprint)
        {
            #region Sanity checks
            if (fingerprint == null) throw new ArgumentNullException("fingerprint");
            #endregion

            return BitConverter.ToString(fingerprint).Replace("-", "");
        }
    }
}
