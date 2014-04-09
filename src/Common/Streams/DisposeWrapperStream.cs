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

namespace NanoByte.Common.Streams
{
    /// <summary>
    /// This wrapper stream passes all operations through to an underlying <see cref="Stream"/> without modifying them. An additional delegate is executed before <see cref="Stream.Dispose()"/> is passed through.
    /// </summary>
    public class DisposeWarpperStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly Action _disposeHandler;

        /// <summary>
        /// Creates a new dispose wrapper stream.
        /// </summary>
        /// <param name="baseStream">The underlying <see cref="Stream"/> providing the actual data. Will be disposed.</param>
        /// <param name="disposeHandler">Executed before <paramref name="baseStream"/> is disposed.</param>
        public DisposeWarpperStream(Stream baseStream, Action disposeHandler)
        {
            _baseStream = baseStream;
            _disposeHandler = disposeHandler;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            try
            {
                _disposeHandler();
                _baseStream.Dispose();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #region Pass-through
        /// <inheritdoc/>
        public override bool CanRead { get { return _baseStream.CanRead; } }

        /// <inheritdoc/>
        public override bool CanSeek { get { return _baseStream.CanSeek; } }

        /// <inheritdoc/>
        public override bool CanWrite { get { return _baseStream.CanWrite; } }

        /// <inheritdoc/>
        public override long Length { get { return _baseStream.Length; } }

        /// <inheritdoc/>
        public override long Position { get { return _baseStream.Position; } set { _baseStream.Position = value; } }

        /// <inheritdoc/>
        public override void Flush()
        {
            _baseStream.Flush();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

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
        #endregion
    }
}
