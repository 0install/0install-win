/*
 * Copyright 2010 Roland Leopold Walkling
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
    [TestFixture]
    public class StoreCreation
    {
        [Test]
        public void ShouldAcceptAnExistingPath()
        {
            using (var dir = new TemporaryDirectoryReplacement(Path.GetFullPath("test-store")))
                Assert.DoesNotThrow(delegate { new DirectoryStore(dir.Path); }, "Store must instantiate given an existing path");
        }

        [Test]
        public void ShouldAcceptRelativePath()
        {
            using (var dir = new TemporaryDirectoryReplacement("relative-path"))
            {
                Assert.False(Path.IsPathRooted(dir.Path), "Internal assertion: Test path must be relative");
                Assert.DoesNotThrow(delegate { new DirectoryStore(dir.Path); }, "Store must accept relative paths");
            }
        }

        [Test]
        public void ShouldProvideDefaultConstructor()
        {
            string cachePath = Path.Combine(Locations.UserCacheDir, DirectoryStore.UserProfileDirectory);
            using (new TemporaryDirectoryReplacement(cachePath))
            {
                Assert.DoesNotThrow(delegate { new DirectoryStore(); }, "Store must be default constructible");
            }
        }

        [Test]
        public void ShouldAcceptInexistantPathAndCreateIt()
        {
            string path = FileUtils.GetTempDirectory();
            Directory.Delete(path);
            try
            {
                Assert.DoesNotThrow(delegate { new DirectoryStore(path); }, "Store's constructor must accept non-existing path");
                Assert.True(Directory.Exists(path));
            }
            finally
            {
                if(Directory.Exists(path)) Directory.Delete(path, true);
            }
        }
    }

    [TestFixture]
    public class StoreFunctionality
    {
        [Test]
        public void ShouldTellIfItContainsAnImplementation()
        {
            string packageDir = CreateArtificialPackage();
            string hash = Manifest.CreateDotFile(packageDir, ManifestFormat.Sha256, null);

            using (var cache = new TemporaryDirectory())
            {
                Directory.Move(packageDir, Path.Combine(cache.Path, hash));
                Assert.True(new DirectoryStore(cache.Path).Contains(new ManifestDigest(hash)));
            }
        }

        [Test]
        public void ShouldAllowToAddFolder()
        {
            string package = CreateArtificialPackage();
            var digest = new ManifestDigest(Manifest.CreateDotFile(package, ManifestFormat.Sha256, null));

            try
            {
                using (var cache = new TemporaryDirectory())
                {
                    var store = new DirectoryStore(cache.Path);
                    store.AddDirectory(package, digest, null);
                    Assert.IsTrue(store.Contains(digest), "After adding, Store must contain the added package");
                    CollectionAssert.AreEqual(new[] {digest.BestDigest}, store.ListAll(), "After adding, Store must show the added package in the complete list");
                }
            }
            finally
            {
                Directory.Delete(package, true);
            }
        }

        [Test]
        public void ShouldAllowToRemove()
        {
            string package = CreateArtificialPackage();
            var digest = new ManifestDigest(Manifest.CreateDotFile(package, ManifestFormat.Sha256, null));

            try
            {
                using (var cache = new TemporaryDirectory())
                {
                    var store = new DirectoryStore(cache.Path);
                    store.AddDirectory(package, digest, null);
                    Assert.IsTrue(store.Contains(digest), "After adding, Store must contain the added package");
                    store.Remove(digest);
                    Assert.IsFalse(store.Contains(digest), "After remove, Store may no longer contain the added package");
                }
            }
            finally
            {
                Directory.Delete(package, true);
            }
        }

        [Test]
        public void ShouldThrowOnAddWithEmptyDigest()
        {
            string package = CreateArtificialPackage();

            try
            {
                using (var cache = new TemporaryDirectory())
                {
                    Assert.Throws(typeof(ArgumentException), () => new DirectoryStore(cache.Path).AddDirectory(package, new ManifestDigest(), null));
                }
            }
            finally
            {
                Directory.Delete(package, true);
            }

        }

        [Test]
        public void ShouldReturnCorrectPathOfPackageInCache()
        {
            string packageDir = CreateArtificialPackage();
            string hash = Manifest.CreateDotFile(packageDir, ManifestFormat.Sha256, null);

            using (var cache = new TemporaryDirectory())
            {
                Directory.Move(packageDir, Path.Combine(cache.Path, hash));
                Assert.AreEqual(Path.Combine(cache.Path, hash), new DirectoryStore(cache.Path).GetPath(new ManifestDigest(hash)), "Store must return the correct path for Implementations it contains");
            }
        }

        [Test]
        public void ShouldThrowWhenRequestedPathOfUncontainedPackage()
        {
            using (var cache = new TemporaryDirectory())
            {
                Assert.Throws(typeof(ImplementationNotFoundException), () => new DirectoryStore(cache.Path).GetPath(new ManifestDigest("sha256=invalid")));
            }
        }

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
    }
}