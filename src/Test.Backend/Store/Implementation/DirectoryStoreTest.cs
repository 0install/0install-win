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
using System.Linq;
using System.Threading;
using Common.Utils;
using Common.Storage;
using NUnit.Framework;
using ZeroInstall.Injector;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Contains test methods for <see cref="DirectoryStore"/>.
    /// </summary>
    [TestFixture]
    public class DirectoryStoreTest
    {
        #region Helpers
        /// <summary>
        /// Creates a temporary directory containing exactly one file named "file.txt" containing 3 ASCII-encoded capital As.
        /// </summary>
        /// <returns>The path of the directory</returns>
        internal static string CreateArtificialPackage()
        {
            string packageDir = FileUtils.GetTempDirectory("0install-unit-tests");
            string subDir = Path.Combine(packageDir, "subdir");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "file.txt"), @"AAA");
            File.SetLastWriteTimeUtc(Path.Combine(subDir, "file.txt"), new DateTime(2000, 1, 1));

            return packageDir;
        }
        #endregion

        private TemporaryDirectory _tempDir;
        private DirectoryStore _store;
        private string _packageDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = new TemporaryDirectory("0install-unit-tests");
            _store = new DirectoryStore(_tempDir.Path);
            _packageDir = CreateArtificialPackage();
        }

        [TearDown]
        public void TearDown()
        {
            _tempDir.Dispose();
            if (Directory.Exists(_packageDir)) Directory.Delete(_packageDir, true);
        }

        [Test]
        public void TestListAll()
        {
            Directory.CreateDirectory(Path.Combine(_tempDir.Path, "sha1=test1"));
            Directory.CreateDirectory(Path.Combine(_tempDir.Path, "sha1new=test2"));
            Directory.CreateDirectory(Path.Combine(_tempDir.Path, "sha256=test3"));
            Directory.CreateDirectory(Path.Combine(_tempDir.Path, "sha256new_test4"));
            Directory.CreateDirectory(Path.Combine(_tempDir.Path, "temp=stuff"));
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
            Directory.CreateDirectory(Path.Combine(_tempDir.Path, "sha1=test"));
            Directory.CreateDirectory(Path.Combine(_tempDir.Path, "temp=stuff"));
            CollectionAssert.AreEqual(new[] {"temp=stuff"}, _store.ListAllTemp());
        }

        [Test]
        public void ShouldTellIfItContainsAnImplementation()
        {
            string hash = Manifest.CreateDotFile(_packageDir, ManifestFormat.Sha256, new SilentHandler());

            Directory.Move(_packageDir, Path.Combine(_store.DirectoryPath, hash));
            Assert.True(_store.Contains(new ManifestDigest(hash)));
        }

        [Test]
        public void ShouldAllowToAddFolder()
        {
            var digest = new ManifestDigest(Manifest.CreateDotFile(_packageDir, ManifestFormat.Sha256, new SilentHandler()));
            _store.AddDirectory(_packageDir, digest, new SilentHandler());

            Assert.IsTrue(_store.Contains(digest), "After adding, Store must contain the added package");
            CollectionAssert.AreEqual(new[] {digest}, _store.ListAll(), "After adding, Store must show the added package in the complete list");
        }

        [Test]
        public void ShouldRecreateMissingStoreDir()
        {
            Directory.Delete(_tempDir.Path, true);

            var digest = new ManifestDigest(Manifest.CreateDotFile(_packageDir, ManifestFormat.Sha256, new SilentHandler()));
            _store.AddDirectory(_packageDir, digest, new SilentHandler());

            Assert.IsTrue(_store.Contains(digest), "After adding, Store must contain the added package");
            CollectionAssert.AreEqual(new[] {digest}, _store.ListAll(), "After adding, Store must show the added package in the complete list");

            Assert.IsTrue(Directory.Exists(_tempDir.Path), "Store directory should have been recreated");
        }

        [Test]
        public void ShouldHandleRelativePaths()
        {
            // Change the working directory
            string oldWorkingDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = _tempDir.Path;
            _store = new DirectoryStore(".");

            ShouldAllowToAddFolder();

            // Restore the original working directory
            Environment.CurrentDirectory = oldWorkingDir;
        }

        [Test]
        public void ShouldAllowToRemove()
        {
            var digest = new ManifestDigest(Manifest.CreateDotFile(_packageDir, ManifestFormat.Sha256, new SilentHandler()));

            _store.AddDirectory(_packageDir, digest, new SilentHandler());
            Assert.IsTrue(_store.Contains(digest), "After adding, Store must contain the added package");
            _store.Remove(digest);
            Assert.IsFalse(_store.Contains(digest), "After remove, Store may no longer contain the added package");
        }

        [Test]
        public void ShouldAllowToRemoveTemp()
        {
            Directory.CreateDirectory(Path.Combine(_store.DirectoryPath, "temp"));
            Assert.IsTrue(_store.Contains("temp"), "After adding, Store must list the temp directory");
            _store.Remove("temp");
            Assert.IsFalse(_store.Contains("temp"), "After remove, Store may no longer list the temp directory");
        }

        [Test]
        public void ShouldThrowOnAddWithEmptyDigest()
        {
            Assert.Throws<ArgumentException>(() => _store.AddDirectory(_packageDir, new ManifestDigest(), new SilentHandler()));
        }

        [Test]
        public void ShouldReturnCorrectPathOfPackageInCache()
        {
            string hash = Manifest.CreateDotFile(_packageDir, ManifestFormat.Sha256, new SilentHandler());

            Directory.Move(_packageDir, Path.Combine(_store.DirectoryPath, hash));
            Assert.AreEqual(Path.Combine(_store.DirectoryPath, hash), _store.GetPath(new ManifestDigest(hash)), "Store must return the correct path for Implementations it contains");
        }

        [Test]
        public void ShouldThrowWhenRequestedPathOfUncontainedPackage()
        {
            Assert.IsNull(_store.GetPath(new ManifestDigest(sha256: "123")));
        }

        [Test]
        public void ShouldDetectDamagedImplementations()
        {
            var digest = new ManifestDigest(Manifest.CreateDotFile(_packageDir, ManifestFormat.Sha1New, new SilentHandler()));
            _store.AddDirectory(_packageDir, digest, new SilentHandler());

            // After correctly adding a directory, the store should be valid
            CollectionAssert.IsEmpty(_store.Audit(new SilentHandler()));

            // A contaminated store should be detected
            Directory.CreateDirectory(Path.Combine(_tempDir.Path, "sha1new=abc"));
            DigestMismatchException problem = _store.Audit(new SilentHandler()).First();
            Assert.AreEqual("sha1new=abc", problem.ExpectedHash);
            Assert.AreEqual("sha1new=da39a3ee5e6b4b0d3255bfef95601890afd80709", problem.ActualHash);
        }

        [Test]
        public void StressTest()
        {
            var digest = new ManifestDigest(Manifest.CreateDotFile(_packageDir, ManifestFormat.Sha256, new SilentHandler()));

            Exception exception = null;
            var threads = new Thread[100];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(delegate()
                {
                    try
                    {
                        _store.AddDirectory(_packageDir, digest, new SilentHandler());
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
