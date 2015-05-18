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
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace ZeroInstall.Store.Trust
{

    #region Enumerations
    /// <seealso cref="OpenPgpSecretKey.Algorithm"/>
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum OpenPgpAlgorithm
    {
        ///<summary>The algorithm used is unknown.</summary>
        Unknown = 0,

        /// <summary>RSA crypto system</summary>
        Rsa = 1,

        /// <summary>Elgamal crypto system</summary>
        Elgamal = 16,

        /// <summary>DAS crypto system</summary>
        Dsa = 17
    }
    #endregion

    /// <summary>
    /// Represents a secret key stored in a local <see cref="IOpenPgp"/> profile.
    /// </summary>
    /// <seealso cref="IOpenPgp.GetSecretKey"/>
    /// <seealso cref="IOpenPgp.ListSecretKeys"/>
    public sealed class OpenPgpSecretKey : IEquatable<OpenPgpSecretKey>
    {
        /// <summary>
        /// A unique identifier string for the key.
        /// </summary>
        [NotNull]
        public string Fingerprint { get; private set; }

        /// <summary>
        /// A short identifier string for the key.
        /// </summary>
        [NotNull]
        public string KeyID { get; private set; }

        /// <summary>
        /// The user's name, e-mail address, etc.
        /// </summary>
        [NotNull]
        public string UserID { get; private set; }

        /// <summary>
        /// The point in time when the key was created in UTC.
        /// </summary>
        public DateTime CreationTime { get; private set; }

        /// <summary>
        /// The encryption algorithm used.
        /// </summary>
        public OpenPgpAlgorithm Algorithm { get; private set; }

        /// <summary>
        /// The length of key in bits.
        /// </summary>
        public int BitLength { get; private set; }

        /// <summary>
        /// Creates a new <see cref="IOpenPgp"/> secret key representation.
        /// </summary>
        /// <param name="fingerprint"> A unique identifier string for the key.</param>
        /// <param name="keyID">A short identifier string for the key.</param>
        /// <param name="userID">The user's name, e-mail address, etc.</param>
        /// <param name="creationTime">The point in time when the key was created in UTC.</param>
        /// <param name="algorithm">The encryption algorithm used.</param>
        /// <param name="bitLength">The length of key in bits.</param>
        public OpenPgpSecretKey([NotNull] string fingerprint, [NotNull] string keyID, [NotNull] string userID, DateTime creationTime, OpenPgpAlgorithm algorithm, int bitLength)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(fingerprint)) throw new ArgumentNullException("fingerprint");
            if (string.IsNullOrEmpty(keyID)) throw new ArgumentNullException("keyID");
            if (string.IsNullOrEmpty(userID)) throw new ArgumentNullException("userID");
            #endregion

            Fingerprint = fingerprint;
            KeyID = keyID;
            UserID = userID;
            CreationTime = creationTime;
            Algorithm = algorithm;
            BitLength = bitLength;
        }

        #region Conversion
        /// <inheritdoc/>
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
            return Fingerprint == other.Fingerprint && KeyID == other.KeyID && UserID == other.UserID && CreationTime == other.CreationTime && Algorithm == other.Algorithm && BitLength == other.BitLength;
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
                var hashCode = Fingerprint.GetHashCode();
                hashCode = (hashCode * 397) ^ KeyID.GetHashCode();
                hashCode = (hashCode * 397) ^ UserID.GetHashCode();
                hashCode = (hashCode * 397) ^ CreationTime.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Algorithm;
                hashCode = (hashCode * 397) ^ BitLength;
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
