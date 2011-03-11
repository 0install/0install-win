/*
 * Copyright 2010-2011 Bastian Eicher
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
using Common.Streams;
using Common.Tasks;
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
        private readonly ZipFile _zip;
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
            lock (StateLock) State = TaskState.Data;

            try
            {
                if (!Directory.Exists(TargetDir)) Directory.CreateDirectory(TargetDir);

                foreach (ZipEntry entry in _zip)
                {
                    string entryName = GetSubEntryName(entry.Name);
                    if (string.IsNullOrEmpty(entryName)) continue;
                    DateTime modTime = GetEntryDateTime(entry);

                    if (entry.IsDirectory) CreateDirectory(entryName, modTime);
                    else if (entry.IsFile)
                    {
                        using (var stream = _zip.GetInputStream(entry))
                        {
                            if (IsSymlink(entry)) CreateSymlink(entryName, StreamUtils.ReadToString(stream));
                            else WriteFile(entryName, modTime, stream, entry.Size, IsExecutable(entry));
                        }
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
                    State = TaskState.IOError;
                }
                return;
            }
            catch (IOException ex)
            {
                lock (StateLock)
                {
                    ErrorMessage = ex.Message;
                    State = TaskState.IOError;
                }
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                lock (StateLock)
                {
                    ErrorMessage = ex.Message;
                    State = TaskState.IOError;
                }
                return;
            }
            #endregion

            lock (StateLock) State = TaskState.Complete;
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
                    // Parse Unix data
                    var entryData = extraData.GetEntryData();
                    unixData.SetData(entryData, 0, entryData.Length);
                    DateTime dateTime = unixData.CreateTime;

                    // HACK: Compensate for unintended local timezone shifting of the Unix epoch in official SharpZipLib release
                    //TimeSpan timeError = new DateTime(1970, 1, 1, 0, 0, 0) - new DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime();
                    //dateTime = dateTime + timeError;

                    return dateTime.ToUniversalTime();
                }
            }

            // Fall back to simple DOS time
            return entry.DateTime;
        }

        /// <summary>
        /// Determines whether a <see cref="ZipEntry"/> was created on a Unix-system with the symlink flag set.
        /// </summary>
        private static bool IsSymlink(ZipEntry entry)
        {
            if (entry.HostSystem != (int)HostSystemID.Unix) return false;

            const int symlinkFlag = (1 << 13) << 16;
            return (entry.ExternalFileAttributes & symlinkFlag) == symlinkFlag;
        }

        /// <summary>
        /// Determines whether a <see cref="ZipEntry"/> was created on a Unix-system with the executable flag set.
        /// </summary>
        private static bool IsExecutable(ZipEntry entry)
        {
            if (entry.HostSystem != (int)HostSystemID.Unix) return false;

            const int executeFlags = (1 + 8 + 64) << 16; // Octal: 111
            return (entry.ExternalFileAttributes & executeFlags) > 0; // Check if anybody is allowed to execute
        }
        #endregion
    }
}
