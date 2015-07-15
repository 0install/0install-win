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

using System;
using JetBrains.Annotations;
using NanoByte.Common.Collections;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Represents a secret key stored in a local <see cref="IOpenPgp"/> profile.
    /// </summary>
    /// <seealso cref="IOpenPgp.ListSecretKeys"/>
    public sealed class OpenPgpSecretKey : IEquatable<OpenPgpSecretKey>
    {
        /// <summary>
        /// A short identifier for the key.
        /// </summary>
        [NotNull]
        public string KeyID { get; private set; }

        /// <summary>
        /// A short identifier for the key.
        /// </summary>
        internal readonly long KeyIDParsed;

        /// <summary>
        /// A long identifier for the key.
        /// </summary>
        [NotNull]
        internal readonly byte[] Fingerprint;

        /// <summary>
        /// The user's name, e-mail address, etc. of the key owner.
        /// </summary>
        [NotNull]
        public string UserID { get; private set; }

        /// <summary>
        /// Creates a new <see cref="IOpenPgp"/> secret key representation.
        /// </summary>
        /// <param name="keyID">A short identifier for the key.</param>
        /// <param name="fingerprint">A long identifier for the key.</param>
        /// <param name="userID">The user's name, e-mail address, etc. of the key owner.</param>
        public OpenPgpSecretKey(long keyID, [NotNull] byte[] fingerprint, [NotNull] string userID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException("userID");
            if (fingerprint == null) throw new ArgumentNullException("fingerprint");
            #endregion

            KeyIDParsed = keyID;
            KeyID = OpenPgpUtils.FormatKeyID(keyID);
            Fingerprint = fingerprint;
            UserID = userID;
        }

        /// <summary>
        /// Creates a new <see cref="IOpenPgp"/> secret key representation.
        /// </summary>
        /// <param name="keyID">A short identifier for the key.</param>
        /// <param name="fingerprint">A long identifier for the key.</param>
        /// <param name="userID">The user's name, e-mail address, etc. of the key owner.</param>
        /// <exception cref="FormatException">The string format of <paramref name="keyID"/> or <paramref name="fingerprint"/> is not valid.</exception>
        public OpenPgpSecretKey([NotNull] string keyID, [NotNull] string fingerprint, [NotNull] string userID)
        {
            KeyIDParsed = OpenPgpUtils.ParseKeyID(keyID);
            KeyID = keyID;
            Fingerprint = OpenPgpUtils.ParseFingerpint(fingerprint);
            UserID = userID;
        }

        #region Conversion
        /// <summary>
        /// Returns the secret key in the form "UserID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return UserID;
        }
        #endregion

        #region Equality
        public bool Equals(OpenPgpSecretKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return KeyIDParsed == other.KeyIDParsed && Fingerprint.SequencedEquals(other.Fingerprint) && UserID == other.UserID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is OpenPgpSecretKey && Equals((OpenPgpSecretKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = KeyIDParsed.GetHashCode();
                hashCode = (hashCode * 397) ^ Fingerprint.GetSequencedHashCode();
                hashCode = (hashCode * 397) ^ UserID.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(OpenPgpSecretKey left, OpenPgpSecretKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(OpenPgpSecretKey left, OpenPgpSecretKey right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}
