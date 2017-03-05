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
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using SevenZip;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Extracts a 7z archive.
    /// </summary>
    public class SevenZipExtractor : ArchiveExtractor
    {
        #region Stream
        private readonly Stream _stream;

        /// <summary>
        /// Prepares to extract a 7z archive contained in a stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed when the extractor is disposed.</param>
        /// <param name="targetPath">The path to the directory to extract into.</param>
        /// <exception cref="IOException">The archive is damaged.</exception>
        internal SevenZipExtractor([NotNull] Stream stream, [NotNull] string targetPath)
            : base(targetPath)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            #endregion

            if (!WindowsUtils.IsWindows) throw new NotSupportedException(Resources.ExtractionOnlyOnWindows);

            _stream = stream;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _stream.Dispose();
        }
        #endregion

        private bool _unitsByte;

        /// <inheritdoc/>
        protected override bool UnitsByte => _unitsByte;

        /// <inheritdoc/>
        protected override void Execute()
        {
            State = TaskState.Header;

            try
            {
                // NOTE: Must do initialization here since the constructor may be called on a different thread and SevenZipSharp is thread-affine
                SevenZipBase.SetLibraryPath(Locations.GetInstalledFilePath(WindowsUtils.Is64BitProcess ? "7zxa-x64.dll" : "7zxa.dll"));

                using (var extractor = new SevenZip.SevenZipExtractor(_stream))
                {
                    State = TaskState.Data;
                    if (!Directory.Exists(EffectiveTargetPath)) Directory.CreateDirectory(EffectiveTargetPath);
                    if (extractor.IsSolid || string.IsNullOrEmpty(Extract)) ExtractComplete(extractor);
                    else ExtractIndividual(extractor);
                }
            }
                #region Error handling
            catch (ObjectDisposedException ex)
            {
                // Async cancellation may cause underlying file stream to be closed
                Log.Warn(ex);
                throw new OperationCanceledException();
            }
            catch (SevenZipException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            catch (ArgumentException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            catch (NullReferenceException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            #endregion

            State = TaskState.Complete;
        }

        /// <summary>
        /// Extracts all files from the archive in one go.
        /// </summary>
        private void ExtractComplete(SevenZip.SevenZipExtractor extractor)
        {
            _unitsByte = false;
            UnitsTotal = 100;
            extractor.Extracting += (sender, e) => { UnitsProcessed = e.PercentDone; };

            CancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(Extract)) extractor.ExtractArchive(EffectiveTargetPath);
            else
            {
                // Use an intermediate temp directory (on the same filesystem)
                string tempDir = Path.Combine(TargetPath, Path.GetRandomFileName());
                extractor.ExtractArchive(tempDir);

                // Get only a specific subdir even though we extracted everything
                string subDir = FileUtils.UnifySlashes(Extract);
                string tempSubDir = Path.Combine(tempDir, subDir);
                if (!FileUtils.IsBreakoutPath(subDir) && Directory.Exists(tempSubDir))
                    new MoveDirectory(tempSubDir, EffectiveTargetPath, overwrite: true).Run(CancellationToken);
                Directory.Delete(tempDir, recursive: true);
            }
            CancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Extracts files from the archive one-by-one.
        /// </summary>
        private void ExtractIndividual(SevenZip.SevenZipExtractor extractor)
        {
            _unitsByte = true;
            UnitsTotal = extractor.UnpackedSize;

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
            Finish();
        }
    }
}
