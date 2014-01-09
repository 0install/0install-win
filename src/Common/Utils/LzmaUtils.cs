/*
 * Copyright 2006-2014 Bastian Eicher
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
using System.IO;
using System.Threading;
using Common.Properties;
using Common.Streams;
using SevenZip;
using SevenZip.Compression.LZMA;

namespace Common.Utils
{
    /// <summary>
    /// Provides helper methods for <see cref="Stream"/> access to LZMA-compressed data.
    /// </summary>
    public static class LzmaUtils
    {
        /// <summary>
        /// Provides a filter for decompressing an LZMA encoded <see cref="Stream"/>.
        /// </summary>
        /// <param name="baseStream">The underlying <see cref="Stream"/> providing the compressed data. Will not be disposed.</param>
        /// <param name="bufferSize">The maximum number of uncompressed bytes to buffer. 32k (the step size of <see cref="SevenZip"/>) is a sensible minimum.</param>
        /// <exception cref="InvalidDataException">Thrown if the <paramref name="baseStream"/> doesn't start with a valid 5-bit LZMA header.</exception>
        /// <remarks>
        /// This method internally uses multi-threading and a <see cref="CircularBufferStream"/>.
        /// The <paramref name="baseStream"/> may be closed with a delay.
        /// </remarks>
        public static Stream GetDecompressionStream(Stream baseStream, int bufferSize)
        {
            #region Sanity checks
            if (baseStream == null) throw new ArgumentNullException("baseStream");
            #endregion

            var bufferStream = new CircularBufferStream(bufferSize);

            var decoder = new Decoder();

            // Read LZMA header
            if (baseStream.CanSeek) baseStream.Position = 0;
            var properties = new byte[5];
            if (baseStream.Read(properties, 0, 5) != 5)
            { // Stream too short
                throw new InvalidDataException(Resources.ArchiveInvalid);
            }
            try
            {
                decoder.SetDecoderProperties(properties);
            }
                #region Error handling
            catch (InvalidParamException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new InvalidDataException(Resources.ArchiveInvalid, ex);
            }
            #endregion

            // Detmerine uncompressed length
            long uncompressedLength = 0;
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < 8; i++)
                {
                    int v = baseStream.ReadByte();
                    if (v < 0) throw new InvalidDataException(Resources.ArchiveInvalid);

                    uncompressedLength |= ((long)(byte)v) << (8 * i);
                }
            }

            // If the uncompressed length is unknown, use original size * 1.5 as an estimate
            unchecked
            {
                bufferStream.SetLength(uncompressedLength == -1 ? baseStream.Length : (long)(uncompressedLength * 1.5));
            }

            // Initialize the producer thread that will deliver uncompressed data
            var thread = new Thread(() =>
            {
                try
                {
                    decoder.Code(baseStream, bufferStream, baseStream.Length, uncompressedLength, null);
                }
                    #region Error handling
                catch (ThreadAbortException)
                {}
                catch (ObjectDisposedException)
                {
                    // If the buffer stream is closed too early the user probably just canceled the extraction process
                }
                catch (DataErrorException ex)
                {
                    // Wrap exception since only certain exception types are allowed
                    bufferStream.RelayErrorToReader(new InvalidDataException(ex.Message, ex));
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
            });
        }
    }
}
