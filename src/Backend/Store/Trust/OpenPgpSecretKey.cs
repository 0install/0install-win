/*
 * Copyright 2010-2016 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common.Collections;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Represents a secret key stored in a local <see cref="IOpenPgp"/> profile.
    /// </summary>
    /// <seealso cref="IOpenPgp.ListSecretKeys"/>
    public sealed class OpenPgpSecretKey : IFingerprintContainer, IEquatable<OpenPgpSecretKey>
    {
        /// <inheritdoc/>
        public long KeyID { get; private set; }

        private readonly byte[] _fingerprint;

        /// <inheritdoc/>
        public byte[] GetFingerprint()
        {
            return _fingerprint.ToArray();
        }

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
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException(nameof(userID));
            if (fingerprint == null) throw new ArgumentNullException(nameof(fingerprint));
            #endregion

            KeyID = keyID;
            _fingerprint = fingerprint;
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
            return KeyID == other.KeyID && _fingerprint.SequencedEquals(other._fingerprint) && UserID == other.UserID;
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
                var hashCode = KeyID.GetHashCode();
                hashCode = (hashCode * 397) ^ _fingerprint.GetSequencedHashCode();
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
