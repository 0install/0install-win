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
    /// Helper methods for <see cref="IKeyIDContainer"/> and <see cref="IFingerprintContainer"/>.
    /// </summary>
    public static class OpenPgpUtils
    {
        /// <summary>
        /// Formats a key ID as a canonical string.
        /// </summary>
        public static string FormatKeyID([NotNull] this IKeyIDContainer keyIDContainer)
        {
            #region Sanity checks
            if (keyIDContainer == null) throw new ArgumentNullException("keyIDContainer");
            #endregion

            return keyIDContainer.KeyID.ToString("x16").ToUpperInvariant();
        }

        /// <summary>
        /// Formats a key fingerprint as a canonical string.
        /// </summary>
        public static string FormatFingerprint([NotNull] this IFingerprintContainer fingerprintContainer)
        {
            #region Sanity checks
            if (fingerprintContainer == null) throw new ArgumentNullException("fingerprintContainer");
            #endregion

            return BitConverter.ToString(fingerprintContainer.GetFingerprint()).Replace("-", "");
        }

        /// <summary>
        /// Parses a canonical string formatting of a key ID.
        /// </summary>
        /// <exception cref="FormatException">The string format is not valid.</exception>
        internal static long ParseKeyID([NotNull] string keyID)
        {
            #region Sanity checks
            if (keyID == null) throw new ArgumentNullException("keyID");
            #endregion

            if (keyID.Length != 16) throw new FormatException("OpenPGP key ID string representation must be 16 characters long.");

            return Convert.ToInt64(keyID, fromBase: 16);
        }

        /// <summary>
        /// Parses a canonical string formatting of a key fingerprint.
        /// </summary>
        /// <exception cref="FormatException">The string format is not valid.</exception>
        [NotNull]
        internal static byte[] ParseFingerpint([NotNull] string fingerprint)
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
        /// Extracts the key ID from a key fingerprint.
        /// </summary>
        internal static long FingerprintToKeyID([NotNull] byte[] fingerprint)
        {
            #region Sanity checks
            if (fingerprint == null) throw new ArgumentNullException("fingerprint");
            #endregion

            // Extract lower 64 bits and treat as Big Endian
            unchecked
            {
                int i1 = (fingerprint[fingerprint.Length - 8] << 24) | (fingerprint[fingerprint.Length - 7] << 16) | (fingerprint[fingerprint.Length - 6] << 8) | fingerprint[fingerprint.Length - 5];
                int i2 = (fingerprint[fingerprint.Length - 4] << 24) | (fingerprint[fingerprint.Length - 3] << 16) | (fingerprint[fingerprint.Length - 2] << 8) | fingerprint[fingerprint.Length - 1];
                return ((long)i1 << 32) | (uint)i2;
            }
        }
    }
}
