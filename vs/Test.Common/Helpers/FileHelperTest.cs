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
using System.Security.Cryptography;
using NUnit.Framework;

namespace Common.Helpers
{
    /// <summary>
    /// Contains test methods for <see cref="FileHelper"/>.
    /// </summary>
    [TestFixture]
    public class FileHelperTest
    {
        /// <summary>
        /// Ensures <see cref="FileHelper.ComputeHash"/> can correctly hash files using SHA1.
        /// </summary>
        [Test]
        public void TestComputeHash()
        {
            const string sha1ForEmptyString = "da39a3ee5e6b4b0d3255bfef95601890afd80709";

            string tempFile = null;
            try
            {
                // Create and hash an empty file
                tempFile = Path.GetTempFileName();
                Assert.AreEqual(sha1ForEmptyString, FileHelper.ComputeHash(tempFile, SHA1.Create()));
            }
            finally
            { // Clean up
                if (tempFile != null) File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Creates a temporary directory using <see cref="FileHelper.GetTempDirectory"/>, ensures it is empty and deletes it again.
        /// </summary>
        [Test]
        public void TestGetTempDirectory()
        {
            string path = FileHelper.GetTempDirectory();
            Assert.IsNotNullOrEmpty(path);
            Assert.IsTrue(Directory.Exists(path));
            Assert.IsEmpty(Directory.GetFileSystemEntries(path));
            Directory.Delete(path);
        }

        /// <summary>
        /// Generates a unique file name using <see cref="FileHelper.GetUniqueFileName"/> and ensures it doesn't exist yet.
        /// </summary>
        [Test]
        public void TestGetUniqueFileName()
        {
            string path = FileHelper.GetUniqueFileName(Path.GetTempPath());
            Assert.IsFalse(File.Exists(path));
            Assert.IsFalse(Directory.Exists(path));
        }

        /// <summary>
        /// Ensures <see cref="FileHelper.UnixTime"/> correctly converts a <see cref="DateTime"/> value to a Unix epoch value.
        /// </summary>
        [Test]
        public void TestUnixTime()
        {
            // 12677 days = 12677 x 86400 seconds = 1095292800 seconds
            Assert.AreEqual(1095292800, FileHelper.UnixTime(new DateTime(2004, 09, 16)));
        }
    }
}
