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
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// An abstract base class for file-entries in a <see cref="Manifest"/>.
    /// </summary>
    /// <remarks>This class and the derived classes are immutable. They should only be used as a part of a <see cref="Manifest"/>.</remarks>
    public abstract class ManifestFileBase : ManifestNode
    {
        #region Properties
        /// <summary>
        /// The hash of the content of the file calculated using the selected digest algorithm.
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        /// The time this file was last modified in the number of seconds since the epoch.
        /// </summary>
        public long ModifiedTime { get; private set; }

        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// The name of the file without the containing directory.
        /// </summary>
        public string FileName { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new file entry.
        /// </summary>
        /// <param name="hash">The hash of the content of the file calculated using the selected digest algorithm.</param>
        /// <param name="modifiedTime">The time this file was last modified in the number of seconds since the epoch.</param>
        /// <param name="size">The size of the file in bytes.</param>
        /// <param name="fileName">The name of the file without the containing directory.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fileName"/> contains a newline character.</exception>
        protected ManifestFileBase(string hash, long modifiedTime, long size, string fileName)
        {
            #region Sanity checks
            if (fileName.Contains("\n")) throw new ArgumentException(Resources.NewlineInName, "fileName");
            #endregion

            Hash = hash;
            ModifiedTime = modifiedTime;
            Size = size;
            FileName = fileName;
        }
        #endregion

        //--------------------//

        #region Equality
        protected bool Equals(ManifestFileBase other)
        {
            if (ReferenceEquals(null, other)) return false;

            return other.Hash == Hash && other.ModifiedTime == ModifiedTime && other.Size == Size && other.FileName == FileName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Hash != null ? Hash.GetHashCode() : 0);
                result = (result * 397) ^ ModifiedTime.GetHashCode();
                result = (result * 397) ^ Size.GetHashCode();
                result = (result * 397) ^ (FileName != null ? FileName.GetHashCode() : 0);
                return result;
            }
        }
        #endregion
    }
}
