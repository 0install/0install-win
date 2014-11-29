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
using System.Diagnostics.CodeAnalysis;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// A parameter structure (data transfer object) containing information about a requested archive extraction.
    /// </summary>
    /// <see cref="IStore.AddArchives"/>
    [Serializable]
    public struct ArchiveFileInfo : IEquatable<ArchiveFileInfo>
    {
        /// <summary>
        /// The file to be extracted.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The sub-directory in the archive (with Unix-style slashes) to be extracted; <see langword="null"/> to extract entire archive.
        /// </summary>
        public string SubDir { get; set; }

        /// <summary>
        /// Sub-path to be appended to the target directory without affecting location of flag files; <see langword="null"/> for none.
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// The MIME type of archive format of the file; <see langword="null"/> to guess.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// The number of bytes at the beginning of the file which should be ignored.
        /// </summary>
        public long StartOffset { get; set; }

        /// <summary>
        /// The URL the file was originally downloaded from.
        /// </summary>
        /// <remarks>This is used to provide additional information in case of an exception.</remarks>
        public Uri OriginalSource { get; set; }

        /// <summary>
        /// Returns the archive in the form "ArchiveFileInfo: Path (MimeType, + StartOffset, SubDir) => Destination". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            string result = string.Format("ArchiveFileInfo: {0} ({1}, + {2}, {3})", Path, MimeType, StartOffset, SubDir);
            if (!string.IsNullOrEmpty(Destination)) result += " => " + Destination;
            if (OriginalSource != null) result += ", originally from: " + OriginalSource.ToString();
            return result;
        }

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ArchiveFileInfo other)
        {
            // NOTE: Exclude Path from comparison to allow easy testing with randomized TemporaryFiles
            return string.Equals(SubDir, other.SubDir) && string.Equals(Destination, other.Destination) && string.Equals(MimeType, other.MimeType) && StartOffset == other.StartOffset && OriginalSource == other.OriginalSource;
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Usage", "CA2231:OverloadOperatorEqualsOnOverridingValueTypeEquals", Justification = "Equals() method is only used for easier unit testing")]
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ArchiveFileInfo && Equals((ArchiveFileInfo)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                // NOTE: Exclude Path from comparison to allow easy testing with randomized TemporaryFiles
                int hashCode = (SubDir != null ? SubDir.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Destination != null ? Destination.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MimeType != null ? MimeType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ StartOffset.GetHashCode();
                hashCode = (hashCode * 397) ^ (OriginalSource != null ? OriginalSource.GetHashCode() : 0);
                return hashCode;
            }
        }
        #endregion
    }
}
