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

using System.IO;
using Common.Helpers;
using NUnit.Framework;

namespace Common
{
    /// <summary>
    /// Contains test methods for <see cref="OffsetStream"/>.
    /// </summary>
    [TestFixture]
    public class OffsetStreamTest
    {
        private OffsetStream _stream;

        [SetUp]
        public void SetUp()
        {
            _stream = new OffsetStream(StreamHelper.CreateFromString("abcd"), 1);
        }

        /// <summary>
        /// Tests the <see cref="OffsetStream.Length"/> property and <see cref="OffsetStream.SetLength"/> method.
        /// </summary>
        [Test]
        public void TestLength()
        {
            Assert.AreEqual(3, _stream.Length);
            _stream.SetLength(2);
            Assert.AreEqual(2, _stream.Length);
        }

        /// <summary>
        /// Tests the <see cref="OffsetStream.Position"/> property and <see cref="OffsetStream.Seek"/> method.
        /// </summary>
        [Test]
        public void TestPositionSeek()
        {
            Assert.AreEqual(0, _stream.Position);
            Assert.AreEqual(1, _stream.BaseStream.Position);

            // Seeking forward should start at the offset
            _stream.Seek(1, SeekOrigin.Current);
            Assert.AreEqual(1, _stream.Position);
            Assert.AreEqual(2, _stream.BaseStream.Position);

            // Seeking from the end should work normally
            _stream.Seek(-1, SeekOrigin.End);
            Assert.AreEqual(3, _stream.BaseStream.Position);
            Assert.AreEqual(2, _stream.Position);

            // Seeking to the beginning should translate to byte 1 in original stream
            _stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(0, _stream.Position);
            Assert.AreEqual(1, _stream.BaseStream.Position);
        }

        /// <summary>
        /// Tests the <see cref="OffsetStream.Seek"/> and <see cref="OffsetStream.Read"/> methods.
        /// </summary>
        [Test]
        public void TestRead()
        {
            // Seeking forward should start at the offset
            _stream.Seek(1, SeekOrigin.Current);
            Assert.AreEqual('c', (char)_stream.ReadByte());

            // Seeking from the end should work normally
            _stream.Seek(-1, SeekOrigin.End);
            Assert.AreEqual('d', (char)_stream.ReadByte());
            
            // Seeking to the beginning should translate to byte 1 in original stream
            _stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual('b', (char)_stream.ReadByte());
        }

        /// <summary>
        /// Tests the <see cref="OffsetStream.Seek"/> and <see cref="OffsetStream.Write"/> methods.
        /// </summary>
        [Test]
        public void TestWrite()
        {
            // Write in different places of the stream
            _stream.Seek(1, SeekOrigin.Current);
            _stream.WriteByte((byte)'3');
            _stream.Seek(-1, SeekOrigin.End);
            _stream.WriteByte((byte)'4');
            _stream.Seek(0, SeekOrigin.Begin);
            _stream.WriteByte((byte)'2');

            // Check both offset and base stream reacted correctly
            Assert.AreEqual("234", StreamHelper.ReadToString(_stream));
            Assert.AreEqual("a234", StreamHelper.ReadToString(_stream.BaseStream));
        }
    }
}
