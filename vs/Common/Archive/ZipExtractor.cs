/*
 * Copyright 2006-2010 Bastian Eicher, Roland Leopold Walkling
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Common.Helpers;
using Common.Properties;
using ICSharpCode.SharpZipLib.Zip;

namespace Common.Archive
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
        /// <param name="stream">The stream containing the archive's data.</param>
        /// <param name="startOffset">The number of bytes at the beginning of the stream which should be ignored.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        public ZipExtractor(Stream stream, long startOffset, string target) : base(stream, startOffset, target)
        {
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

        #region Content
        /// <inheritdoc />
        public override IEnumerable<string> ListContent()
        {
            var contentList = new List<string>((int)_zip.Count);
            try
            {
                foreach (ZipEntry entry in _zip)
                    contentList.Add(StringHelper.UnifySlashes(entry.Name));
            }
            catch (ZipException ex)
            {
                throw new IOException(Resources.ArchiveInvalid, ex);
            }

            return contentList;
        }

        /// <inheritdoc />
        public override IEnumerable<string> ListDirectories()
        {
            var directoryList = new List<string>((int)_zip.Count);
            try
            {
                foreach (ZipEntry entry in _zip)
                    if (entry.IsDirectory) directoryList.Add(StringHelper.UnifySlashes(entry.Name));
            }
            catch (ZipException ex)
            {
                throw new IOException(Resources.ArchiveInvalid, ex);
            }

            return directoryList;
        }
        #endregion

        #region Extraction
        /// <inheritdoc />
        protected override void RunExtraction()
        {
            State = ProgressState.Data;

            try
            {
                foreach (ZipEntry entry in _zip)
                {
                    string entryName = GetSubEntryName(entry.Name);
                    if (string.IsNullOrEmpty(entryName)) continue;

                    if (entry.IsDirectory) CreateDirectory(entryName, entry.DateTime);
                    else if (entry.IsFile)
                    {
                        using (var stream = _zip.GetInputStream(entry))
                            WriteFile(entryName, entry.DateTime, stream, entry.Size, IsXbitSet(entry));
                    }

                    BytesProcessed = Stream.Position - StartOffset;
                }
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

            State = ProgressState.Complete;
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
