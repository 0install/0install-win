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
using ICSharpCode.SharpZipLib.Tar;
using JetBrains.Annotations;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Extracts a TAR archive.
    /// </summary>
    public class TarExtractor : Extractor
    {
        #region Stream
        private readonly TarInputStream _tarStream;

        /// <summary>
        /// Prepares to extract a TAR archive contained in a stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed when the extractor is disposed.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">The archive is damaged.</exception>
        internal TarExtractor([NotNull] Stream stream, [NotNull] string target)
            : base(target)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            UnitsTotal = stream.Length;

            try
            {
                _tarStream = new TarInputStream(stream);
            }
            catch (SharpZipBaseException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _tarStream.Dispose();
        }
        #endregion

        /// <inheritdoc/>
        protected override void Execute()
        {
            State = TaskState.Data;

            try
            {
                if (!Directory.Exists(EffectiveTargetDir)) Directory.CreateDirectory(EffectiveTargetDir);

                TarEntry entry;
                while ((entry = _tarStream.GetNextEntry()) != null)
                {
                    string relativePath = GetRelativePath(entry.Name);
                    if (string.IsNullOrEmpty(relativePath)) continue;

                    if (entry.IsDirectory) CreateDirectory(relativePath, entry.TarHeader.ModTime);
                    else if (entry.TarHeader.TypeFlag == TarHeader.LF_LINK)
                    {
                        string targetPath = GetRelativePath(entry.TarHeader.LinkName);
                        if (string.IsNullOrEmpty(targetPath)) throw new IOException(string.Format(Resources.HardlinkTargetMissing, relativePath, entry.TarHeader.LinkName));
                        QueueHardlink(relativePath, targetPath, IsExecutable(entry));
                    }
                    else if (entry.TarHeader.TypeFlag == TarHeader.LF_SYMLINK) CreateSymlink(relativePath, entry.TarHeader.LinkName);
                    else WriteFile(relativePath, entry.Size, entry.TarHeader.ModTime, _tarStream, IsExecutable(entry));

                    UpdateProgress();
                }

                Finish();
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
        /// Updates <see cref="TaskBase.UnitsProcessed"/> to reflect the number of bytes extracted so far.
        /// </summary>
        protected virtual void UpdateProgress()
        {
            UnitsProcessed = _tarStream.Position;
        }

        /// <summary>
        /// Determines whether a <see cref="TarEntry"/> was created with the executable flag set.
        /// </summary>
        private static bool IsExecutable(TarEntry entry)
        {
            const int executeFlags = 1 + 8 + 64; // Octal: 111
            return (entry.TarHeader.Mode & executeFlags) > 0; // Check if anybody is allowed to execute
        }

        /// <summary>
        /// Helper method for <see cref="Extractor.WriteFile"/>.
        /// </summary>
        /// <param name="stream">The <see cref="TarInputStream"/> containing the entry data to write to a file.</param>
        /// <param name="fileStream">Stream access to the file to write.</param>
        protected override void StreamToFile(Stream stream, FileStream fileStream)
        {
            _tarStream.CopyEntryContents(fileStream);
        }
    }
}
