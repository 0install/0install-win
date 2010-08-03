using System;
using System.IO;
using Common.Archive;
using Common.Helpers;
using Common.Storage;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Implementation
{
    [TestFixture]
    public class StoreCreation
    {
        [Test]
        public void DefaultConstructorShouldCreateCacheDirIfInexistant()
        {
            string cache = Locations.GetUserCacheDir(DirectoryStore.UserProfileDirectory);
            using (new TemporaryDirectoryMove(cache))
            {
                try
                {
                    Assert.DoesNotThrow(delegate { new DirectoryStore(); }, "Store's default constructor must accept non-existing path");
                    Assert.True(Directory.Exists(cache));
                }
                finally
                {
                    if (Directory.Exists(cache)) Directory.Delete(cache, recursive: true);
                }
            }
        }

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
            string cachePath = Locations.GetUserCacheDir(DirectoryStore.UserProfileDirectory);
            using (new TemporaryDirectoryReplacement(cachePath))
            {
                Assert.DoesNotThrow(delegate { new DirectoryStore(); }, "Store must be default constructible");
            }
        }

        [Test]
        public void ShouldAcceptInexistantPathAndCreateIt()
        {
            string path = FileHelper.GetTempDirectory();
            Directory.Delete(path);
            try
            {
                Assert.DoesNotThrow(delegate { new DirectoryStore(path); }, "Store's constructor must accept non-existing path");
                Assert.True(Directory.Exists(path));
            }
            finally
            {
                if(Directory.Exists(path)) Directory.Delete(path, recursive: true);
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
            string packageDir = CreateArtificialPackage();
            var digest = new ManifestDigest(Manifest.CreateDotFile(packageDir, ManifestFormat.Sha256, null));

            using (var cache = new TemporaryDirectory())
            {
                var store = new DirectoryStore(cache.Path);
                store.AddDirectory(packageDir, digest, null);
                Assert.True(store.Contains(digest), "After adding, Store must contain the added package");
            }
        }

        // Test deactivated because FastZip writes incorrect file changed times
        //[Test]
        public void ShouldAllowToAddArchive()
        {
            string packageDir = CreateArtificialPackage();
            var digest = new ManifestDigest(Manifest.CreateDotFile(packageDir, ManifestFormat.Sha256, null));

            string zipFile = Path.GetTempFileName();
            new FastZip().CreateZip(zipFile, packageDir, true, "");

            try
            {
                using (var cache = new TemporaryDirectory())
                {
                    var store = new DirectoryStore(cache.Path);
                    store.AddArchive(new ArchiveFileInfo {Path = zipFile, MimeType = "application/zip"}, digest, null, null);
                    Assert.True(store.Contains(digest), "After adding, Store must contain the added package");
                }
            }
            finally
            {
                File.Delete(zipFile);
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
            string packageDir = FileHelper.GetTempDirectory();
            string subDir = Path.Combine(packageDir, "subdir");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "file.txt"), @"AAA");

            return packageDir;
        }
        #endregion
    }
}