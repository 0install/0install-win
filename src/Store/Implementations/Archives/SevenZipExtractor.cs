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
            if (disposing) _extractor.Dispose();
        }
        #endregion

        /// <inheritdoc/>
        protected override void Execute()
        {
            Status = TaskStatus.Data;

            try
            {
                if (!Directory.Exists(EffectiveTargetDir)) Directory.CreateDirectory(EffectiveTargetDir);

                if (string.IsNullOrEmpty(SubDir))
                {
                    _extractor.Extracting += OnExtracting;
                    _extractor.ExtractArchive(EffectiveTargetDir);
                }
                else ExtractWithSubDir();
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

        private void OnExtracting(object sender, ProgressEventArgs e)
        {
            UnitsProcessed = UnitsTotal * e.PercentDone / 100;
            if (CancellationToken.IsCancellationRequested) e.Cancel = true;
        }

        /// <summary>
        /// Extracts individual files from the archive, respecting the specified <see cref="Extractor.SubDir"/>.
        /// Very slow if <see cref="SevenZip.SevenZipExtractor.IsSolid"/>.
        /// </summary>
        private void ExtractWithSubDir()
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
                    File.SetLastWriteTimeUtc(CombinePath(relativePath), entry.LastWriteTime);

                    UnitsProcessed += (long)entry.Size;
                }
            }
            SetDirectoryWriteTimes();
        }
    }
}
