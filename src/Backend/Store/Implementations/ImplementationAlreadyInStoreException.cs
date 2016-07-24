/*
 * Copyright 2010-2016 Bastian Eicher
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
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Indicates an <see cref="Implementation"/> being added to an <see cref="IStore"/> is already in the store.
    /// </summary>
    [Serializable]
    public sealed class ImplementationAlreadyInStoreException : Exception
    {
        #region Properties
        /// <summary>
        /// The hash value the <see cref="Implementation"/> was supposed to have.
        /// </summary>
        public ManifestDigest ManifestDigest { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new implementation already in store exception.
        /// </summary>
        /// <param name="manifestDigest">The digest of the <see cref="Implementation"/> that was supposed to be added.</param>
        public ImplementationAlreadyInStoreException(ManifestDigest manifestDigest)
            : base(string.Format(Resources.ImplementationAlreadyInStore, manifestDigest))
        {
            ManifestDigest = manifestDigest;
        }

        /// <inheritdoc/>
        public ImplementationAlreadyInStoreException()
            : base(string.Format(Resources.ImplementationAlreadyInStore, "unknown"))
        {}

        /// <inheritdoc/>
        public ImplementationAlreadyInStoreException(string message) : base(message)
        {}

        /// <inheritdoc/>
        public ImplementationAlreadyInStoreException(string message, Exception innerException) : base(message, innerException)
        {}

        /// <summary>
        /// Deserializes an exception.
        /// </summary>
        private ImplementationAlreadyInStoreException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            #region Sanity checks
            if (info == null) throw new ArgumentNullException(nameof(info));
            #endregion

            ManifestDigest = (ManifestDigest)info.GetValue("ManifestDigest", typeof(ManifestDigest));
        }
        #endregion

        #region Serialization
        /// <inheritdoc/>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            #region Sanity checks
            if (info == null) throw new ArgumentNullException(nameof(info));
            #endregion

            info.AddValue("ManifestDigest", ManifestDigest);

            base.GetObjectData(info, context);
        }
        #endregion
    }
}
