/*
 * Copyright 2010 Bastian Eicher
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
using Common;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Tar;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation.Archive
{
    /// <summary>
    /// Provides methods for extracting a TAR archive (optionally as a background task).
    /// </summary>
    public class TarExtractor : Extractor
    {
        #region Variables
        private TarInputStream _tar;
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares to extract a TAR archive contained in a stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed.</param>
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
                _tar = new TarInputStream(stream);
            }
            catch (SharpZipBaseException ex)
            {
                // Make sure only standard exception types are thrown to the outside
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
        }
        #endregion

        //--------------------//
        
        #region Extraction
        /// <inheritdoc />
        protected override void RunTask()
        {
            lock (StateLock) State = ProgressState.Data;

            try
            {
                if (!Directory.Exists(Target)) Directory.CreateDirectory(Target);

                TarEntry entry;
                while ((entry = _tar.GetNextEntry()) != null)
                {
                    string entryName = GetSubEntryName(entry.Name);
                    if (string.IsNullOrEmpty(entryName)) continue;

                    if (entry.IsDirectory) CreateDirectory(entryName, entry.TarHeader.ModTime);
                    else WriteFile(entryName, entry.TarHeader.ModTime, _tar, entry.Size, IsXbitSet(entry));

                    BytesProcessed = _tar.Position;
                }
            }
            #region Error handling
            catch (SharpZipBaseException ex)
            {
                lock (StateLock)
                {
                    ErrorMessage = Resources.ArchiveInvalid + "\n" + ex.Message;
                    State = ProgressState.IOError;
                }
                return;
            }
            catch (IOException ex)
            {
                lock (StateLock)
                {
                    ErrorMessage = ex.Message;
                    State = ProgressState.IOError;
                }
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                lock (StateLock)
                {
                    ErrorMessage = ex.Message;
                    State = ProgressState.IOError;
                }
                return;
            }
            #endregion

            lock (StateLock) State = ProgressState.Complete;
        }

        /// <summary>
        /// Determines whether an <see cref="TarEntry"/> was packed on a Unix-system with the executable flag set to true.
        /// </summary>
        private static bool IsXbitSet(TarEntry entry)
        {
            const int executeFlags = 1 + 8 + 8 * 8; // Octal: 111
            return ((entry.TarHeader.Mode & executeFlags) != 0);
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
    }
}
