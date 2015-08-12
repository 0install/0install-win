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
    /// Represents a signature checked by an <see cref="IOpenPgp"/> implementation.
    /// </summary>
    /// <seealso cref="IOpenPgp.Verify"/>
    public abstract class OpenPgpSignature : IKeyIDContainer
    {
        /// <inheritdoc/>
        public long KeyID { get; private set; }

        /// <summary>
        /// Creates a new signature.
        /// </summary>
        /// <param name="keyID">The key ID of the key used to create this signature.</param>
        protected OpenPgpSignature(long keyID)
        {
            KeyID = keyID;
        }

        #region Equality
        protected bool Equals(OpenPgpSignature other)
        {
            #region Sanity checks
            if (other == null) throw new ArgumentNullException(nameof(other));
            #endregion

            return KeyID == other.KeyID;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((OpenPgpSignature)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return KeyID.GetHashCode();
        }
        #endregion
    }

    /// <summary>
    /// Represents a valid signature.
    /// </summary>
    /// <seealso cref="IOpenPgp.Verify"/>
    public sealed class ValidSignature : OpenPgpSignature, IFingerprintContainer
    {
        private readonly byte[] _fingerprint;

        /// <inheritdoc/>
        public byte[] GetFingerprint()
        {
            return _fingerprint.ToArray();
        }

        /// <summary>
        /// The point in time when the signature was created in UTC.
        /// </summary>
        public readonly DateTime Timestamp;

        /// <summary>
        /// Creates a new valid signature.
        /// </summary>
        /// <param name="keyID">The key ID of the key used to create this signature.</param>
        /// <param name="fingerprint">The fingerprint of the key used to create this signature.</param>
        /// <param name="timestamp">The point in time when the signature was created in UTC.</param>
        public ValidSignature(long keyID, [NotNull] byte[] fingerprint, DateTime timestamp) : base(keyID)
        {
            #region Sanity checks
            if (fingerprint == null) throw new ArgumentNullException(nameof(fingerprint));
            #endregion

            _fingerprint = fingerprint;
            Timestamp = timestamp;
        }

        #region Conversion
        /// <summary>
        /// Returns the signature information in the form "ValidSignature: Fingerprint (Timestamp)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("ValidSignature: {0} ({1})", this.FormatFingerprint(), Timestamp);
        }
        #endregion

        #region Equality
        private bool Equals(ValidSignature other)
        {
            return base.Equals(other) && _fingerprint.SequencedEquals(other._fingerprint) && Timestamp == other.Timestamp;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is ValidSignature && Equals((ValidSignature)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ _fingerprint.GetSequencedHashCode();
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                return hashCode;
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
        /// Creates a new signature error.
        /// </summary>
        /// <param name="keyID">The key ID of the key used to create this signature.</param>
        public ErrorSignature(long keyID) : base(keyID)
        {}

        #region Conversion
        /// <summary>
        /// Returns the signature information in the form "ErrorSignature: KeyID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("ErrorSignature: {0}", this.FormatKeyID());
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is ErrorSignature && Equals((ErrorSignature)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
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
        public BadSignature(long keyID) : base(keyID)
        {}

        #region Conversion
        /// <summary>
        /// Returns the signature information in the form "BadSignature: KeyID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("BadSignature: {0}", this.FormatKeyID());
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
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
    /// Use <see cref="IOpenPgp.ImportKey"/> to import the key and then retry.
    /// </summary>
    /// <seealso cref="IOpenPgp.Verify"/>
    public sealed class MissingKeySignature : ErrorSignature
    {
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
            return string.Format("MissingKeySignature: {0}", this.FormatKeyID());
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
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
