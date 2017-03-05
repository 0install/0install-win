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
using JetBrains.Annotations;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Manifests
{
    /// <summary>
    /// An abstract base class for directory-element entries (files and symlinks) in a <see cref="Manifest"/>.
    /// </summary>
    /// <remarks>This class and the derived classes are immutable. They should only be used as a part of a <see cref="Manifest"/>.</remarks>
    [Serializable]
    public abstract class ManifestDirectoryElement : ManifestNode
    {
        /// <summary>
        /// The digest of the content of the file calculated using the selected digest algorithm.
        /// </summary>
        [NotNull]
        public string Digest { get; }

        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// The name of the file without the containing directory.
        /// </summary>
        [NotNull]
        public string Name { get; }

        /// <summary>
        /// Creates a new directory element entry.
        /// </summary>
        /// <param name="digest">The digest of the content of the element calculated using the selected digest algorithm.</param>
        /// <param name="size">The size of the element in bytes.</param>
        /// <param name="name">The name of the element without the containing directory.</param>
        /// <exception cref="NotSupportedException"><paramref name="name"/> contains a newline character.</exception>
        protected ManifestDirectoryElement(string digest, long size, string name)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(digest)) throw new ArgumentNullException(nameof(digest));
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (name.Contains("\n")) throw new NotSupportedException(Resources.NewlineInName);
            #endregion

            Digest = digest;
            Size = size;
            Name = name;
        }

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(ManifestDirectoryElement other)
        {
            if (other == null) return false;
            return other.Digest == Digest && other.Size == Size && other.Name == Name;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = Digest.GetHashCode();
                result = (result * 397) ^ Size.GetHashCode();
                result = (result * 397) ^ Name.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}