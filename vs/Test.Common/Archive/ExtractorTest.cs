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
using Common.Storage;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

namespace Common.Archive
{
    /// <summary>
    /// Contains test methods for <see cref="Extractor"/>.
    /// </summary>
    [TestFixture]
    public class ExtractorTest
    {
        /// <summary>
        /// Ensures <see cref="Extractor.GuessMimeType"/> correctly guesses the MIME types for different file names.
        /// </summary>
        [Test]
        public void TestGuessMimeType()
        {
            Assert.AreEqual("application/zip", Extractor.GuessMimeType("test.zip"));
            Assert.AreEqual("application/vnd.ms-cab-compressed", Extractor.GuessMimeType("test.cab"));
            Assert.AreEqual("application/x-tar", Extractor.GuessMimeType("test.tar"));
            Assert.AreEqual("application/x-compressed-tar", Extractor.GuessMimeType("test.tar.gz"));
            Assert.AreEqual("application/x-compressed-tar", Extractor.GuessMimeType("test.tgz"));
            Assert.AreEqual("application/x-bzip-compressed-tar", Extractor.GuessMimeType("test.tar.bz2"));
            Assert.AreEqual("application/x-bzip-compressed-tar", Extractor.GuessMimeType("test.tbz2"));
            Assert.AreEqual("application/x-bzip-compressed-tar", Extractor.GuessMimeType("test.tbz"));
            Assert.AreEqual("application/x-lzma-compressed-tar", Extractor.GuessMimeType("test.tar.lzma"));
            Assert.AreEqual("application/x-deb", Extractor.GuessMimeType("test.deb"));
            Assert.AreEqual("application/x-rpm", Extractor.GuessMimeType("test.rpm"));
            Assert.AreEqual("application/x-apple-diskimage", Extractor.GuessMimeType("test.dmg"));
        }

        /// <summary>
        /// Ensures <see cref="Extractor.CreateExtractor(string,string,long)"/> correctly creates a <see cref="ZipExtractor"/>.
        /// </summary>
        [Test]
        public void TestCreateExtractor()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                string path = Path.Combine(tempDir.Path, "a.zip");

                using (var file = File.Create(path))
                {
                    using (var zipStream = new ZipOutputStream(file) {IsStreamOwner = false})
                    {
                        var entry = new ZipEntry("file");
                        zipStream.PutNextEntry(entry);
                        zipStream.CloseEntry();
                    }
                }

                using (var extractor = Extractor.CreateExtractor(null, path, 0))
                    Assert.IsInstanceOf(typeof(ZipExtractor), extractor);
            }
        }
    }
}
