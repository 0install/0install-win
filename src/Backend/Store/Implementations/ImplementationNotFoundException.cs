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
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Indicates an <see cref="Store.Model.Implementation"/> could not be found in a <see cref="IStore"/>.
    /// </summary>
    [Serializable]
    public sealed class ImplementationNotFoundException : IOException
    {
        #region Properties
        /// <summary>
        /// The <see cref="ManifestDigest"/> of the <see cref="Store.Model.Implementation"/> to be found.
        /// </summary>
        public ManifestDigest ManifestDigest { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new implementation not found exception.
        /// </summary>
        /// <param name="manifestDigest">The <see cref="ManifestDigest"/> of the <see cref="Store.Model.Implementation"/> to be found.</param>
        public ImplementationNotFoundException(ManifestDigest manifestDigest)
            : base(string.Format(Resources.ImplementationNotFound, manifestDigest))
        {
            ManifestDigest = manifestDigest;
        }

        /// <summary>
        /// Creates a new implementation not found exception without specifying the specific implementation ID.
        /// </summary>
        public ImplementationNotFoundException()
            : base(string.Format(Resources.ImplementationNotFound, "unknown"))
        {}

        /// <inheritdoc/>
        public ImplementationNotFoundException(string message) : base(message)
        {}

        /// <inheritdoc/>
        public ImplementationNotFoundException(string message, Exception innerException) : base(message, innerException)
        {}

        /// <summary>
        /// Deserializes an exception.
        /// </summary>
        private ImplementationNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
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
