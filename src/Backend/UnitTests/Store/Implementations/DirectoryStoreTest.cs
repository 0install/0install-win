/*
 * Copyright 2010 Roland Leopold Walkling, Bastian Eicher
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
using System.Threading;
using NanoByte.Common.Storage;
using NanoByte.Common.Utils;
using NUnit.Framework;
using ZeroInstall.Services;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Contains test methods for <see cref="DirectoryStore"/>.
    /// </summary>
    [TestFixture]
    public class DirectoryStoreTest
    {
        private MockTaskHandler _handler;
        private TemporaryDirectory _tempDir;
        private DirectoryStore _store;

        [SetUp]
        public void SetUp()
        {
            _handler = new MockTaskHandler();
            _tempDir = new TemporaryDirectory("0install-unit-tests");
            _store = new DirectoryStore(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            _store.Purge(_handler);
            _tempDir.Dispose();
        }

        [Test]
        public void TestContains()
        {
            Directory.CreateDirectory(Path.Combine(_tempDir, "sha256new_123ABC"));
            Assert.IsTrue(_store.Contains(new ManifestDigest(sha256New: "123ABC")));
            Assert.IsFalse(_store.Contains(new ManifestDigest(sha256New: "456XYZ")));
        }

        [Test]
        public void TestListAll()
        {
            Directory.CreateDirectory(Path.Combine(_tempDir, "sha1=test1"));
            Directory.CreateDirectory(Path.Combine(_tempDir, "sha1new=test2"));
            Directory.CreateDirectory(Path.Combine(_tempDir, "sha256=test3"));
            Directory.CreateDirectory(Path.Combine(_tempDir, "sha256new_test4"));
            Directory.CreateDirectory(Path.Combine(_tempDir, "temp=stuff"));
            CollectionAssert.AreEqual(new[]
            {
                new ManifestDigest(sha1: "test1"),
                new ManifestDigest(sha1New: "test2"),
                new ManifestDigest(sha256: "test3"),
                new ManifestDigest(sha256New: "test4")
            }, _store.ListAll());
        }

        [Test]
        public void TestListAllTemp()
        {
            Directory.CreateDirectory(Path.Combine(_tempDir, "sha1=test"));
            Directory.CreateDirectory(Path.Combine(_tempDir, "temp=stuff"));
            CollectionAssert.AreEqual(new[] {Path.Combine(_tempDir, "temp=stuff")}, _store.ListAllTemp());
        }

        private string DeployPackage(string id, PackageBuilder builder)
        {
            string path = Path.Combine(_tempDir, id);
            builder.WritePackageInto(path);
            ManifestTest.CreateDotFile(path, ManifestFormat.FromPrefix(id), _handler);
            FileUtils.EnableWriteProtection(path);
            return path;
        }

        [Test]
        public void ShouldHardlinkIdenticalFilesInSameImplementation()
        {
            string package1Path = DeployPackage("sha256=1", new PackageBuilder()
                .AddFile("fileA", "abc", new DateTime(2000, 1, 1))
                .AddFolder("dir").AddFile("fileB", "abc", new DateTime(2000, 1, 1)));

            Assert.AreEqual(3, _store.Optimise(_handler));
            Assert.AreEqual(0, _store.Optimise(_handler));
            Assert.IsTrue(FileUtils.AreHardlinked(
                Path.Combine(package1Path, "fileA"),
                Path.Combine(package1Path, "dir", "fileB")));
        }

        [Test]
        public void ShouldHardlinkIdenticalFilesInDifferentImplementations()
        {
            string package1Path = DeployPackage("sha256=1", new PackageBuilder()
                .AddFile("fileA", "abc", new DateTime(2000, 1, 1)));
            string package2Path = DeployPackage("sha256=2", new PackageBuilder()
                .AddFile("fileA", "abc", new DateTime(2000, 1, 1)));

            Assert.AreEqual(3, _store.Optimise(_handler));
            Assert.AreEqual(0, _store.Optimise(_handler));
            Assert.IsTrue(FileUtils.AreHardlinked(
                Path.Combine(package1Path, "fileA"),
                Path.Combine(package2Path, "fileA")));
        }

        [Test]
        public void ShouldNotHardlinkWithDifferentTimestamps()
        {
            string package1Path = DeployPackage("sha256=1", new PackageBuilder()
                .AddFile("fileA", "abc", new DateTime(2000, 1, 1))
                .AddFile("fileX", "abc", new DateTime(2000, 2, 2)));

            Assert.AreEqual(0, _store.Optimise(_handler));
            Assert.IsFalse(FileUtils.AreHardlinked(
                Path.Combine(package1Path, "fileA"),
                Path.Combine(package1Path, "fileX")));
        }

        [Test]
        public void ShouldNotHardlinkDifferentFiles()
        {
            string package1Path = DeployPackage("sha256=1", new PackageBuilder()
                .AddFolder("dir").AddFile("fileB", "abc", new DateTime(2000, 1, 1)));
            string package2Path = DeployPackage("sha256=2", new PackageBuilder()
                .AddFolder("dir").AddFile("fileB", "def", new DateTime(2000, 1, 1)));

            Assert.AreEqual(0, _store.Optimise(_handler));
            Assert.IsFalse(FileUtils.AreHardlinked(
                Path.Combine(package1Path, "dir", "fileB"),
                Path.Combine(package2Path, "dir", "fileB")));
        }

        [Test]
        public void ShouldNotHardlinkAcrossManifestFormatBorders()
        {
            string package1Path = DeployPackage("sha256=1", new PackageBuilder()
                .AddFile("fileA", "abc", new DateTime(2000, 1, 1)));
            string package2Path = DeployPackage("sha256new_2", new PackageBuilder()
                .AddFile("fileA", "abc", new DateTime(2000, 1, 1)));

            Assert.AreEqual(0, _store.Optimise(_handler));
            Assert.IsFalse(FileUtils.AreHardlinked(
                Path.Combine(package1Path, "fileA"),
                Path.Combine(package2Path, "fileA")));
        }

        [Test]
        public void ShouldAllowToAddFolder()
        {
            using (var packageDir = new TemporaryDirectory("0install-unit-tests"))
            {
                var digest = new ManifestDigest(ManifestTest.CreateDotFile(packageDir, ManifestFormat.Sha256, _handler));
                _store.AddDirectory(packageDir, digest, _handler);

                Assert.IsTrue(_store.Contains(digest), "After adding, Store must contain the added package");
                CollectionAssert.AreEqual(new[] {digest}, _store.ListAll(), "After adding, Store must show the added package in the complete list");
            }
        }

        [Test]
        public void ShouldRecreateMissingStoreDir()
        {
            Directory.Delete(_tempDir, recursive: true);

            using (var packageDir = new TemporaryDirectory("0install-unit-tests"))
            {
                var digest = new ManifestDigest(ManifestTest.CreateDotFile(packageDir, ManifestFormat.Sha256, _handler));
                _store.AddDirectory(packageDir, digest, _handler);

                Assert.IsTrue(_store.Contains(digest), "After adding, Store must contain the added package");
                CollectionAssert.AreEqual(new[] {digest}, _store.ListAll(), "After adding, Store must show the added package in the complete list");

                Assert.IsTrue(Directory.Exists(_tempDir), "Store directory should have been recreated");
            }
        }

        [Test]
        public void ShouldHandleRelativePaths()
        {
            // Change the working directory
            string oldWorkingDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = _tempDir;

            try
            {
                _store = new DirectoryStore(".");
                ShouldAllowToAddFolder();
            }
            finally
            {
                // Restore the original working directory
                Environment.CurrentDirectory = oldWorkingDir;
            }
        }

        [Test]
        public void ShouldAllowToRemove()
        {
            string implPath = Path.Combine(_tempDir, "sha256new_123ABC");
            Directory.CreateDirectory(implPath);

            _store.Remove(new ManifestDigest(sha256New: "123ABC"));
            Assert.IsFalse(Directory.Exists(implPath), "After remove, Store may no longer contain the added package");
        }

        [Test]
        public void ShouldReturnCorrectPathOfPackageInCache()
        {
            string implPath = Path.Combine(_tempDir, "sha256new_123ABC");
            Directory.CreateDirectory(implPath);
            Assert.AreEqual(implPath, _store.GetPath(new ManifestDigest(sha256New: "123ABC")), "Store must return the correct path for Implementations it contains");
        }

        [Test]
        public void ShouldThrowWhenRequestedPathOfUncontainedPackage()
        {
            Assert.IsNull(_store.GetPath(new ManifestDigest(sha256: "123")));
        }

        [Test]
        public void TestAuditPass()
        {
            using (var packageDir = new TemporaryDirectory("0install-unit-tests"))
            {
                new PackageBuilder().AddFolder("subdir")
                    .AddFile("file", "AAA", new DateTime(2000, 1, 1))
                    .WritePackageInto(packageDir);
                var digest = new ManifestDigest(ManifestTest.CreateDotFile(packageDir, ManifestFormat.Sha1New, _handler));
                _store.AddDirectory(packageDir, digest, _handler);

                _store.Verify(digest, _handler);
                Assert.IsNull(_handler.LastQuestion);
            }
        }

        [Test]
        public void TestAuditFail()
        {
            Directory.CreateDirectory(Path.Combine(_tempDir, "sha1new=abc"));
            Assert.IsTrue(_store.Contains(new ManifestDigest(sha1New: "abc")));

            _handler.AnswerQuestionWith = true;
            _store.Verify(new ManifestDigest(sha1New: "abc"), _handler);
            Assert.AreEqual(
                expected: string.Format(Resources.ImplementationDamaged + Environment.NewLine + Resources.ImplementationDamagedAskRemove, "sha1new=abc"),
                actual: _handler.LastQuestion);

            Assert.IsFalse(_store.Contains(new ManifestDigest(sha1New: "abc")));
        }

        [Test]
        public void StressTest()
        {
            using (var packageDir = new TemporaryDirectory("0install-unit-tests"))
            {
                new PackageBuilder().AddFolder("subdir")
                    .AddFile("file", "AAA", new DateTime(2000, 1, 1))
                    .WritePackageInto(packageDir);

                var digest = new ManifestDigest(ManifestTest.CreateDotFile(packageDir, ManifestFormat.Sha256, _handler));

                Exception exception = null;
                var threads = new Thread[100];
                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i] = new Thread(() =>
                    {
                        try
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            _store.AddDirectory(packageDir, digest, _handler);
                            _store.Remove(digest);
                        }
                        catch (ImplementationAlreadyInStoreException)
                        {}
                        catch (ImplementationNotFoundException)
                        {}
                        catch (Exception ex)
                        {
                            exception = ex;
                        }
                    });
                    threads[i].Start();
                }

                foreach (var thread in threads)
                    thread.Join();
                if (exception != null)
                    Assert.Fail(exception.ToString());

                Assert.IsFalse(_store.Contains(digest));
            }
        }
    }
}
