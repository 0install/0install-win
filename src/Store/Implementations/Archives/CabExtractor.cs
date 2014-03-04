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
using System.Linq;
using Common.Tasks;
using Common.Utils;
using WixToolset.Dtf.Compression;
using WixToolset.Dtf.Compression.Cab;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Provides methods for extracting a MS Cabinet archive (optionally as a background task).
    /// </summary>
    public class CabExtractor : Extractor, IUnpackStreamContext
    {
        #region Variables
        private readonly CabEngine _cabEngine = new CabEngine();
        #endregion

        #region Constructor
        /// <summary>
        /// Prepares to extract a MS Cabinet archive contained in a stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will not be disposed.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        public CabExtractor(Stream stream, string target)
            : base(stream, target)
        {
            try
            {
                UnitsTotal = _cabEngine.GetFileInfo(this, _ => true).Sum(x => x.Length);
            }
                #region Error handling
            catch (CabException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
            #endregion
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
                _cabEngine.Unpack(this, _ => true);

                // CABs do not store modification times for diretories but manifests need them to be consistent, so we fix them to the beginning of the Unix epoch
                new DirectoryInfo(TargetDir).Walk(dir => dir.LastWriteTimeUtc = FileUtils.FromUnixTime(0));
            }
                #region Error handling
            catch (CabException ex)
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

            Stream.Dispose();
            lock (StateLock) State = TaskState.Complete;
        }

        Stream IUnpackStreamContext.OpenArchiveReadStream(int archiveNumber, string archiveName, CompressionEngine compressionEngine)
        {
            return new DuplicateStream(Stream);
        }

        void IUnpackStreamContext.CloseArchiveReadStream(int archiveNumber, string archiveName, Stream stream)
        {}

        private long _bytesStaged;

        Stream IUnpackStreamContext.OpenFileWriteStream(string path, long fileSize, DateTime lastWriteTime)
        {
            string entryName = GetSubEntryName(path);
            if (entryName == null) return null;

            _bytesStaged = fileSize;
            return OpenFileWriteStream(entryName, fileSize);
        }

        void IUnpackStreamContext.CloseFileWriteStream(string path, Stream stream, FileAttributes attributes, DateTime lastWriteTime)
        {
            stream.Close();
            File.SetLastWriteTimeUtc(CombinePath(GetSubEntryName(path)), lastWriteTime);

            UnitsProcessed += _bytesStaged;
        }
        #endregion

        #region Cancel
        /// <inheritdoc/>
        public override void Cancel()
        {
            base.Cancel();

            // Make sure any left-over worker threads are terminated
            Stream.Dispose();
        }
        #endregion
    }
}
