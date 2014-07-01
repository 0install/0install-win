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
using System.IO;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
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
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        internal ZipExtractor(Stream stream, string target) : base(target)
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
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
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
                    DateTime modTime = GetEntryDateTime(centralEntry, localEntry);

                    if (centralEntry.IsDirectory) CreateDirectory(relativePath, modTime);
                    else if (centralEntry.IsFile)
                    {
                        if (IsSymlink(centralEntry)) CreateSymlink(relativePath, _zipStream.ReadToString());
                        else WriteFile(relativePath, centralEntry.Size, modTime, _zipStream, IsExecutable(centralEntry));
                    }

                    UnitsProcessed += centralEntry.CompressedSize;
                }

                SetDirectoryWriteTimes();
            }
                #region Error handling
            catch (SharpZipBaseException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
            catch (InvalidDataException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
            #endregion

            State = TaskState.Complete;
        }

        /// <summary>
        /// Determines the "last changed" time of an entry, using extended Unix data if possible.
        /// </summary>
        /// <param name="centralEntry">The entry data from the central directory.</param>
        /// <param name="localEntry">The entry data from the local header.</param>
        private static DateTime GetEntryDateTime(ZipEntry centralEntry, ZipEntry localEntry)
        {
            if (centralEntry.HostSystem == (int)HostSystemID.Unix)
            {
                var unixData = new ExtendedUnixData();
                var extraData = new ZipExtraData(centralEntry.ExtraData);
                if (extraData.Find(unixData.TagID))
                {
                    // Parse Unix data
                    var entryData = extraData.GetEntryData();
                    unixData.SetData(entryData, 0, entryData.Length);
                    return unixData.CreateTime;
                }
            }
            //else if (centralEntry.HostSystem == (int)HostSystemID.WindowsNT)
            //{
            //    var ntData = new NTTaggedData();
            //    var extraData = new ZipExtraData(centralEntry.ExtraData);
            //    if (extraData.Find(ntData.TagID))
            //    {
            //        // Parse Unix data
            //        var entryData = extraData.GetEntryData();
            //        ntData.SetData(entryData, 0, entryData.Length);
            //        return ntData.LastModificationTime;
            //    }
            //}

            // Fall back to simple DOS time
            return localEntry.DateTime;
        }

        /// <summary>
        /// Determines whether a <see cref="ZipEntry"/> was created on a Unix-system with the symlink flag set.
        /// </summary>
        private static bool IsSymlink(ZipEntry entry)
        {
            if (entry.HostSystem != (int)HostSystemID.Unix) return false;

            const int symlinkFlag = (1 << 13) << 16;
            return entry.ExternalFileAttributes.HasFlag(symlinkFlag);
        }

        /// <summary>
        /// Determines whether a <see cref="ZipEntry"/> was created on a Unix-system with the executable flag set.
        /// </summary>
        private static bool IsExecutable(ZipEntry entry)
        {
            if (entry.HostSystem != (int)HostSystemID.Unix) return false;

            const int executeFlags = (1 + 8 + 64) << 16; // Octal: 111
            return (entry.ExternalFileAttributes & executeFlags) > 0; // Check if anybody is allowed to execute
        }
    }
}
