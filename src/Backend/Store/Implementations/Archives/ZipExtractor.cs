/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.IO;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using JetBrains.Annotations;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using NanoByte.Common.Values;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Extracts a ZIP archive.
    /// </summary>
    public class ZipExtractor : Extractor
    {
        #region Stream
        /// <summary>Information about the files in the archive as stored in the central directory.</summary>
        private readonly ZipEntry[] _centralDirectory;

        private readonly ZipInputStream _zipStream;

        /// <summary>
        /// Prepares to extract a ZIP archive contained in a stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed when the extractor is disposed.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">The archive is damaged.</exception>
        internal ZipExtractor([NotNull] Stream stream, [NotNull] string target) : base(target)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            UnitsTotal = stream.Length;

            try
            {
                // Read the central directory
                using (var zipFile = new ZipFile(stream) {IsStreamOwner = false})
                {
                    _centralDirectory = new ZipEntry[zipFile.Count];
                    for (int i = 0; i < _centralDirectory.Length; i++)
                        _centralDirectory[i] = zipFile[i];
                }
                stream.Seek(0, SeekOrigin.Begin);

                _zipStream = new ZipInputStream(stream);
            }
                #region Error handling
            catch (ZipException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            #endregion
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _zipStream.Dispose();
        }
        #endregion

        /// <inheritdoc/>
        protected override void Execute()
        {
            State = TaskState.Data;

            try
            {
                if (!Directory.Exists(EffectiveTargetDir)) Directory.CreateDirectory(EffectiveTargetDir);

                // Read ZIP file sequentially and reference central directory in parallel
                int i = 0;
                ZipEntry localEntry;
                while ((localEntry = _zipStream.GetNextEntry()) != null)
                {
                    ZipEntry centralEntry = _centralDirectory[i++];

                    string relativePath = GetRelativePath(centralEntry.Name);
                    if (string.IsNullOrEmpty(relativePath)) continue;

                    if (centralEntry.IsDirectory) CreateDirectory(relativePath, localEntry.DateTime);
                    else if (centralEntry.IsFile)
                    {
                        if (IsSymlink(centralEntry)) CreateSymlink(relativePath, _zipStream.ReadToString());
                        else WriteFile(relativePath, centralEntry.Size, localEntry.DateTime, _zipStream, IsExecutable(centralEntry));
                    }

                    UnitsProcessed += centralEntry.CompressedSize;
                }

                SetDirectoryWriteTimes();
            }
                #region Error handling
            catch (SharpZipBaseException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            catch (InvalidDataException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            #endregion

            State = TaskState.Complete;
        }

        /// <summary>
        /// Determines whether a <see cref="ZipEntry"/> was created on a Unix-system with the symlink flag set.
        /// </summary>
        private static bool IsSymlink(ZipEntry entry)
        {
            if (entry.HostSystem != HostSystemID.Unix) return false;

            const int symlinkFlag = (1 << 13) << 16;
            return entry.ExternalFileAttributes.HasFlag(symlinkFlag);
        }

        /// <summary>
        /// Determines whether a <see cref="ZipEntry"/> was created on a Unix-system with the executable flag set.
        /// </summary>
        private static bool IsExecutable(ZipEntry entry)
        {
            if (entry.HostSystem != HostSystemID.Unix) return false;

            const int executeFlags = (1 + 8 + 64) << 16; // Octal: 111
            return (entry.ExternalFileAttributes & executeFlags) > 0; // Check if anybody is allowed to execute
        }
    }
}
