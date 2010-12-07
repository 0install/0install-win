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
using Common.Utils;
using Common.Storage;
using NUnit.Framework;
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
            string packageDir = FileUtils.GetTempDirectory();
            string subDir = Path.Combine(packageDir, "subdir");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "file.txt"), @"AAA");

            return packageDir;
        }
        #endregion

        private TemporaryDirectory _tempDir;
        private DirectoryStore _store;
        private string _packageDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = new TemporaryDirectory();
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
        public void ShouldTellIfItContainsAnImplementation()
        {
            string hash = Manifest.CreateDotFile(_packageDir, ManifestFormat.Sha256, null);

            Directory.Move(_packageDir, Path.Combine(_store.DirectoryPath, hash));
            Assert.True(_store.Contains(new ManifestDigest(hash)));
        }

        [Test]
        public void ShouldAllowToAddFolder()
        {
            var digest = new ManifestDigest(Manifest.CreateDotFile(_packageDir, ManifestFormat.Sha256, null));

            var store = _store;
            store.AddDirectory(_packageDir, digest, null);
            Assert.IsTrue(store.Contains(digest), "After adding, Store must contain the added package");
            CollectionAssert.AreEqual(new[] { digest.BestDigest }, store.ListAll(), "After adding, Store must show the added package in the complete list");
        }

        [Test]
        public void ShouldAllowToRemove()
        {
            var digest = new ManifestDigest(Manifest.CreateDotFile(_packageDir, ManifestFormat.Sha256, null));

            var store = _store;
            store.AddDirectory(_packageDir, digest, null);
            Assert.IsTrue(store.Contains(digest), "After adding, Store must contain the added package");
            store.Remove(digest);
            Assert.IsFalse(store.Contains(digest), "After remove, Store may no longer contain the added package");
        }

        [Test]
        public void ShouldThrowOnAddWithEmptyDigest()
        {
            Assert.Throws(typeof(ArgumentException), () => _store.AddDirectory(_packageDir, new ManifestDigest(), null));
        }

        [Test]
        public void ShouldReturnCorrectPathOfPackageInCache()
        {
            string hash = Manifest.CreateDotFile(_packageDir, ManifestFormat.Sha256, null);

            Directory.Move(_packageDir, Path.Combine(_store.DirectoryPath, hash));
            Assert.AreEqual(Path.Combine(_store.DirectoryPath, hash), _store.GetPath(new ManifestDigest(hash)), "Store must return the correct path for Implementations it contains");
        }

        [Test]
        public void ShouldThrowWhenRequestedPathOfUncontainedPackage()
        {
            Assert.Throws(typeof(ImplementationNotFoundException), () => _store.GetPath(new ManifestDigest("sha256=123")));
        }
    }
}