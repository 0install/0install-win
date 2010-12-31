﻿/*
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
using System.Globalization;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// An immutable symlink-entry in a <see cref="Manifest"/>.
    /// </summary>
    /// <remarks>This class is immutable. It should only be used as a part of a <see cref="Manifest"/>.</remarks>
    [Serializable]
    public sealed class ManifestSymlink : ManifestNode, IEquatable<ManifestSymlink>
    {
        #region Properties
        /// <summary>
        /// The hash of the link target path.
        /// </summary>
        public string Hash { get; private set; }
        
        /// <summary>
        /// The length of the link target path.
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// The name of the symlink without the containing directory.
        /// </summary>
        public string SymlinkName { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new symlink-entry.
        /// </summary>
        /// <param name="hash">The hash of the link target path.</param>
        /// <param name="size">The length of the link target path.</param>
        /// <param name="symlinkName">The name of the symlink without the containing directory.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="symlinkName"/> contains a newline character.</exception>
        internal ManifestSymlink(string hash, long size, string symlinkName)
        {
            #region Sanity checks
            if (symlinkName.Contains("\n")) throw new ArgumentException(Resources.NewlineInName, "symlinkName");
            #endregion

            Hash = hash;
            Size = size;
            SymlinkName = symlinkName;
        }
        #endregion

        #region Factory methods
        /// <summary>
        /// Creates a new node from a string representation as created by <see cref="ToString"/>.
        /// </summary>
        /// <param name="line">The string representation to parse.</param>
        /// <returns>The newly created node.</returns>
        /// <exception cref="FormatException">Thrown if the <paramref name="line"/> format is incorrect.</exception>
        internal static ManifestSymlink FromString(string line)
        {
            const int numberOfParts = 4;
            string[] parts = line.Split(new[] { ' ' }, numberOfParts);
            if (parts.Length != numberOfParts) throw new ArgumentException(Resources.InvalidNumberOfLineParts, "line");

            try { return new ManifestSymlink(parts[1], long.Parse(parts[2]), parts[3]); }
            #region Error handling
            catch (OverflowException ex)
            {
                throw new FormatException(Resources.NumberTooLarge, ex);
            }
            #endregion
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the string representation of this node for the manifest format.
        /// </summary>
        /// <returns><code>"S", space, hash, space, size, space, symlink name, newline</code></returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "S {0} {1} {2}", Hash, Size, SymlinkName);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ManifestSymlink other)
        {
            if (ReferenceEquals(null, other)) return false;

            return Equals(other.Hash, Hash) && other.Size == Size && Equals(other.SymlinkName, SymlinkName);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(ManifestSymlink) && Equals((ManifestSymlink)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Hash != null ? Hash.GetHashCode() : 0);
                result = (result * 397) ^ Size.GetHashCode();
                result = (result * 397) ^ (SymlinkName != null ? SymlinkName.GetHashCode() : 0);
                return result;
            }
        }
        #endregion
    }
}
