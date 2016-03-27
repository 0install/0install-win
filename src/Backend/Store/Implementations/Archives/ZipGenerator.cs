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
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using JetBrains.Annotations;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Creates a ZIP archive from a directory. Preserves execuable bits, symlinks and timestamps.
    /// </summary>
    public class ZipGenerator : ArchiveGenerator
    {
        #region Stream
        private readonly ZipOutputStream _zipStream;

        /// <summary>
        /// Prepares to generate a ZIP archive from a directory.
        /// </summary>
        /// <param name="sourceDirectory">The path of the directory to capture/store in the archive.</param>
        /// <param name="stream">The stream to write the generated archive to. Will be disposed when the generator is disposed.</param>
        internal ZipGenerator([NotNull] string sourceDirectory, [NotNull] Stream stream)
            : base(sourceDirectory)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            _zipStream = new ZipOutputStream(stream);
        }

        protected override void Dispose(bool disposing)
        {
            _zipStream.Dispose();
        }
        #endregion

        /// <inheritdoc/>
        protected override void HandleFile(FileInfo file, bool executable = false)
        {
            #region Sanity checks
            if (file == null) throw new ArgumentNullException("file");
            #endregion

            var entry = new ZipEntry(file.RelativeTo(SourceDirectory))
            {
                Size = file.Length,
                DateTime = file.LastWriteTimeUtc,
                HostSystem = HostSystemID.Unix,
                ExtraData = GetUnixTimestamp(file.LastWriteTimeUtc)
            };
            if (executable)
                entry.ExternalFileAttributes = ZipExtractor.DefaultAttributes | ZipExtractor.ExecuteAttributes;
            _zipStream.PutNextEntry(entry);
            using (var stream = file.OpenRead())
                stream.CopyTo(_zipStream);
        }

        /// <summary>
        /// Encodes a <paramref name="timestamp"/> as a <see cref="ZipEntry.ExtraData"/> in a format that ensures it will be read for <see cref="ZipEntry.DateTime"/>, preserving second-accuracy.
        /// </summary>
        private static byte[] GetUnixTimestamp(DateTime timestamp)
        {
            var extraData = new ZipExtraData();
            extraData.AddEntry(new ExtendedUnixData
            {
                AccessTime = timestamp, CreateTime = timestamp, ModificationTime = timestamp,
                Include = ExtendedUnixData.Flags.AccessTime | ExtendedUnixData.Flags.CreateTime | ExtendedUnixData.Flags.ModificationTime
            });
            return extraData.GetEntryData();
        }

        /// <inheritdoc/>
        protected override void HandleSymlink(FileSystemInfo symlink, byte[] data)
        {
            #region Sanity checks
            if (symlink == null) throw new ArgumentNullException("symlink");
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            _zipStream.PutNextEntry(new ZipEntry(symlink.RelativeTo(SourceDirectory))
            {
                Size = data.Length,
                HostSystem = HostSystemID.Unix,
                ExternalFileAttributes = ZipExtractor.DefaultAttributes | ZipExtractor.SymlinkAttributes
            });
            _zipStream.Write(data);
        }

        /// <inheritdoc/>
        protected override void HandleDirectory(DirectoryInfo directory)
        {
            #region Sanity checks
            if (directory == null) throw new ArgumentNullException("directory");
            #endregion

            _zipStream.PutNextEntry(new ZipEntry(directory.RelativeTo(SourceDirectory) + '/'));
        }
    }
}
