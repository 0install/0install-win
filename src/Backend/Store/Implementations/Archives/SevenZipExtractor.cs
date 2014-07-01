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
using System.Collections.Generic;
using System.IO;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NanoByte.Common.Utils;
using SevenZip;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Extracts a 7z archive.
    /// </summary>
    public class SevenZipExtractor : Extractor
    {
        #region Stream
        private readonly Stream _stream;

        /// <summary>
        /// Prepares to extract a 7z archive contained in a stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed when the extractor is disposed.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        internal SevenZipExtractor(Stream stream, string target)
            : base(target)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            if (!WindowsUtils.IsWindows) throw new NotSupportedException(Resources.ExtractionOnlyOnWindows);

            _stream = stream;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _stream.Dispose();
        }
        #endregion

        /// <inheritdoc/>
        protected override void Execute()
        {
            State = TaskState.Header;

            try
            {
                // NOTE: Must do initialization here since the constructor may be called on a different thread and SevenZipSharp is thread-affine
                SevenZipBase.SetLibraryPath(Path.Combine(Locations.InstallBase, WindowsUtils.Is64BitProcess ? "7zxa-x64.dll" : "7zxa.dll"));
                using (var extractor = new SevenZip.SevenZipExtractor(_stream))
                {
                    UnitsTotal = extractor.UnpackedSize;

                    State = TaskState.Data;
                    if (!Directory.Exists(EffectiveTargetDir)) Directory.CreateDirectory(EffectiveTargetDir);
                    if (extractor.IsSolid || string.IsNullOrEmpty(SubDir)) ExtractComplete(extractor);
                    else ExtractIndividual(extractor);
                }
            }
                #region Error handling
            catch (SevenZipException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
            catch (ArgumentException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
            catch (NullReferenceException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
            #endregion

            State = TaskState.Complete;
        }

        /// <summary>
        /// Extracts all files from the archive in one go.
        /// </summary>
        private void ExtractComplete(SevenZip.SevenZipExtractor extractor)
        {
            extractor.Extracting += (sender, e) => { UnitsProcessed = UnitsTotal * e.PercentDone / 100; };

            CancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(SubDir)) extractor.ExtractArchive(EffectiveTargetDir);
            else
            {
                // Use an intermediate temp directory (on the same filesystem)
                string tempDir = Path.Combine(TargetDir, Path.GetRandomFileName());
                extractor.ExtractArchive(tempDir);

                // Get only a specific subdir even though we extracted everything
                string subDir = FileUtils.UnifySlashes(SubDir);
                string tempSubDir = Path.Combine(tempDir, subDir);
                if (!FileUtils.IsBreakoutPath(subDir) && Directory.Exists(tempSubDir))
                    new MoveDirectory(tempSubDir, EffectiveTargetDir, overwrite: true).Run(CancellationToken);
                else Directory.Delete(tempDir, recursive: true);
            }
            CancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Extracts files from the archive one-by-one.
        /// </summary>
        private void ExtractIndividual(SevenZip.SevenZipExtractor extractor)
        {
            foreach (var entry in extractor.ArchiveFileData)
            {
                string relativePath = GetRelativePath(entry.FileName.Replace('\\', '/'));
                if (relativePath == null) continue;

                if (entry.IsDirectory) CreateDirectory(relativePath, entry.LastWriteTime);
                else
                {
                    using (var stream = OpenFileWriteStream(relativePath))
                        extractor.ExtractFile(entry.Index, stream);
                    File.SetLastWriteTimeUtc(CombinePath(relativePath), DateTime.SpecifyKind(entry.LastWriteTime, DateTimeKind.Utc));

                    UnitsProcessed += (long)entry.Size;
                }
            }
            SetDirectoryWriteTimes();
        }
    }
}
