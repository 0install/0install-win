/*
 * Copyright 2010-2014 Bastian Eicher
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

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// An directory-entry in a <see cref="Manifest"/>.
    /// </summary>
    /// <remarks>This class is immutable. It should only be used as a part of a <see cref="Manifest"/>.</remarks>
    [Serializable]
    public sealed class ManifestDirectory : ManifestNode, IEquatable<ManifestDirectory>
    {
        #region Properties
        /// <summary>
        /// The time this directory was last modified encoded as Unix time (number of seconds since the epoch).
        /// </summary>
        /// <remarks>Only used for old manifest format.</remarks>
        public long ModifiedTime { get; private set; }

        /// <summary>
        /// The complete path of this directory relative to the tree root as a Unix-Path beginning with a slash.
        /// </summary>
        public string FullPath { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new directory-entry.
        /// </summary>
        /// <param name="modifiedTime">The time this directory was last modified in the number of seconds since the epoch.</param>
        /// <param name="fullPath">The complete path of this directory relative to the tree root as a Unix-Path beginning with a slash.</param>
        /// <exception cref="ArgumentException"><paramref name="fullPath"/> contains a newline character.</exception>
        public ManifestDirectory(long modifiedTime, string fullPath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(fullPath)) throw new ArgumentNullException("fullPath");
            if (fullPath.Contains("\n")) throw new ArgumentException(Resources.NewlineInName, "fullPath");
            #endregion

            ModifiedTime = modifiedTime;
            FullPath = fullPath;
        }

        /// <summary>
        /// Creates a new directory-entry (old format).
        /// </summary>
        /// <param name="fullPath">The complete path of this directory relative to the tree root as a Unix-Path beginning with a slash.</param>
        internal ManifestDirectory(string fullPath) : this(0, fullPath)
        {}
        #endregion

        #region Factory methods
        /// <summary>
        /// Creates a new node from a string representation as created by <see cref="ToString"/>.
        /// </summary>
        /// <param name="line">The string representation to parse.</param>
        /// <returns>The newly created node.</returns>
        /// <exception cref="FormatException">The <paramref name="line"/> format is incorrect.</exception>
        internal static ManifestDirectory FromString(string line)
        {
            const int numberOfParts = 2;
            string[] parts = line.Split(new[] {' '}, numberOfParts);
            if (parts.Length != numberOfParts) throw new FormatException(Resources.InvalidNumberOfLineParts);

            return new ManifestDirectory(parts[1]);
        }

        /// <summary>
        /// Creates a new node from a string representation as created by <see cref="ToStringOld"/>.
        /// </summary>
        /// <param name="line">The string representation to parse.</param>
        /// <returns>The newly created node.</returns>
        /// <exception cref="FormatException">The <paramref name="line"/> format is incorrect.</exception>
        internal static ManifestDirectory FromStringOld(string line)
        {
            const int numberOfParts = 3;
            string[] parts = line.Split(new[] {' '}, numberOfParts);
            if (parts.Length != numberOfParts) throw new FormatException(Resources.InvalidNumberOfLineParts);

            try
            {
                return new ManifestDirectory(long.Parse(parts[1]), parts[2]);
            }
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
        /// Returns the string representation of this node for the new manifest format.
        /// </summary>
        /// <returns><code>"D", space, full path name, newline</code></returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "D {0}", FullPath);
        }

        /// <summary>
        /// Returns the string representation of this node for the old manifest format.
        /// </summary>
        /// <returns><code>"D", space, mtime, space, full path name, newline</code></returns>
        public override string ToStringOld()
        {
            return string.Format(CultureInfo.InvariantCulture, "D {0} {1}", ModifiedTime, FullPath);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ManifestDirectory other)
        {
            if (other == null) return false;

            // Directory ModifiedTime is ignored in the new manifest format
            return /*other.ModifiedTime == ModifiedTime &&*/ Equals(other.FullPath, FullPath);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ManifestDirectory && Equals((ManifestDirectory)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (ModifiedTime.GetHashCode() * 397) ^ (FullPath != null ? FullPath.GetHashCode() : 0);
            }
        }
        #endregion
    }
}
