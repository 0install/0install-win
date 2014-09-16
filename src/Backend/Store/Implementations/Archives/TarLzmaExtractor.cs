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
using System.Threading;
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
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">The archive is damaged.</exception>
        internal TarLzmaExtractor(Stream stream, string target)
            : base(GetDecompressionStream(stream), target)
        {
            _stream = stream;
            UnitsTotal = stream.Length;
        }

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
        private static Stream GetDecompressionStream(Stream stream, int bufferSize = 128 * 1024)
        {
            var bufferStream = new CircularBufferStream(bufferSize);
            var decoder = new Decoder();

            // Read LZMA header
            if (stream.CanSeek) stream.Position = 0;
            var properties = new byte[5];
            if (stream.Read(properties, 0, 5) != 5)
            { // Stream too short
                throw new IOException(Resources.ArchiveInvalid);
            }
            try
            {
                decoder.SetDecoderProperties(properties);
            }
                #region Error handling
            catch (ApplicationException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            #endregion

            // Detmerine uncompressed length
            long uncompressedLength = 0;
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < 8; i++)
                {
                    int v = stream.ReadByte();
                    if (v < 0) throw new IOException(Resources.ArchiveInvalid);

                    uncompressedLength |= ((long)(byte)v) << (8 * i);
                }
            }

            // If the uncompressed length is unknown, use original size * 1.5 as an estimate
            unchecked
            {
                bufferStream.SetLength(uncompressedLength == -1 ? stream.Length : (long)(uncompressedLength * 1.5));
            }

            // Initialize the producer thread that will deliver uncompressed data
            var thread = new Thread(() =>
            {
                try
                {
                    decoder.Code(stream, bufferStream, stream.Length, uncompressedLength, null);
                }
                    #region Error handling
                catch (ThreadAbortException)
                {}
                catch (ObjectDisposedException)
                {
                    // If the buffer stream is closed too early the user probably just canceled the extraction process
                }
                catch (ApplicationException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    bufferStream.RelayErrorToReader(new IOException(ex.Message, ex));
                }
                    #endregion

                finally
                {
                    bufferStream.DoneWriting();
                }
            }) {IsBackground = true};
            thread.Start();

            return new DisposeWarpperStream(bufferStream, () =>
            {
                // Stop producer thread when the buffer stream is closed
                thread.Abort();
                thread.Join();

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
