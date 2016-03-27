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
using SevenZip.Sdk;
using SevenZip.Sdk.Compression.Lzma;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Creates a LZMA-compressed TAR archive from a directory. Preserves execuable bits, symlinks, hardlinks and timestamps.
    /// </summary>
    public class TarLzmaGenerator : TarGenerator
    {
        internal TarLzmaGenerator([NotNull] string sourceDirectory, [NotNull] Stream stream)
            : base(sourceDirectory, GetCompressionStream(stream))
        {}

        /// <summary>
        /// Provides a filter for compressing a <see cref="Stream"/> with LZMA.
        /// </summary>
        /// <param name="stream">The underlying <see cref="Stream"/> to write the compressed data to.</param>
        /// <param name="bufferSize">The maximum number of uncompressed bytes to buffer. 32k (the step size of <see cref="SevenZip"/>) is a sensible minimum.</param>
        /// <remarks>
        /// This method internally uses multi-threading and a <see cref="CircularBufferStream"/>.
        /// The <paramref name="stream"/> may be closed with a delay.
        /// </remarks>
        private static Stream GetCompressionStream(Stream stream, int bufferSize = 128 * 1024)
        {
            var bufferStream = new CircularBufferStream(bufferSize);
            var encoder = new Encoder();

            var consumerThread = new Thread(() =>
            {
                try
                {
                    // Write LZMA header
                    encoder.SetCoderProperties(
                        new[] {CoderPropId.DictionarySize, CoderPropId.PosStateBits, CoderPropId.LitContextBits, CoderPropId.LitPosBits, CoderPropId.Algorithm, CoderPropId.NumFastBytes, CoderPropId.MatchFinder, CoderPropId.EndMarker},
                        new object[] {1 << 23, 2, 3, 0, 2, 128, "bt4", true});
                    encoder.WriteCoderProperties(stream);

                    // Write "uncompressed length" header
                    var uncompressedLengthData = BitConverter.GetBytes(TarLzmaExtractor.UnknownSize);
                    if (!BitConverter.IsLittleEndian) Array.Reverse(uncompressedLengthData);
                    stream.Write(uncompressedLengthData);

                    encoder.Code(
                        inStream: bufferStream, outStream: stream,
                        inSize: TarLzmaExtractor.UnknownSize, outSize: TarLzmaExtractor.UnknownSize,
                        progress: null);
                }
                catch (ObjectDisposedException)
                {
                    // If the buffer stream is closed too early the user probably just canceled the compression process
                }
                finally
                {
                    stream.Dispose();
                }
            }) {IsBackground = true};
            consumerThread.Start();

            return new DisposeWarpperStream(bufferStream, disposeHandler: () =>
            {
                bufferStream.DoneWriting();
                consumerThread.Join();
            });
        }
    }
}
