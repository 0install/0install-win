/*
 * Copyright 2006-2014 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Text;
using Moq;
using NanoByte.Common.Storage;
using NUnit.Framework;

namespace NanoByte.Common.Utils
{
    /// <summary>
    /// Contains test methods for <see cref="FileUtils"/>.
    /// </summary>
    [TestFixture]
    public class FileUtilsTest
    {
        #region Paths
        [Test]
        public void TestUnifySlashes()
        {
            Assert.AreEqual("a" + Path.DirectorySeparatorChar + "b", FileUtils.UnifySlashes("a/b"));
        }

        [Test]
        public void TestIsBreakoutPath()
        {
            Assert.IsTrue(FileUtils.IsBreakoutPath(WindowsUtils.IsWindows ? @"C:\test" : "/test"), "Should detect absolute paths");

            foreach (string path in new[] {"..", "/..", "../", "/../", "a/../b", "../a", "a/.."})
                Assert.IsTrue(FileUtils.IsBreakoutPath(path), "Should detect parent directory references");

            foreach (string path in new[] {"..a", "a/..a", "a..", "a/a.."})
                Assert.IsFalse(FileUtils.IsBreakoutPath(path), "Should not trip on '..' as a part of file/directory names");

            Assert.IsFalse(FileUtils.IsBreakoutPath(""));
        }

        [Test]
        public void TestRelativeTo()
        {
            if (WindowsUtils.IsWindows)
            {
                Assert.AreEqual("a/b", new DirectoryInfo(@"C:\test\a\b").RelativeTo(new DirectoryInfo(@"C:\test")));
                Assert.AreEqual("a/b", new DirectoryInfo(@"C:\test\a\b").RelativeTo(new DirectoryInfo(@"C:\test\")));
            }
            else
            {
                Assert.AreEqual("a/b", new DirectoryInfo("/test/a/b").RelativeTo(new DirectoryInfo("/test")));
                Assert.AreEqual("a/b", new DirectoryInfo("/test/a/b").RelativeTo(new DirectoryInfo("/test/")));
            }
        }
        #endregion

        #region Time
        /// <summary>
        /// Ensures <see cref="FileUtils.ToUnixTime"/> correctly converts a <see cref="DateTime"/> value to a Unix epoch value.
        /// </summary>
        [Test]
        public void TestToUnixTime()
        {
            // 12677 days = 12677 x 86400 seconds = 1095292800 seconds
            Assert.AreEqual(1095292800, new DateTime(2004, 09, 16).ToUnixTime());
        }

        /// <summary>
        /// Ensures <see cref="FileUtils.FromUnixTime"/> correctly converts a Unix epoch value to a <see cref="DateTime"/> value.
        /// </summary>
        [Test]
        public void TestFromUnixTime()
        {
            // 12677 days = 12677 x 86400 seconds = 1095292800 seconds
            Assert.AreEqual(new DateTime(2004, 09, 16), FileUtils.FromUnixTime(1095292800));
        }
        #endregion

        #region Exists
        /// <summary>
        /// Ensures <see cref="FileUtils.ExistsCaseSensitive"/> correctly detects case mismatches.
        /// </summary>
        [Test]
        public void TestExistsCaseSensitive()
        {
            using (var tempDir = new TemporaryDirectory("unit-tests"))
            {
                File.WriteAllText(Path.Combine(tempDir, "test"), "");
                Assert.IsTrue(FileUtils.ExistsCaseSensitive(Path.Combine(tempDir, "test")));
                Assert.IsFalse(FileUtils.ExistsCaseSensitive(Path.Combine(tempDir, "Test")));
            }
        }
        #endregion

        #region Temp
        /// <summary>
        /// Creates a temporary fileusing <see cref="FileUtils.GetTempFile"/>, ensures it is empty and deletes it again.
        /// </summary>
        [Test]
        public void TestGetTempFile()
        {
            string path = FileUtils.GetTempFile("unit-tests");
            Assert.IsNotNullOrEmpty(path);
            Assert.IsTrue(File.Exists(path));
            Assert.AreEqual("", File.ReadAllText(path));
            File.Delete(path);
        }

        /// <summary>
        /// Creates a temporary directory using <see cref="FileUtils.GetTempDirectory"/>, ensures it is empty and deletes it again.
        /// </summary>
        [Test]
        public void TestGetTempDirectory()
        {
            string path = FileUtils.GetTempDirectory("unit-tests");
            Assert.IsNotNullOrEmpty(path);
            Assert.IsTrue(Directory.Exists(path));
            Assert.IsEmpty(Directory.GetFileSystemEntries(path));
            Directory.Delete(path);
        }
        #endregion

        #region Replace
        /// <summary>
        /// Ensures <see cref="FileUtils.Replace"/> correctly replaces the content of one file with that of another.
        /// </summary>
        [Test]
        public void TestReplace()
        {
            using (var tempDir = new TemporaryDirectory("unit-tests"))
            {
                string sourcePath = Path.Combine(tempDir, "source");
                string targetPath = Path.Combine(tempDir, "target");

                File.WriteAllText(sourcePath, @"source");
                File.WriteAllText(targetPath, @"target");
                FileUtils.Replace(sourcePath, targetPath);
                Assert.AreEqual("source", File.ReadAllText(targetPath));
            }
        }

        /// <summary>
        /// Ensures <see cref="FileUtils.Replace"/> correctly handles a missing destination file (simply move).
        /// </summary>
        [Test]
        public void TestReplaceMissing()
        {
            using (var tempDir = new TemporaryDirectory("unit-tests"))
            {
                string sourcePath = Path.Combine(tempDir, "source");
                string targetPath = Path.Combine(tempDir, "target");

                File.WriteAllText(sourcePath, @"source");
                FileUtils.Replace(sourcePath, targetPath);
                Assert.AreEqual("source", File.ReadAllText(targetPath));
            }
        }
        #endregion

        #region Read
        [Test]
        public void TestReadFirstline()
        {
            using (var tempFile = new TemporaryFile("unit-tests"))
            {
                File.WriteAllText(tempFile, "line1\nline2");
                Assert.AreEqual("line1", new FileInfo(tempFile).ReadFirstLine(Encoding.ASCII));
            }
        }
        #endregion

        #region Directory walking
        // Interfaces used for mocking delegates
        // ReSharper disable once MemberCanBePrivate.Global
        public interface IActionSimulator<in T>
        {
            void Execute(T obj);
        }

        [Test]
        public void TestWalk()
        {
            using (var tempDir = new TemporaryDirectory("unit-tests"))
            {
                string subDirPath = Path.Combine(tempDir, "subdir");
                Directory.CreateDirectory(subDirPath);
                string filePath = Path.Combine(subDirPath, "file");
                File.WriteAllText(filePath, "");

                // Set up delegate mocks
                var dirCallbackMock = new Mock<IActionSimulator<string>>(MockBehavior.Strict);
                // ReSharper disable AccessToDisposedClosure
                dirCallbackMock.Setup(x => x.Execute(tempDir)).Verifiable();
                // ReSharper restore AccessToDisposedClosure
                dirCallbackMock.Setup(x => x.Execute(subDirPath)).Verifiable();
                var fileCallbackMock = new Mock<IActionSimulator<string>>(MockBehavior.Strict);
                fileCallbackMock.Setup(x => x.Execute(filePath)).Verifiable();

                new DirectoryInfo(tempDir).Walk(
                    dir => dirCallbackMock.Object.Execute(dir.FullName),
                    file => fileCallbackMock.Object.Execute(file.FullName));

                dirCallbackMock.Verify();
                fileCallbackMock.Verify();
            }
        }
        #endregion

        #region Links
        [Test]
        public void TestCreateSymlinkPosixFile()
        {
            if (!UnixUtils.IsUnix) Assert.Ignore("Can only test POSIX symlinks on Unixoid system");

            using (var tempDir = new TemporaryDirectory("unit-tests"))
            {
                File.WriteAllText(Path.Combine(tempDir, "target"), @"data");
                string sourcePath = Path.Combine(tempDir, "symlink");
                FileUtils.CreateSymlink(sourcePath, "target");

                Assert.IsTrue(File.Exists(sourcePath), "Symlink should look like file");
                Assert.AreEqual("data", File.ReadAllText(sourcePath), "Symlinked file contents should be equal");

                string target;
                Assert.IsTrue(FileUtils.IsSymlink(sourcePath, out target), "Should detect symlink as such");
                Assert.AreEqual(target, "target", "Should retrieve relative link target");
                Assert.IsFalse(FileUtils.IsRegularFile(sourcePath), "Should not detect symlink as regular file");
            }
        }

        [Test]
        public void TestCreateSymlinkPosixDirectory()
        {
            if (!UnixUtils.IsUnix) Assert.Ignore("Can only test POSIX symlinks on Unixoid system");

            using (var tempDir = new TemporaryDirectory("unit-tests"))
            {
                Directory.CreateDirectory(Path.Combine(tempDir, "target"));
                string sourcePath = Path.Combine(tempDir, "symlink");
                FileUtils.CreateSymlink(sourcePath, "target");

                Assert.IsTrue(Directory.Exists(sourcePath), "Symlink should look like directory");

                string contents;
                Assert.IsTrue(FileUtils.IsSymlink(sourcePath, out contents), "Should detect symlink as such");
                Assert.AreEqual(contents, "target", "Should retrieve relative link target");
            }
        }

        [Test]
        public void TestCreateSymlinkNtfsFile()
        {
            if (!WindowsUtils.IsWindowsVista) Assert.Ignore("Can only test NTFS symlinks on Windows Vista or newer");
            if (!WindowsUtils.IsAdministrator) Assert.Ignore("Can only test NTFS symlinks with Administrator privileges");

            using (var tempDir = new TemporaryDirectory("unit-tests"))
            {
                File.WriteAllText(Path.Combine(tempDir, "target"), @"data");
                string sourcePath = Path.Combine(tempDir, "symlink");
                FileUtils.CreateSymlink(sourcePath, "target");

                Assert.IsTrue(File.Exists(sourcePath), "Symlink should look like file");
                Assert.AreEqual("data", File.ReadAllText(sourcePath), "Symlinked file contents should be equal");
            }
        }

        [Test]
        public void TestCreateSymlinkNtfsDirectory()
        {
            if (!WindowsUtils.IsWindowsVista) Assert.Ignore("Can only test NTFS symlinks on Windows Vista or newer");
            if (!WindowsUtils.IsAdministrator) Assert.Ignore("Can only test NTFS symlinks with Administrator privileges");

            using (var tempDir = new TemporaryDirectory("unit-tests"))
            {
                Directory.CreateDirectory(Path.Combine(tempDir, "target"));
                string sourcePath = Path.Combine(tempDir, "symlink");
                FileUtils.CreateSymlink(sourcePath, "target");

                Assert.IsTrue(Directory.Exists(sourcePath), "Symlink should look like directory");
            }
        }

        [Test]
        public void TestCreateHardlink()
        {
            using (var tempDir = new TemporaryDirectory("unit-tests"))
            {
                string sourcePath = Path.Combine(tempDir, "hardlink");
                string destinationPath = Path.Combine(tempDir, "target");
                string copyPath = Path.Combine(tempDir, "copy");

                File.WriteAllText(destinationPath, @"data");
                FileUtils.CreateHardlink(sourcePath, destinationPath);
                Assert.IsTrue(File.Exists(sourcePath), "Hardlink should look like regular file");
                Assert.AreEqual("data", File.ReadAllText(sourcePath), "Hardlinked file contents should be equal");
                Assert.IsTrue(FileUtils.AreHardlinked(sourcePath, destinationPath));

                File.Copy(sourcePath, copyPath);
                Assert.IsFalse(FileUtils.AreHardlinked(sourcePath, copyPath));
            }
        }
        #endregion

        #region Unix
        [Test]
        public void TestIsRegularFile()
        {
            using (var tempFile = new TemporaryFile("unit-tests"))
                Assert.IsTrue(FileUtils.IsRegularFile(tempFile), "Regular file should be detected as such");
        }

        [Test]
        public void TestIsSymlink()
        {
            using (var tempFile = new TemporaryFile("unit-tests"))
            {
                string contents;
                Assert.IsFalse(FileUtils.IsSymlink(tempFile, out contents), "File was incorrectly identified as symlink");
                Assert.IsNull(contents);
            }
        }

        [Test]
        public void TestIsExecutable()
        {
            using (var tempFile = new TemporaryFile("unit-tests"))
                Assert.IsFalse(FileUtils.IsExecutable(tempFile), "File was incorrectly identified as executable");
        }

        [Test]
        public void TestSetExecutable()
        {
            if (!UnixUtils.IsUnix) Assert.Ignore("Can only test executable bits on Unixoid system");

            using (var tempFile = new TemporaryFile("unit-tests"))
            {
                Assert.IsFalse(FileUtils.IsExecutable(tempFile), "File should not be executable yet");

                FileUtils.SetExecutable(tempFile, true);
                Assert.IsTrue(FileUtils.IsExecutable(tempFile), "File should now be executable");
                Assert.IsTrue(FileUtils.IsRegularFile(tempFile), "File should still be considered a regular file");

                FileUtils.SetExecutable(tempFile, false);
                Assert.IsFalse(FileUtils.IsExecutable(tempFile), "File should no longer be executable");
            }
        }
        #endregion
    }
}
