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
using System.Globalization;
using JetBrains.Annotations;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// A directory entry in a <see cref="Manifest"/>.
    /// </summary>
    /// <remarks>This class is immutable. It should only be used as a part of a <see cref="Manifest"/>.</remarks>
    [Serializable]
    public sealed class ManifestDirectory : ManifestNode, IEquatable<ManifestDirectory>
    {
        /// <summary>
        /// The complete path of this directory relative to the tree root as a Unix-Path beginning with a slash.
        /// </summary>
        [NotNull]
        public string FullPath { get; }

        /// <summary>
        /// Creates a new directory-entry.
        /// </summary>
        /// <param name="fullPath">The complete path of this directory relative to the tree root as a Unix-Path beginning with a slash.</param>
        /// <exception cref="ArgumentException"><paramref name="fullPath"/> contains a newline character.</exception>
        public ManifestDirectory(string fullPath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(fullPath)) throw new ArgumentNullException(nameof(fullPath));
            if (fullPath.Contains("\n")) throw new ArgumentException(Resources.NewlineInName, nameof(fullPath));
            #endregion

            FullPath = fullPath;
        }

        #region Factory methods
        /// <summary>
        /// Creates a new node from a string representation as created by <see cref="ToString"/>.
        /// </summary>
        /// <param name="line">The string representation to parse.</param>
        /// <returns>The newly created node.</returns>
        /// <exception cref="FormatException">The <paramref name="line"/> format is incorrect.</exception>
        [NotNull]
        internal static ManifestDirectory FromString([NotNull] string line)
        {
            const int numberOfParts = 2;
            string[] parts = line.Split(new[] {' '}, numberOfParts);
            if (parts.Length != numberOfParts) throw new FormatException(Resources.InvalidNumberOfLineParts);

            return new ManifestDirectory(parts[1]);
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the string representation of this node for the new manifest format.
        /// </summary>
        /// <returns><c>"D", space, full path name, newline</c></returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "D {0}", FullPath);
        }

        /// <summary>
        /// Returns the string representation of this node for the old manifest format.
        /// </summary>
        /// <returns><c>"D", space, mtime, space, full path name, newline</c></returns>
        public override string ToStringOld()
        {
            return string.Format(CultureInfo.InvariantCulture, "D {0}", FullPath);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ManifestDirectory other)
        {
            if (other == null) return false;

            return FullPath == other.FullPath;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is ManifestDirectory && Equals((ManifestDirectory)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return FullPath.GetHashCode();
        }
        #endregion
    }
}
