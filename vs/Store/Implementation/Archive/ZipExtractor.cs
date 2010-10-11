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
using ICSharpCode.SharpZipLib.Zip;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementation.Archive
{
    /// <summary>
    /// Provides methods for extracting a ZIP archive (optionally as a background task).
    /// </summary>
    public class ZipExtractor : Extractor
    {
        #region Variables
        private ZipFile _zip;
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares to extract a ZIP archive contained in a stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        public ZipExtractor(Stream stream, string target) : base(stream, target)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            try
            {
                _zip = new ZipFile(stream) { IsStreamOwner = false };
            }
            catch (ZipException ex)
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

                foreach (ZipEntry entry in _zip)
                {
                    string entryName = GetSubEntryName(entry.Name);
                    if (string.IsNullOrEmpty(entryName)) continue;
                    DateTime modTime = GetEntryDateTime(entry);

                    if (entry.IsDirectory) CreateDirectory(entryName, modTime);
                    else if (entry.IsFile)
                    {
                        using (var stream = _zip.GetInputStream(entry))
                            WriteFile(entryName, modTime, stream, entry.Size, IsXbitSet(entry));
                    }

                    BytesProcessed += entry.CompressedSize;
                }

                SetDirectoryWriteTimes();
            }
            #region Error handling
            catch (ZipException ex)
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
        /// Determines the "last changed" time of <see cref="ZipEntry"/>, using extended Unix data if possible.
        /// </summary>
        private static DateTime GetEntryDateTime(ZipEntry entry)
        {
            if (entry.HostSystem == (int)HostSystemID.Unix)
            {
                var unixData = new ExtendedUnixData();
                var extraData = new ZipExtraData(entry.ExtraData);
                if (extraData.Find(unixData.TagID))
                {
                    var entryData = extraData.GetEntryData();
                    unixData.SetData(entryData, 0, entryData.Length);

                    // ToDo: Remove this hack
                    return unixData.CreateTime.ToUniversalTime() + new TimeSpan(1, 0, 0);
                }
            }

            // Fall back to simple DOS time
            return entry.DateTime;
        }

        /// <summary>
        /// Determines whether an <see cref="ZipEntry"/> was packed on a Unix-system with the executable flag set to true.
        /// </summary>
        private static bool IsXbitSet(ZipEntry entry)
        {
            if (entry.HostSystem != (int)HostSystemID.Unix) return false;
            const int userExecuteFlag = 0x0040 << 16;
            return ((entry.ExternalFileAttributes & userExecuteFlag) == userExecuteFlag);
        }
        #endregion
    }
}
