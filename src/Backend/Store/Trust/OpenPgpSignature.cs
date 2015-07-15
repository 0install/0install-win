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

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Represents a signature checked by an <see cref="IOpenPgp"/> implementation.
    /// </summary>
    /// <seealso cref="IOpenPgp.Verify"/>
    public abstract class OpenPgpSignature
    {}

    /// <summary>
    /// Represents a valid signature.
    /// </summary>
    /// <seealso cref="IOpenPgp.Verify"/>
    public sealed class ValidSignature : OpenPgpSignature
    {
        /// <summary>
        /// The fingerprint of the key used to create this signature.
        /// </summary>
        /// <seealso cref="OpenPgpSecretKey.Fingerprint"/>
        [NotNull]
        public string Fingerprint { get; private set; }

        /// <summary>
        /// The point in time when the signature was created in UTC.
        /// </summary>
        public readonly DateTime Timestamp;

        /// <summary>
        /// Creates a new valid signature.
        /// </summary>
        /// <param name="fingerprint">The fingerprint of the key used to create this signature.</param>
        /// <param name="timestamp">The point in time when the signature was created in UTC.</param>
        public ValidSignature([NotNull] string fingerprint, DateTime timestamp)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(fingerprint)) throw new ArgumentNullException("fingerprint");
            #endregion

            Fingerprint = fingerprint;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Creates a new valid signature.
        /// </summary>
        /// <param name="fingerprint">The fingerprint of the key used to create this signature.</param>
        /// <param name="timestamp">The point in time when the signature was created in UTC.</param>
        public ValidSignature([NotNull] byte[] fingerprint, DateTime timestamp)
            : this(OpenPgpUtils.FormatFingerprint(fingerprint), timestamp)
        {}

        #region Conversion
        /// <summary>
        /// Returns the signature information in the form "ValidSignature: Fingerprint (Timestamp)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("ValidSignature: {0} ({1})", Fingerprint, Timestamp);
        }
        #endregion

        #region Equality
        private bool Equals(ValidSignature other)
        {
            return Timestamp.Equals(other.Timestamp) && string.Equals(Fingerprint, other.Fingerprint);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ValidSignature && Equals((ValidSignature)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Timestamp.GetHashCode() * 397) ^ Fingerprint.GetHashCode();
            }
        }
        #endregion
    }

    /// <summary>
    /// Represents a signature that could not be validated for some reason.
    /// </summary>
    /// <seealso cref="IOpenPgp.Verify"/>
    public class ErrorSignature : OpenPgpSignature
    {
        /// <summary>
        /// The key ID of the key used to create this signature.
        /// </summary>
        /// <seealso cref="OpenPgpSecretKey.KeyID"/>
        [NotNull]
        public string KeyID { get; private set; }

        /// <summary>
        /// Creates a new signature error.
        /// </summary>
        /// <param name="keyID">The key ID of the key used to create this signature.</param>
        public ErrorSignature([NotNull] string keyID)
        {
            KeyID = keyID;
        }

        /// <summary>
        /// Creates a new signature error.
        /// </summary>
        /// <param name="keyID">The key ID of the key used to create this signature.</param>
        public ErrorSignature(long keyID)
            : this(OpenPgpUtils.FormatKeyID(keyID))
        {}

        #region Conversion
        /// <summary>
        /// Returns the signature information in the form "ErrorSignature: KeyID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("ErrorSignature: {0}", KeyID);
        }
        #endregion

        #region Equality
        protected bool Equals(ErrorSignature other)
        {
            #region Sanity checks
            if (other == null) throw new ArgumentNullException("other");
            #endregion

            return string.Equals(KeyID, other.KeyID);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ErrorSignature)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return KeyID.GetHashCode();
        }
        #endregion
    }

    /// <summary>
    /// Represents a bad signature (i.e., the message has been tampered with).
    /// </summary>
    /// <seealso cref="IOpenPgp.Verify"/>
    public sealed class BadSignature : ErrorSignature
    {
        /// <summary>
        /// Creates a new bad signature.
        /// </summary>
        /// <param name="keyID">The key ID of the key used to create this signature.</param>
        public BadSignature([NotNull] string keyID) : base(keyID)
        {}

        /// <summary>
        /// Creates a new bad signature.
        /// </summary>
        /// <param name="keyID">The key ID of the key used to create this signature.</param>
        public BadSignature(long keyID) : base(keyID)
        {}

        #region Conversion
        /// <summary>
        /// Returns the signature information in the form "BadSignature: KeyID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("BadSignature: {0}", KeyID);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is BadSignature && Equals((BadSignature)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }

    /// <summary>
    /// Represents a signature that could not yet be verified because the key is missing.
    /// Use <seealso cref="IOpenPgp.ImportKey"/> to import the key and then retry.
    /// </summary>
    /// <seealso cref="IOpenPgp.Verify"/>
    public sealed class MissingKeySignature : ErrorSignature
    {
        /// <summary>
        /// Creates a new missing key error.
        /// </summary>
        /// <param name="keyID">The key ID of the key used to create this signature.</param>
        public MissingKeySignature([NotNull] string keyID) : base(keyID)
        {}

        /// <summary>
        /// Creates a new missing key error.
        /// </summary>
        /// <param name="keyID">The key ID of the key used to create this signature.</param>
        public MissingKeySignature(long keyID) : base(keyID)
        {}

        #region Conversion
        /// <summary>
        /// Returns the signature information in the form "MissingKeySignature: KeyID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("MissingKeySignature: {0}", KeyID);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MissingKeySignature && Equals((MissingKeySignature)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
