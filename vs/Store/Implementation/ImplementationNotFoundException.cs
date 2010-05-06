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

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Indicates an <see cref="Implementation"/> could not be found in a <see cref="IStore"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "This exception type has a specific signaling purpose and doesn't need to carry extra info like Messages")]
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "This exception type has a specific signaling purpose and doesn't need to be serializable")]
    public class ImplementationNotFoundException : Exception
    {
        #region Properties
        /// <summary>
        /// The <see cref="ManifestDigest"/> of the <see cref="Implementation"/> to be found.
        /// </summary>
        public ManifestDigest ManifestDigest { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new implementation not found exception.
        /// </summary>
        /// <param name="manifestDigest">The <see cref="ManifestDigest"/> of the <see cref="Implementation"/> to be found.</param>
        public ImplementationNotFoundException(ManifestDigest manifestDigest)
        {
            ManifestDigest = manifestDigest;
        }
        #endregion
    }
}
