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
    /// An executable file-entry in a <see cref="Manifest"/>.
    /// </summary>
    /// <remarks>This class is immutable. It should only be used as a part of a <see cref="Manifest"/>.</remarks>
    [Serializable]
    public sealed class ManifestExecutableFile : ManifestFileBase, IEquatable<ManifestExecutableFile>
    {
        #region Constructor
        /// <summary>
        /// Creates a new executable file entry.
        /// </summary>
        /// <param name="hash">The hash of the content of the file calculated using the selected digest algorithm.</param>
        /// <param name="modifiedTime">The time this file was last modified in the number of seconds since the epoch.</param>
        /// <param name="size">The size of the file in bytes.</param>
        /// <param name="fileName">The name of the file without the containing directory.</param>
        /// <exception cref="NotSupportedException">Thrown if <paramref name="fileName"/> contains a newline character.</exception>
        public ManifestExecutableFile(string hash, long modifiedTime, long size, string fileName) : base(hash, modifiedTime, size, fileName)
        {}
        #endregion

        #region Factory methods
        /// <summary>
        /// Creates a new node from a string representation as created by <see cref="ToString"/>.
        /// </summary>
        /// <param name="line">The string representation to parse.</param>
        /// <returns>The newly created node.</returns>
        /// <exception cref="FormatException">Thrown if the <paramref name="line"/> format is incorrect.</exception>
        internal static ManifestExecutableFile FromString(string line)
        {
            const int numberOfParts = 5;
            string[] parts = line.Split(new[] { ' ' }, numberOfParts);
            if (parts.Length != numberOfParts) throw new ArgumentException(Resources.InvalidNumberOfLineParts, "line");

            try { return new ManifestExecutableFile(parts[1], long.Parse(parts[2]), long.Parse(parts[3]), parts[4]); }
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
        /// <returns><code>"X", space, hash, space, mtime, space, size, space, file name, newline</code></returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "X {0} {1} {2} {3}", Hash, ModifiedTime, Size, FileName);
        }
        #endregion

        #region Equality
        public bool Equals(ManifestExecutableFile other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(ManifestExecutableFile) && Equals((ManifestExecutableFile)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
