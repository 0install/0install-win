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
using System.IO;
using System.Threading;
using JetBrains.Annotations;
using NanoByte.Common.Streams;
using SevenZip.Sdk.Compression.Lzma;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Extracts a LZMA-compressed TAR archive.
    /// </summary>
    public class TarLzmaExtractor : TarExtractor
    {
        private readonly Stream _stream;

        /// <summary>
        /// Prepares to extract a TAR archive contained in a LZMA-compressed stream.
        /// </summary>
        /// <param name="stream">The stream containing the archive data to be extracted. Will be disposed when the extractor is disposed.</param>
        /// <param name="targetPath">The path to the directory to extract into.</param>
        /// <exception cref="IOException">The archive is damaged.</exception>
        internal TarLzmaExtractor([NotNull] Stream stream, [NotNull] string targetPath)
            : base(GetDecompressionStream(stream), targetPath)
        {
            _stream = stream;
            UnitsTotal = stream.Length;
        }

        /// <summary>
        /// Magic number used by <see cref="SevenZip"/> to indicate the size of a data stream is unknown and should be determined on-the-fly (keep reading/writing to EOF).
        /// </summary>
        internal const long UnknownSize = -1;

        /// <summary>
        /// Provides a filter for decompressing an LZMA encoded <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The underlying <see cref="Stream"/> providing the compressed data.</param>
        /// <param name="bufferSize">The maximum number of uncompressed bytes to buffer. 32k (the step size of <see cref="SevenZip"/>) is a sensible minimum.</param>
        /// <exception cref="IOException">The <paramref name="stream"/> doesn't start with a valid 5-bit LZMA header.</exception>
        /// <remarks>
        /// This method internally uses multi-threading and a <see cref="CircularBufferStream"/>.
        /// The <paramref name="stream"/> may be closed with a delay.
        /// </remarks>
        internal static Stream GetDecompressionStream(Stream stream, int bufferSize = 128 * 1024)
        {
            var bufferStream = new CircularBufferStream(bufferSize);
            var decoder = new Decoder();

            // Read LZMA header
            if (stream.CanSeek) stream.Position = 0;
            try
            {
                decoder.SetDecoderProperties(stream.Read(5));
            }
                #region Error handling
            catch (IOException ex)
            {
                // Wrap exception to add context
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            catch (ApplicationException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            #endregion

            // Read "uncompressed length" header
            var uncompressedLengthData = stream.Read(8);
            if (!BitConverter.IsLittleEndian) Array.Reverse(uncompressedLengthData);
            long uncompressedLength = BitConverter.ToInt64(uncompressedLengthData, startIndex: 0);

            bufferStream.SetLength((uncompressedLength == UnknownSize)
                ? (long)(stream.Length * 1.5)
                : uncompressedLength);

            var producerThread = new Thread(() =>
            {
                try
                {
                    decoder.Code(
                        inStream: stream, outStream: bufferStream,
                        inSize: UnknownSize, outSize: uncompressedLength,
                        progress: null);
                }
                catch (ThreadAbortException)
                {}
                catch (ObjectDisposedException)
                {
                    // If the buffer stream is closed too early the user probably just canceled the extraction process
                }
                catch (ApplicationException ex)
                {
                    bufferStream.RelayErrorToReader(new IOException(ex.Message, ex));
                }
                finally
                {
                    bufferStream.DoneWriting();
                }
            }) {IsBackground = true};
            producerThread.Start();

            return new DisposeWarpperStream(bufferStream, disposeHandler: () =>
            {
                producerThread.Abort();
                producerThread.Join();
                stream.Dispose();
            });
        }

        /// <inheritdoc/>
        protected override void UpdateProgress()
        {
            // Use original stream instead of decompressed stream to track progress
            UnitsProcessed = _stream.Position;
        }
    }
}
