/*
 * Copyright 2010-2011 Bastian Eicher, Simon E. Silva Lauinger
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

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Represents a signature checked by an <see cref="IOpenPgp"/> implementation.
    /// </summary>
    public abstract class OpenPgpSignature
    {
        // signature codes
        private const string ValidSignature = "VALIDSIG";
        private const string BadSignature = "BADSIG";
        private const string ErrorSignature = "ERRSIG";

        private const int VerifyCodeIndex = 1;
        private const int FingerprintIndex = 2;
        private const int TimestampIndex = 4;
        private const int KeyIDIndex = 2;
        private const int ErrorCodeIndex = 7;

        internal static OpenPgpSignature Parse(string line)
        {
            string[] signatureParts = line.Split(' ');
            switch(signatureParts[VerifyCodeIndex])
            {
                case ValidSignature:
                    return new ValidSignature(signatureParts[FingerprintIndex], new DateTime(int.Parse(signatureParts[TimestampIndex])));
                case BadSignature: return new BadSignature(signatureParts[KeyIDIndex]);
                case ErrorSignature:
                    return GetErrorSignature(int.Parse(signatureParts[ErrorCodeIndex]), signatureParts);
                default:
                    return null;
            }
        }

        private static OpenPgpSignature GetErrorSignature(int errorCode, string[] signatureParts)
        {
            switch(errorCode)
            {
                case 9: return new MissingKeySignature(signatureParts[KeyIDIndex]);
                default: return new ErrorSignature(signatureParts[KeyIDIndex], errorCode);
            }
        }
    }

    /// <summary>
    /// Represents a valid signature.
    /// </summary>
    public sealed class ValidSignature : OpenPgpSignature
    {
        /// <summary>
        /// A unique identifier string for the key used to create this signature.
        /// </summary>
        public readonly string Fingerprint;

        /// <summary>
        /// The point in time when the signature was created in UTC.
        /// </summary>
        public readonly DateTime Timestamp;

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
    }

    /// <summary>
    /// Represents a bad signature (i.e., the message has been tampered with).
    /// </summary>
    public sealed class BadSignature : OpenPgpSignature
    {
        /// <summary>
        /// A short identifier string for the key used to create this signature.
        /// </summary>
        public readonly string KeyID;

        /// <summary>
        /// Creates a new bad signature.
        /// </summary>
        /// <param name="keyID">A short identifier string for the key used to create this signature.</param>
        public BadSignature(string keyID)
        {
            KeyID = keyID;
        }
    }

    /// <summary>
    /// Represents a signature which's key is missing.
    /// </summary>
    public sealed class MissingKeySignature : OpenPgpSignature
    {
        /// <summary>
        /// A short identifier string for the key used to create this signature.
        /// </summary>
        public readonly string KeyID;

        /// <summary>
        /// Creates a new missing key error.
        /// </summary>
        /// <param name="keyID">A short identifier string for the key used to create this signature.</param>
        public MissingKeySignature(string keyID)
        {
            KeyID = keyID;
        }
    }

    /// <summary>
    /// Represents a signature that could not be validated for some reason.
    /// </summary>
    public sealed class ErrorSignature : OpenPgpSignature
    {
        /// <summary>
        /// A short identifier string for the key used to create this signature.
        /// </summary>
        public readonly string KeyID;

        /// <summary>
        /// The code that refers to a description of the error.
        /// </summary>
        private readonly int ErrorCode;

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
    }
}
