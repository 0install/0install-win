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
using System.Diagnostics.CodeAnalysis;
using ZeroInstall.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Indicates an <see cref="Implementation"/> directory does not match a <see cref="ManifestDigest"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "This exception type has a specific signaling purpose and doesn't need to carry extra info like Messages")]
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "This exception type has a specific signaling purpose and doesn't need to be serializable")]
    public class DigestMismatchException : Exception
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
        #endregion
    }
}
