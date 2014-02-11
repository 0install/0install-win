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
using Common.Tasks;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Tar;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Provides methods for extracting a TAR archive (optionally as a background task).
    /// </summary>
    public class TarExtractor : Extractor
    {
        #region Variables
        private readonly TarInputStream _tar;
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares to extract a TAR archive contained in a stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will not be disposed.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        public TarExtractor(Stream stream, string target) : base(stream, target)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            try
            {
                _tar = new TarInputStream(stream) {IsStreamOwner = false};
            }
            catch (SharpZipBaseException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Prepares to extract a TAR archive contained in a compressed stream.
        /// </summary>
        /// <param name="stream">The compressed stream. Only used to track progress. Will not be disposed.</param>
        /// <param name="decompressionStream">A non-seekable decompressed view of <paramref name="stream"/>. Will be disposed.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        protected TarExtractor(Stream stream, Stream decompressionStream, string target)
            : base(stream, target)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            if (decompressionStream == null) throw new ArgumentNullException("decompressionStream");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            try
            {
                _tar = new TarInputStream(decompressionStream) {IsStreamOwner = true};
            }
            catch (SharpZipBaseException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
        }
        #endregion

        //--------------------//

        #region Extraction
        /// <inheritdoc />
        protected override void RunTask()
        {
            lock (StateLock) State = TaskState.Data;

            try
            {
                if (!Directory.Exists(EffectiveTargetDir)) Directory.CreateDirectory(EffectiveTargetDir);

                TarEntry entry;
                while ((entry = _tar.GetNextEntry()) != null)
                {
                    string entryName = GetSubEntryName(entry.Name);
                    if (string.IsNullOrEmpty(entryName)) continue;

                    if (entry.IsDirectory) CreateDirectory(entryName, entry.TarHeader.ModTime);
                    else if (entry.TarHeader.TypeFlag == TarHeader.LF_LINK) CreateHardlink(entryName, entry.TarHeader.LinkName);
                    else if (entry.TarHeader.TypeFlag == TarHeader.LF_SYMLINK) CreateSymlink(entryName, entry.TarHeader.LinkName);
                    else WriteFile(entryName, entry.TarHeader.ModTime, _tar, entry.Size, IsExecutable(entry));

                    UnitsProcessed = Stream.Position;
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

            _tar.Dispose();
            lock (StateLock) State = TaskState.Complete;
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
            ((TarInputStream)stream).CopyEntryContents(fileStream);
        }
        #endregion

        #region Cancel
        /// <inheritdoc/>
        public override void Cancel()
        {
            base.Cancel();

            // Make sure any left-over worker threads are terminated
            _tar.Dispose();
        }
        #endregion
    }
}
