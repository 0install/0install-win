/*
 * Copyright 2010 Bastian Eicher
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
using System.Runtime.Serialization;
using System.Security.Permissions;
using ZeroInstall.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Indicates an <see cref="Implementation"/> directory does not match a <see cref="ManifestDigest"/>.
    /// </summary>
    [Serializable]
    public sealed class DigestMismatchException : Exception
    {
        #region Properties
        /// <summary>
        /// The hash value the <see cref="Implementation"/> was supposed to have.
        /// </summary>
        public string ExpectedHash { get; private set; }

        /// <summary>
        /// The hash value that was actually calculated.
        /// </summary>
        public string ActualHash { get; private set; }

        /// <summary>
        /// The content of the ".manifest" file that created <see cref="ActualHash"/>.
        /// </summary>
        public string Manifest { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new digest mismatch exception.
        /// </summary>
        /// <param name="expectedHash">The hash value the <see cref="Implementation"/> was supposed to have.</param>
        /// <param name="actualHash">The hash value that was actually calculated.</param>
        /// <param name="manifest">The content of the ".manifest" file that created <paramref name="actualHash"/>.</param>
        public DigestMismatchException(string expectedHash, string actualHash, string manifest)
            : base(string.Format(Resources.DigestMismatch, expectedHash, actualHash))
        {
            ExpectedHash = expectedHash;
            ActualHash = actualHash;
            Manifest = manifest;
        }

        public DigestMismatchException()
            : base(string.Format(Resources.DigestMismatch, "unknown", "unknown"))
        {}

        public DigestMismatchException(string message) : base(message) 
        {}

        public DigestMismatchException(string message, Exception innerException) : base (message, innerException)
        {}

        private DigestMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            #region Sanity checks
            if (info == null) throw new ArgumentNullException("info");
            #endregion

            ExpectedHash = info.GetString("ExpectedHash");
            ActualHash = info.GetString("ActualHash");
            Manifest = info.GetString("Manifest");
        }
        #endregion

        #region Serialization
        /// <inheritdoc/>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            #region Sanity checks
            if (info == null) throw new ArgumentNullException("info");
            #endregion

            info.AddValue("ExpectedHash", ExpectedHash);
            info.AddValue("ActualHash", ActualHash);
            info.AddValue("Manifest", Manifest);

            base.GetObjectData(info, context);
        }
        #endregion
    }
}
