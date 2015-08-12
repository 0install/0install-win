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
using NanoByte.Common.Storage;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// An abstract base class for file entries in a <see cref="Manifest"/>.
    /// </summary>
    /// <remarks>This class and the derived classes are immutable. They should only be used as a part of a <see cref="Manifest"/>.</remarks>
    [Serializable]
    public abstract class ManifestFileBase : ManifestDirectoryElement
    {
        /// <summary>
        /// The time this file was last modified as Unix time.
        /// </summary>
        protected long ModifiedTimeUnix { get; private set; }

        /// <summary>
        /// The time this file was last modified.
        /// </summary>
        public DateTime ModifiedTime => FileUtils.FromUnixTime(ModifiedTimeUnix);

        /// <summary>
        /// Creates a new file entry.
        /// </summary>
        /// <param name="digest">The digest of the content of the file calculated using the selected digest algorithm.</param>
        /// <param name="modifiedTime">The time this file was last modified.</param>
        /// <param name="size">The size of the file in bytes.</param>
        /// <param name="name">The name of the file without the containing directory.</param>
        /// <exception cref="NotSupportedException"><paramref name="name"/> contains a newline character.</exception>
        protected ManifestFileBase(string digest, DateTime modifiedTime, long size, string name)
            : base(digest, size, name)
        {
            ModifiedTimeUnix = modifiedTime.ToUnixTime();
        }

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(ManifestFileBase other)
        {
            if (other == null) return false;
            return ModifiedTimeUnix == other.ModifiedTimeUnix && base.Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ ModifiedTimeUnix.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
