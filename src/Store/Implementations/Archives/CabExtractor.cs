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
    /// Extracts a MS Cabinet archive.
    /// </summary>
    public sealed class CabExtractor : Extractor
    {
        #region Stream
        private readonly StreamContext _streamContext;
        private readonly CabEngine _cabEngine = new CabEngine();

        /// <summary>
        /// Prepares to extract a MS Cabinet archive contained in a stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed when the extractor is disposed.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">Thrown if the archive is damaged.</exception>
        internal CabExtractor(Stream stream, string target)
            : base(target)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            _streamContext = new StreamContext(this, stream);

            try
            {
                UnitsTotal = _cabEngine.GetFileInfo(_streamContext, _ => true).Sum(x => x.Length);
            }
                #region Error handling
            catch (CabException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
            #endregion
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _streamContext.Dispose();
        }
        #endregion

        /// <inheritdoc />
        protected override void RunTask()
        {
            lock (StateLock) State = TaskState.Data;

            try
            {
                if (!Directory.Exists(EffectiveTargetDir)) Directory.CreateDirectory(EffectiveTargetDir);
                _cabEngine.Unpack(_streamContext, _ => true);

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

            lock (StateLock) State = TaskState.Complete;
        }

        private class StreamContext : IUnpackStreamContext, IDisposable
        {
            private readonly CabExtractor _extractor;
            private readonly Stream _fileStream;

            public StreamContext(CabExtractor extractor, Stream fileStream)
            {
                _extractor = extractor;
                _fileStream = fileStream;
            }

            public Stream OpenArchiveReadStream(int archiveNumber, string archiveName, CompressionEngine compressionEngine)
            {
                return new DuplicateStream(_fileStream);
            }

            public void CloseArchiveReadStream(int archiveNumber, string archiveName, Stream stream)
            {}

            private long _bytesStaged;

            public Stream OpenFileWriteStream(string path, long fileSize, DateTime lastWriteTime)
            {
                string entryName = _extractor.GetSubEntryName(path);
                if (entryName == null) return null;

                _bytesStaged = fileSize;
                return _extractor.OpenFileWriteStream(entryName, fileSize);
            }

            public void CloseFileWriteStream(string path, Stream stream, FileAttributes attributes, DateTime lastWriteTime)
            {
                stream.Close();
                File.SetLastWriteTimeUtc(_extractor.CombinePath(_extractor.GetSubEntryName(path)), lastWriteTime);

                _extractor.UnitsProcessed += _bytesStaged;
            }

            public void Dispose()
            {
                _fileStream.Dispose();
            }
        }
    }
}
