/*
 * Copyright 2006-2013 Bastian Eicher
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

namespace Common.Streams
{
    /// <summary>
    /// This filter stream provides a view of an underlying <see cref="Stream"/> shifted by a certain number of bytes (the beginning is cut away).
    /// </summary>
    public class OffsetStream : Stream
    {
        #region Variables
        private readonly Stream _baseStream;
        private readonly long _offset;
        #endregion

        #region Properties
        /// <summary>
        /// The underlying <see cref="Stream"/> providing the actual data.
        /// </summary>
        public Stream BaseStream { get { return _baseStream; } }

        /// <summary>
        /// The number of bytes the underlying <see cref="Stream"/> is shifted / how many bytes are cut away at the beginning.
        /// </summary>
        public long Offset { get { return _offset; } }

        /// <inheritdoc/>
        public override bool CanRead { get { return _baseStream.CanRead; } }

        /// <inheritdoc/>
        public override bool CanWrite { get { return _baseStream.CanWrite; } }

        /// <inheritdoc/>
        public override bool CanSeek { get { return true; } }

        /// <inheritdoc/>
        public override long Position { get { return _baseStream.Position - _offset; } set { _baseStream.Position = value + _offset; } }

        /// <inheritdoc/>
        public override long Length { get { return _baseStream.Length - _offset; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new shifting stream.
        /// </summary>
        /// <param name="baseStream">The underlying <see cref="Stream"/> providing the actual data. Will be disposed.</param>
        /// <param name="offset">The number of bytes <paramref name="baseStream"/> is to be shifted / how many bytes are to be cut away at the beginning.</param>
        public OffsetStream(Stream baseStream, long offset)
        {
            #region Sanity checks
            if (baseStream == null) throw new ArgumentNullException("baseStream");
            if (!baseStream.CanSeek) throw new ArgumentException(Resources.MissingStreamSeekSupport, "baseStream");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", Resources.ArgMustBeGreaterThanZero);
            #endregion

            _baseStream = baseStream;
            _offset = offset;

            _baseStream.Position = offset;
        }
        #endregion

        //--------------------//

        #region Operations
        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _baseStream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin) offset += _offset;
            return _baseStream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            _baseStream.SetLength(value + _offset);
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
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion
    }
}
