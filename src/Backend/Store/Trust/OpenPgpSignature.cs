/*
 * Copyright 2010-2014 Bastian Eicher, Simon E. Silva Lauinger
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
using NanoByte.Common.Storage;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// Represents a signature checked by an <see cref="IOpenPgp"/> implementation.
    /// </summary>
    public abstract class OpenPgpSignature
    {
        #region Factory methods
        /// <summary>
        /// Parses information about a signature checked by an <see cref="IOpenPgp"/> implementation from a console line.
        /// </summary>
        /// <param name="line">The console line containing the signature information.</param>
        /// <returns>The parsed signature representation; <see langword="null"/> if <paramref name="line"/> did not contain any signature information.</returns>
        /// <exception cref="FormatException"><paramref name="line"/> contains incorrectly formatted signature information.</exception>
        [CanBeNull]
        internal static OpenPgpSignature Parse([NotNull] string line)
        {
            #region Sanity checks
            if (line == null) throw new ArgumentNullException("line");
            #endregion

            const int signatureTypeIndex = 1, fingerprintIndex = 2, timestampIndex = 4, keyIDIndex = 2, errorCodeIndex = 7;

            string[] signatureParts = line.Split(' ');
            if (signatureParts.Length < signatureTypeIndex + 1) return null;
            switch (signatureParts[signatureTypeIndex])
            {
                case "VALIDSIG":
                    if (signatureParts.Length != 12) throw new FormatException("Incorrect number of columns in VALIDSIG line.");
                    return new ValidSignature(signatureParts[fingerprintIndex], FileUtils.FromUnixTime(long.Parse(signatureParts[timestampIndex])));

                case "BADSIG":
                    if (signatureParts.Length < 3) throw new FormatException("Incorrect number of columns in BADSIG line.");
                    return new BadSignature(signatureParts[keyIDIndex]);

                case "ERRSIG":
                    if (signatureParts.Length != 8) throw new FormatException("Incorrect number of columns in ERRSIG line.");
                    int errorCode = int.Parse(signatureParts[errorCodeIndex]);
                    switch (errorCode)
                    {
                        case 9:
                            return new MissingKeySignature(signatureParts[keyIDIndex]);
                        default:
                            return new ErrorSignature(signatureParts[keyIDIndex], errorCode);
                    }

                default:
                    return null;
            }
        }
        #endregion
    }

    /// <summary>
    /// Represents a valid signature.
    /// </summary>
    public sealed class ValidSignature : OpenPgpSignature
    {
        #region Variables
        /// <summary>
        /// A unique identifier string for the key used to create this signature.
        /// </summary>
        public readonly string Fingerprint;

        /// <summary>
        /// The point in time when the signature was created in UTC.
        /// </summary>
        public readonly DateTime Timestamp;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new valid signature.
        /// </summary>
        /// <param name="fingerprint">A short identifier string for the key used to create this signature.</param>
        /// <param name="timestamp">The point in time when the signature was created in UTC.</param>
        public ValidSignature(string fingerprint, DateTime timestamp)
        {
            Fingerprint = fingerprint;
            Timestamp = timestamp;
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the archive in the form "ValidSignature: Fingerprint (Timestamp)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("ValidSignature: {0} ({1})", Fingerprint, Timestamp);
        }
        #endregion
    }

    /// <summary>
    /// Represents a bad signature (i.e., the message has been tampered with).
    /// </summary>
    public sealed class BadSignature : OpenPgpSignature
    {
        #region Variables
        /// <summary>
        /// A short identifier string for the key used to create this signature.
        /// </summary>
        public readonly string KeyID;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new bad signature.
        /// </summary>
        /// <param name="keyID">A short identifier string for the key used to create this signature.</param>
        public BadSignature(string keyID)
        {
            KeyID = keyID;
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the archive in the form "BadSignature: KeyID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("BadSignature: {0}", KeyID);
        }
        #endregion
    }

    /// <summary>
    /// Represents a signature which's key is missing.
    /// </summary>
    public sealed class MissingKeySignature : OpenPgpSignature
    {
        #region Variables
        /// <summary>
        /// A short identifier string for the key used to create this signature.
        /// </summary>
        public readonly string KeyID;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new missing key error.
        /// </summary>
        /// <param name="keyID">A short identifier string for the key used to create this signature.</param>
        public MissingKeySignature(string keyID)
        {
            KeyID = keyID;
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the archive in the form "MissingKeySignature: KeyID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("MissingKeySignature: {0}", KeyID);
        }
        #endregion
    }

    /// <summary>
    /// Represents a signature that could not be validated for some reason.
    /// </summary>
    public sealed class ErrorSignature : OpenPgpSignature
    {
        #region Variables
        /// <summary>
        /// A short identifier string for the key used to create this signature.
        /// </summary>
        public readonly string KeyID;

        /// <summary>
        /// The code that refers to a description of the error.
        /// </summary>
        public readonly int ErrorCode;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new signature error.
        /// </summary>
        /// <param name="keyID">A short identifier string for the key used to create this signature.</param>
        /// <param name="errorCode">The code that refers to a description of the error.</param>
        public ErrorSignature(string keyID, int errorCode)
        {
            KeyID = keyID;
            ErrorCode = errorCode;
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the archive in the form "ErrorSignature: KeyID (ErrorCode)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("MissingKeySignature: {0} ({1})", KeyID, ErrorCode);
        }
        #endregion
    }
}
