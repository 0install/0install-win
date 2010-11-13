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

namespace ZeroInstall.Store.Implementation.Archive
{
    /// <summary>
    /// A parameter structure containing information about a requested archive extraction.
    /// </summary>
    /// <see cref="IStore.AddArchive"/>
    /// <see cref="IStore.AddMultipleArchives"/>
    [Serializable]
    public struct ArchiveFileInfo
    {
        /// <summary>
        /// The file to be extracted.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The MIME type of archive format of the file; <see langword="null"/> to guess.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// The sub-directory in the archive to be extracted; <see langword="null"/> for entire archive.
        /// </summary>
        public string SubDir { get; set; }

        /// <summary>
        /// The number of bytes at the beginning of the file which should be ignored.
        /// </summary>
        public long StartOffset { get; set; }
    }
}
