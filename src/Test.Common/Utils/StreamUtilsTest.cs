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

using System.IO;
using System.Text;
using Common.Storage;
using NUnit.Framework;

namespace Common.Utils
{
    /// <summary>
    /// Contains test methods for <see cref="StreamUtils"/>.
    /// </summary>
    [TestFixture]
    public class StreamUtilsTest
    {
        /// <summary>
        /// Ensures <see cref="StreamUtils.Equals(System.IO.Stream,System.IO.Stream)"/> works correctly.
        /// </summary>
        [Test]
        public void TestEquals()
        {
            Assert.IsTrue(StreamUtils.Equals("abc".ToStream(), "abc".ToStream()));
            Assert.IsFalse(StreamUtils.Equals("ab".ToStream(), "abc".ToStream()));
            Assert.IsFalse(StreamUtils.Equals("abc".ToStream(), "ab".ToStream()));
            Assert.IsFalse(StreamUtils.Equals("abc".ToStream(), "".ToStream()));
        }

        /// <summary>
        /// Ensures <see cref="StreamUtils.ToStream"/> and <see cref="StreamUtils.ReadToString"/> work correctly.
        /// </summary>
        [Test]
        public void TestString()
        {
            const string test = "Test";
            using (var stream = test.ToStream())
                Assert.AreEqual(test, stream.ReadToString());
        }

        /// <summary>
        /// Ensures <see cref="StreamUtils.WriteTo(System.IO.Stream,System.String)"/> correctly writes streams to files.
        /// </summary>
        [Test]
        public void TestWriteToFile()
        {
            using (var tempFile = new TemporaryFile("unit-tests"))
            {
                "abc".ToStream().WriteTo(tempFile);
                Assert.AreEqual("abc", new FileInfo(tempFile).ReadFirstLine(Encoding.UTF8));
            }
        }
    }
}
