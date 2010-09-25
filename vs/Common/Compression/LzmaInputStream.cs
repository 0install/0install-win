/*
 * Copyright 2006-2010 Bastian Eicher
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
using Common.Properties;
using SevenZip;
using SevenZip.Compression.LZMA;

namespace Common.Compression
{
    /// <summary>
    /// This filter stream is used to decompress a LZMA format stream. 
    /// </summary>
    public class LzmaInputStream : Stream
    {
        #region Variables
        private readonly Stream _baseStream;
        private readonly Stream _memoryStream = new MemoryStream();
        #endregion

        #region Properties
        /// <summary>
        /// The underlying <see cref="Stream"/> providing the compressed data.
        /// </summary>
        public Stream BaseStream { get { return _baseStream; } }

        /// <summary>Returns <see langword="true"/> to indicate this stream can be read.</summary>
        public override bool CanRead { get { return true; } }

        /// <summary>Returns <see langword="false"/> to indicate this stream is read-only.</summary>
        public override bool CanWrite { get { return false; } }

        /// <summary>Returns <see langword="true"/> to indicate this stream can seek.</summary>
        public override bool CanSeek { get { return true; } }

        /// <summary>
        /// Gets or sets the position within the uncompressed data stream.
        /// </summary>
        public override long Position { get { return _memoryStream.Position; } set { _memoryStream.Position = value; } }

        /// <summary>
        /// The length of the uncompressed data stream.
        /// </summary>
        public override long Length { get { return _memoryStream.Length; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new LZMA decompression stream.
        /// </summary>
        /// <param name="baseStream">The underlying <see cref="Stream"/> providing the compressed data. Will be disposed.</param>
        /// <exception cref="InvalidParamException">Thrown if the <paramref name="baseStream"/> doesn't start with a valid 5-bit LZMA header.</exception>
        public LzmaInputStream(Stream baseStream)
        {
            #region Sanity checks
            if (baseStream == null) throw new ArgumentNullException("baseStream");
            #endregion

            _baseStream = baseStream;

             var decoder = new Decoder();

            // Read LZMA header
            if (baseStream.CanSeek) baseStream.Position = 0;
            var properties = new byte[5];
            if (baseStream.Read(properties, 0, 5) != 5) throw new InvalidParamException();
            decoder.SetDecoderProperties(properties);

            // Detmerine uncompressed length
            long uncompressedLength = 0;
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < 8; i++)
                {
                    int v = baseStream.ReadByte();
                    if (v < 0) throw (new InvalidParamException());

                    uncompressedLength |= ((long)(byte)v) << (8 * i);
                }
            }

            // Extract everything into memory
            // ToDo: Replace with real on-demand stream semantics
            try { decoder.Code(_baseStream, _memoryStream, _baseStream.Length, uncompressedLength, null); }
            #region Error handling
            catch (DataErrorException ex)
            {
                // Make sure only standard exception types are thrown to the outside
                throw new IOException(Resources.ArchiveInvalid, ex);
            }
            #endregion
            _memoryStream.Position = 0;
        }
        #endregion

        //--------------------//

        #region Operations
        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _memoryStream.Read(buffer, offset, count);
        }

        /// <summary>Not supported.</summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>Not supported.</summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _memoryStream.Seek(offset, origin);
        }

        /// <summary>Not supported.</summary>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            _baseStream.Flush();
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (_baseStream != null) _baseStream.Dispose();
                if (_memoryStream != null) _memoryStream.Dispose();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion
    }
}
