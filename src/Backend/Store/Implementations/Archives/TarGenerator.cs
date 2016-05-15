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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Tar;
using JetBrains.Annotations;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Creates a TAR archive from a directory. Preserves execuable bits, symlinks, hardlinks and timestamps.
    /// </summary>
    public class TarGenerator : ArchiveGenerator
    {
        #region Stream
        private readonly TarOutputStream _tarStream;

        /// <summary>
        /// Prepares to generate a TAR archive from a directory.
        /// </summary>
        /// <param name="sourceDirectory">The path of the directory to capture/store in the archive.</param>
        /// <param name="stream">The stream to write the generated archive to. Will be disposed when the generator is disposed.</param>
        internal TarGenerator([NotNull] string sourceDirectory, [NotNull] Stream stream)
            : base(sourceDirectory)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            _tarStream = new TarOutputStream(stream);
        }

        protected override void Dispose(bool disposing)
        {
            _tarStream.Dispose();
        }
        #endregion

        private readonly List<FileInfo> _previousFiles = new List<FileInfo>();

        /// <inheritdoc/>
        protected override void HandleFile(FileInfo file, bool executable = false)
        {
            #region Sanity checks
            if (file == null) throw new ArgumentNullException("file");
            #endregion

            var entry = new TarEntry(new TarHeader
            {
                Name = file.RelativeTo(SourceDirectory),
                ModTime = file.LastWriteTimeUtc,
                Mode = (executable ? TarExtractor.DefaultMode | TarExtractor.ExecuteMode : TarExtractor.DefaultMode)
            });

            var hardlinkTarget = _previousFiles.FirstOrDefault(previousFile => FileUtils.AreHardlinked(previousFile.FullName, file.FullName));
            if (hardlinkTarget != null)
            {
                entry.TarHeader.TypeFlag = TarHeader.LF_LINK;
                entry.TarHeader.LinkName = hardlinkTarget.RelativeTo(SourceDirectory);
                _tarStream.PutNextEntry(entry);
            }
            else
            {
                _previousFiles.Add(file);

                entry.Size = file.Length;
                _tarStream.PutNextEntry(entry);
                using (var stream = file.OpenRead())
                    stream.CopyToEx(_tarStream);
            }
            _tarStream.CloseEntry();
        }

        /// <inheritdoc/>
        protected override void HandleSymlink(FileSystemInfo symlink, byte[] data)
        {
            #region Sanity checks
            if (symlink == null) throw new ArgumentNullException("symlink");
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            _tarStream.PutNextEntry(new TarEntry(new TarHeader
            {
                Name = symlink.RelativeTo(SourceDirectory),
                TypeFlag = TarHeader.LF_SYMLINK,
                Size = data.Length
            }));
            _tarStream.Write(data);
            _tarStream.CloseEntry();
        }

        /// <inheritdoc/>
        protected override void HandleDirectory(DirectoryInfo directory)
        {
            #region Sanity checks
            if (directory == null) throw new ArgumentNullException("directory");
            #endregion

            _tarStream.PutNextEntry(new TarEntry(new TarHeader
            {
                Name = directory.RelativeTo(SourceDirectory),
                TypeFlag = TarHeader.LF_DIR,
                Mode = TarExtractor.DefaultMode | TarExtractor.ExecuteMode
            }));
            _tarStream.CloseEntry();
        }
    }
}
