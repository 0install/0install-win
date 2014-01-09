﻿/*
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

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// An non-executable file-entry in a <see cref="Manifest"/>.
    /// </summary>
    /// <remarks>This class is immutable. It should only be used as a part of a <see cref="Manifest"/>.</remarks>
    [Serializable]
    public sealed class ManifestNormalFile : ManifestFileBase, IEquatable<ManifestNormalFile>
    {
        #region Constructor
        /// <summary>
        /// Creates a new non-executable file entry.
        /// </summary>
        /// <param name="hash">The hash of the content of the file calculated using the selected digest algorithm.</param>
        /// <param name="modifiedTime">The time this file was last modified in the number of seconds since the epoch.</param>
        /// <param name="size">The size of the file in bytes.</param>
        /// <param name="fileName">The name of the file without the containing directory.</param>
        /// <exception cref="NotSupportedException">Thrown if <paramref name="fileName"/> contains a newline character.</exception>
        public ManifestNormalFile(string hash, long modifiedTime, long size, string fileName) : base(hash, modifiedTime, size, fileName)
        {}
        #endregion

        #region Factory methods
        /// <summary>
        /// Creates a new node from a string representation as created by <see cref="ToString"/>.
        /// </summary>
        /// <param name="line">The string representation to parse.</param>
        /// <returns>The newly created node.</returns>
        /// <exception cref="FormatException">Thrown if the <paramref name="line"/> format is incorrect.</exception>
        internal static ManifestNormalFile FromString(string line)
        {
            const int numberOfParts = 5;
            string[] parts = line.Split(new[] {' '}, numberOfParts);
            if (parts.Length != numberOfParts) throw new FormatException(Resources.InvalidNumberOfLineParts);

            try
            {
                return new ManifestNormalFile(parts[1], long.Parse(parts[2]), long.Parse(parts[3]), parts[4]);
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
        /// Returns the string representation of this node for the manifest format.
        /// </summary>
        /// <returns><code>"F", space, hash, space, mtime, space, size, space, file name, newline</code></returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "F {0} {1} {2} {3}", Digest, ModifiedTime, Size, FileName);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ManifestNormalFile other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ManifestNormalFile && Equals((ManifestNormalFile)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
