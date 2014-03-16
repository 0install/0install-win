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
using Common.Storage;
using Common.Tasks;
using Common.Utils;
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
        static SevenZipExtractor()
        {
            SevenZipBase.SetLibraryPath(Path.Combine(Locations.InstallBase, WindowsUtils.Is64BitProcess ? "7zxa-x64.dll" : "7zxa.dll"));
        }

        private readonly Stream _stream;
        private readonly SevenZip.SevenZipExtractor _extractor;

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

            try
            {
                _stream = stream;
                _extractor = new SevenZip.SevenZipExtractor(stream);
                UnitsTotal = _extractor.UnpackedSize;
            }
                #region Error handling
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
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _extractor.Dispose();
                _stream.Dispose();
            }
        }
        #endregion

        /// <inheritdoc/>
        protected override void Execute()
        {
            Status = TaskStatus.Data;

            try
            {
                if (!Directory.Exists(EffectiveTargetDir)) Directory.CreateDirectory(EffectiveTargetDir);

                if (_extractor.IsSolid || string.IsNullOrEmpty(SubDir)) ExtractComplete();
                else ExtractIndividual();
            }
                #region Error handling
            catch (SevenZipException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
            #endregion

            Status = TaskStatus.Complete;
        }

        /// <summary>
        /// Extracts all files from the archive in one go.
        /// </summary>
        private void ExtractComplete()
        {
            _extractor.Extracting += (sender, e) =>
            {
                UnitsProcessed = UnitsTotal * e.PercentDone / 100;
                if (CancellationToken.IsCancellationRequested) e.Cancel = true;
            };

            if (string.IsNullOrEmpty(SubDir)) _extractor.ExtractArchive(EffectiveTargetDir);
            else
            {
                using (var tempDirectory = new TemporaryDirectory("0install"))
                {
                    _extractor.ExtractArchive(tempDirectory);

                    string subDir = FileUtils.UnifySlashes(SubDir);
                    if (FileUtils.IsBreakoutPath(subDir)) return;
                    string tempSubDir = Path.Combine(tempDirectory, subDir);
                    if (Directory.Exists(tempSubDir))
                        new MoveDirectory(tempSubDir, EffectiveTargetDir).Run(CancellationToken);
                }
            }
        }

        /// <summary>
        /// Extracts files from the archive one-by-one.
        /// </summary>
        private void ExtractIndividual()
        {
            foreach (var entry in _extractor.ArchiveFileData)
            {
                string relativePath = GetRelativePath(entry.FileName.Replace('\\', '/'));
                if (relativePath == null) continue;

                if (entry.IsDirectory) CreateDirectory(relativePath, entry.LastWriteTime);
                else
                {
                    using (var stream = OpenFileWriteStream(relativePath))
                        _extractor.ExtractFile(entry.Index, stream);
                    File.SetLastWriteTimeUtc(CombinePath(relativePath), new DateTime(entry.LastWriteTime.Ticks, DateTimeKind.Utc));

                    UnitsProcessed += (long)entry.Size;
                }
            }
            SetDirectoryWriteTimes();
        }
    }
}
