/*
 * Copyright 2006-2012 Bastian Eicher
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

using NUnit.Framework;

namespace Common.Utils
{
    /// <summary>
    /// Contains test methods for <see cref="ArchiveUtils"/>.
    /// </summary>
    [TestFixture]
    public class ArchiveUtilsTest
    {
        /// <summary>
        /// 
        /// </summary>
        [Test(Description = "Ensures ArchiveUtils.GuessMimeType correctly guesses the MIME types for different file names.")]
        public void TestGuessMimeType()
        {
            Assert.AreEqual("application/zip", ArchiveUtils.GuessMimeType("test.zip"));
            Assert.AreEqual("application/vnd.ms-cab-compressed", ArchiveUtils.GuessMimeType("test.cab"));
            Assert.AreEqual("application/x-tar", ArchiveUtils.GuessMimeType("test.tar"));
            Assert.AreEqual("application/x-compressed-tar", ArchiveUtils.GuessMimeType("test.tar.gz"));
            Assert.AreEqual("application/x-compressed-tar", ArchiveUtils.GuessMimeType("test.tgz"));
            Assert.AreEqual("application/x-bzip-compressed-tar", ArchiveUtils.GuessMimeType("test.tar.bz2"));
            Assert.AreEqual("application/x-bzip-compressed-tar", ArchiveUtils.GuessMimeType("test.tbz2"));
            Assert.AreEqual("application/x-bzip-compressed-tar", ArchiveUtils.GuessMimeType("test.tbz"));
            Assert.AreEqual("application/x-lzma-compressed-tar", ArchiveUtils.GuessMimeType("test.tar.lzma"));
            Assert.AreEqual("application/x-deb", ArchiveUtils.GuessMimeType("test.deb"));
            Assert.AreEqual("application/x-rpm", ArchiveUtils.GuessMimeType("test.rpm"));
            Assert.AreEqual("application/x-apple-diskimage", ArchiveUtils.GuessMimeType("test.dmg"));
        }
    }
}
