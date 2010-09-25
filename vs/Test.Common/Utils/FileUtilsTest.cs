/*
 * Copyright 2006-2010 Bastian Eicher, Simon E. Silva Lauinger
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

namespace Common.Utils
{
    /// <summary>
    /// Contains test methods for <see cref="FileUtils"/>.
    /// </summary>
    [TestFixture]
    public class FileUtilsTest
    {
        private const string _sha1ForEmptyString = "da39a3ee5e6b4b0d3255bfef95601890afd80709";

        /// <summary>
        /// Ensures <see cref="FileUtils.ComputeHash(string,HashAlgorithm)"/> can correctly hash files using SHA1.
        /// </summary>
        [Test]
        public void TestComputeHashFile()
        {
            string tempFile = null;
            try
            {
                // Create and hash an empty file
                tempFile = Path.GetTempFileName();
                Assert.AreEqual(_sha1ForEmptyString, FileUtils.ComputeHash(tempFile, SHA1.Create()));
            }
            finally
            { // Clean up
                if (tempFile != null) File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Ensures <see cref="FileUtils.ComputeHash(Stream,HashAlgorithm)"/> can correctly hash files using SHA1.
        /// </summary>
        [Test]
        public void TestComputeHashStream()
        {

            string tempFile = null;
            try
            {
                // Create and hash an empty file
                tempFile = Path.GetTempFileName();
                Assert.AreEqual(_sha1ForEmptyString, FileUtils.ComputeHash(new MemoryStream(), SHA1.Create()));
            }
            finally
            { // Clean up
                if (tempFile != null) File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Creates a temporary directory using <see cref="FileUtils.GetTempDirectory"/>, ensures it is empty and deletes it again.
        /// </summary>
        [Test]
        public void TestGetTempDirectory()
        {
            string path = FileUtils.GetTempDirectory();
            Assert.IsNotNullOrEmpty(path);
            Assert.IsTrue(Directory.Exists(path));
            Assert.IsEmpty(Directory.GetFileSystemEntries(path));
            Directory.Delete(path);
        }

        /// <summary>
        /// Ensures <see cref="FileUtils.UnixTime"/> correctly converts a <see cref="DateTime"/> value to a Unix epoch value.
        /// </summary>
        [Test]
        public void TestUnixTime()
        {
            // 12677 days = 12677 x 86400 seconds = 1095292800 seconds
            Assert.AreEqual(1095292800, FileUtils.UnixTime(new DateTime(2004, 09, 16)));
        }

        /// <summary>
        /// Ensures <see cref="FileUtils.CopyDirectory"/> correctly copies a directories from one location to another and detects usage errors.
        /// </summary>
        [Test]
        public void TestCopyDirectory()
        {
            string temp1 = FileUtils.GetTempDirectory();
            string subdir = Path.Combine(temp1, "subdir");
            Directory.CreateDirectory(subdir);
            File.WriteAllText(Path.Combine(subdir, "file"), "A");

            string temp2 = FileUtils.GetTempDirectory();
            Directory.Delete(temp2);
            
            try
            {
                Assert.Throws<ArgumentException>(() => FileUtils.CopyDirectory(temp1, temp1, false));
                Assert.Throws<DirectoryNotFoundException>(() => FileUtils.CopyDirectory(temp2, temp1, false));

                FileUtils.CopyDirectory(temp1, temp2, false);
                FileAssert.AreEqual(Path.Combine(subdir, "file"), Path.Combine(Path.Combine(temp2, "subdir"), "file"));

                Assert.Throws<IOException>(() => FileUtils.CopyDirectory(temp1, temp2, false));
            }
            finally
            {
                Directory.Delete(temp1, true);
                Directory.Delete(temp2, true);
            }
        }

        /// <summary>
        /// Ensures <see cref="FileUtils.CopyDirectory"/> correctly copies a directory on top another.
        /// </summary>
        [Test]
        public void TestCopyDirectoryOverwrite()
        {
            string temp1 = FileUtils.GetTempDirectory();
            string subdir1 = Path.Combine(temp1, "subdir");
            Directory.CreateDirectory(subdir1);
            File.WriteAllText(Path.Combine(subdir1, "file"), @"A");
            File.SetLastWriteTimeUtc(Path.Combine(subdir1, "file"), new DateTime(2000, 1, 1));
            Directory.SetLastWriteTimeUtc(subdir1, new DateTime(2000, 1, 1));

            string temp2 = FileUtils.GetTempDirectory();
            string subdir2 = Path.Combine(temp2, "subdir");
            Directory.CreateDirectory(subdir2);
            File.WriteAllText(Path.Combine(subdir2, "file"), @"B");
            File.SetLastWriteTimeUtc(Path.Combine(subdir2, "file"), new DateTime(2002, 1, 1));
            Directory.SetLastWriteTimeUtc(subdir2, new DateTime(2002, 1, 1));

            try
            {
                FileUtils.CopyDirectory(temp1, temp2, true);
                FileAssert.AreEqual(Path.Combine(subdir1, "file"), Path.Combine(Path.Combine(temp2, "subdir"), "file"));
                Assert.AreEqual(new DateTime(2000, 1, 1), Directory.GetLastWriteTimeUtc(subdir2));
                Assert.AreEqual(new DateTime(2000, 1, 1), File.GetLastWriteTimeUtc(Path.Combine(subdir2, "file")));
            }
            finally
            {
                Directory.Delete(temp1, true);
                Directory.Delete(temp2, true);
            }
        }
    }
}
