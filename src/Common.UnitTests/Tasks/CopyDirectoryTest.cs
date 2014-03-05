/*
 * Copyright 2010-2014 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using Common.Utils;
using NUnit.Framework;

namespace Common.Tasks
{
    /// <summary>
    /// Contains test methods for <see cref="CopyDirectory"/>.
    /// </summary>
    [TestFixture]
    public class CopyDirectoryTest
    {
        /// <summary>
        /// Ensures <see cref="CopyDirectory"/> correctly copies a directories from one location to another.
        /// </summary>
        [Test]
        public void Normal()
        {
            string temp1 = CreateCopyTestTempDir();
            string temp2 = FileUtils.GetTempDirectory("unit-tests");
            Directory.Delete(temp2);

            try
            {
                new CopyDirectory(temp1, temp2).Run();
                FileAssert.AreEqual(
                    expected: Path.Combine(Path.Combine(temp1, "subdir"), "file"),
                    actual: Path.Combine(Path.Combine(temp2, "subdir"), "file"));
                Assert.AreEqual(
                    expected: new DateTime(2000, 1, 1),
                    actual: Directory.GetLastWriteTimeUtc(Path.Combine(temp2, "subdir")),
                    message: "Last-write time for copied directory is invalid");
                Assert.AreEqual(
                    expected: new DateTime(2000, 1, 1),
                    actual: File.GetLastWriteTimeUtc(Path.Combine(Path.Combine(temp2, "subdir"), "file")),
                    message: "Last-write time for copied file is invalid");

                Assert.Throws<IOException>(() => new CopyDirectory(temp1, temp2).Run());
            }
            finally
            {
                File.SetAttributes(Path.Combine(Path.Combine(temp1, "subdir"), "file"), FileAttributes.Normal);
                Directory.Delete(temp1, recursive: true);
                File.SetAttributes(Path.Combine(Path.Combine(temp2, "subdir"), "file"), FileAttributes.Normal);
                Directory.Delete(temp2, recursive: true);
            }
        }

        /// <summary>
        /// Ensures <see cref="CopyDirectory"/> correctly detects usage errors.
        /// </summary>
        [Test]
        public void ErrorHandling()
        {
            Assert.Throws<ArgumentException>(() => new CopyDirectory("a", "a"));

            string temp = FileUtils.GetTempDirectory("unit-tests");
            Directory.Delete(temp);
            Assert.Throws<DirectoryNotFoundException>(() => new CopyDirectory(temp, "a").Run());
        }

        /// <summary>
        /// Ensures <see cref="CopyDirectory"/> correctly copies a directories from one location to another without setting directory timestamps.
        /// </summary>
        [Test]
        public void NoDirTimestamp()
        {
            string temp1 = CreateCopyTestTempDir();
            string temp2 = FileUtils.GetTempDirectory("unit-tests");
            Directory.Delete(temp2);

            try
            {
                new CopyDirectory(temp1, temp2, preserveDirectoryTimestamps: false).Run();
                FileAssert.AreEqual(
                    expected: Path.Combine(Path.Combine(temp1, "subdir"), "file"),
                    actual: Path.Combine(Path.Combine(temp2, "subdir"), "file"));
                Assert.AreNotEqual(
                    expected: new DateTime(2000, 1, 1),
                    actual: Directory.GetLastWriteTimeUtc(Path.Combine(temp2, "subdir")),
                    message: "Last-write time for copied directory is invalid");
                Assert.AreEqual(
                    expected: new DateTime(2000, 1, 1),
                    actual: File.GetLastWriteTimeUtc(Path.Combine(Path.Combine(temp2, "subdir"), "file")),
                    message: "Last-write time for copied file is invalid");

                Assert.Throws<IOException>(() => new CopyDirectory(temp1, temp2).Run());
            }
            finally
            {
                File.SetAttributes(Path.Combine(temp1, Path.Combine("subdir", "file")), FileAttributes.Normal);
                Directory.Delete(temp1, recursive: true);
                File.SetAttributes(Path.Combine(temp2, Path.Combine("subdir", "file")), FileAttributes.Normal);
                Directory.Delete(temp2, recursive: true);
            }
        }

        /// <summary>
        /// Ensures <see cref="CopyDirectory"/> correctly copies a directory on top another.
        /// </summary>
        [Test]
        public void Overwrite()
        {
            string temp1 = CreateCopyTestTempDir();

            string temp2 = FileUtils.GetTempDirectory("unit-tests");
            string subdir2 = Path.Combine(temp2, "subdir");
            Directory.CreateDirectory(subdir2);
            File.WriteAllText(Path.Combine(subdir2, "file"), @"B");
            File.SetLastWriteTimeUtc(Path.Combine(subdir2, "file"), new DateTime(2002, 1, 1));
            Directory.SetLastWriteTimeUtc(subdir2, new DateTime(2002, 1, 1));

            try
            {
                new CopyDirectory(temp1, temp2, preserveDirectoryTimestamps: true, overwrite: true).Run();
                FileAssert.AreEqual(
                    expected: Path.Combine(Path.Combine(temp1, "subdir"), "file"),
                    actual: Path.Combine(Path.Combine(temp2, "subdir"), "file"));
                Assert.AreEqual(
                    expected: new DateTime(2000, 1, 1),
                    actual: Directory.GetLastWriteTimeUtc(Path.Combine(temp2, "subdir")),
                    message: "Last-write time for copied directory is invalid");
                Assert.AreEqual(
                    expected: new DateTime(2000, 1, 1),
                    actual: File.GetLastWriteTimeUtc(Path.Combine(Path.Combine(temp2, "subdir"), "file")),
                    message: "Last-write time for copied file is invalid");
            }
            finally
            {
                File.SetAttributes(Path.Combine(temp1, Path.Combine("subdir", "file")), FileAttributes.Normal);
                Directory.Delete(temp1, recursive: true);
                File.SetAttributes(Path.Combine(temp2, Path.Combine("subdir", "file")), FileAttributes.Normal);
                Directory.Delete(temp2, recursive: true);
            }
        }

        private static string CreateCopyTestTempDir()
        {
            string tempPath = FileUtils.GetTempDirectory("unit-tests");
            string subdir1 = Path.Combine(tempPath, "subdir");
            Directory.CreateDirectory(subdir1);
            File.WriteAllText(Path.Combine(subdir1, "file"), @"A");
            File.SetLastWriteTimeUtc(Path.Combine(subdir1, "file"), new DateTime(2000, 1, 1));
            File.SetAttributes(Path.Combine(subdir1, "file"), FileAttributes.ReadOnly);
            Directory.SetLastWriteTimeUtc(subdir1, new DateTime(2000, 1, 1));
            return tempPath;
        }
    }
}