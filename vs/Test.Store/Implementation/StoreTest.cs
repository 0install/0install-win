using System;
using System.IO;
using Common.Helpers;
using Common.Storage;
using NUnit.Framework;
using ZeroInstall.Model;
using ZeroInstall.Store.Utilities;

namespace ZeroInstall.Store.Implementation
{
    [TestFixture]
    public class StoreCreation
    {
        [Test]
        public void DefaultConstructorShouldCreateCacheDirIfInexistant()
        {
            string cache = Locations.GetUserCacheDir(DirectoryStore.UserProfileDirectory);
            using (new TemporaryMove(cache))
            {
                try
                {
                    Assert.DoesNotThrow(delegate { new DirectoryStore(); }, "Store's default constructor must accept non-existing path");
                    Assert.True(System.IO.Directory.Exists(cache));
                }
                finally
                {
                    if (System.IO.Directory.Exists(cache)) System.IO.Directory.Delete(cache, recursive: true);
                }
            }
        }

        [Test]
        public void ShouldAcceptAnExistingPath()
        {
            using (var dir = new TemporaryReplacement(Path.GetFullPath("test-store")))
                Assert.DoesNotThrow(delegate { new DirectoryStore(dir.Path); }, "Store must instantiate given an existing path");
        }

        [Test]
        public void ShouldAcceptRelativePath()
        {
            using (var dir = new TemporaryReplacement("relative-path"))
            {
                Assert.False(Path.IsPathRooted(dir.Path), "Internal assertion: Test path must be relative");
                Assert.DoesNotThrow(delegate { new DirectoryStore(dir.Path); }, "Store must accept relative paths");
            }
        }

        [Test]
        public void ShouldProvideDefaultConstructor()
        {
            string cachePath = Locations.GetUserCacheDir(DirectoryStore.UserProfileDirectory);
            using (var cache = new TemporaryReplacement(cachePath))
            {
                Assert.DoesNotThrow(delegate { new DirectoryStore(); }, "Store must be default constructible");
            }
        }

        [Test]
        public void ShouldAcceptInexistantPathAndCreateIt()
        {
            string path = Path.GetFullPath(FileHelper.GetUniqueFileName(Path.GetTempPath()));
            try
            {
                Assert.DoesNotThrow(delegate { new DirectoryStore(path); }, "Store's constructor must accept non-existing path");
                Assert.True(System.IO.Directory.Exists(path));
            }
            finally
            {
                if(System.IO.Directory.Exists(path)) System.IO.Directory.Delete(path, recursive: true);
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
            string hash = Manifest.CreateDotFile(packageDir, ManifestFormat.Sha256);

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
            var digest = new ManifestDigest(Manifest.Generate(packageDir, ManifestFormat.Sha256).CalculateHash());

            using (var cache = new TemporaryDirectory())
            {
                var store = new DirectoryStore(cache.Path);
                store.Add(packageDir, digest);
                Assert.True(store.Contains(digest), "After adding, Store must contain the added package");
            }
        }

        [Test]
        public void ShouldThrowOnAddWithEmptyDigest()
        {
            string package = CreateArtificialPackage();

            using (var cache = new TemporaryDirectory())
            {
                Assert.Throws(typeof(ArgumentException), delegate { new DirectoryStore(cache.Path).Add(package, new ManifestDigest()); });
            }
        }

        [Test]
        public void ShouldReturnCorrectPathOfPackageInCache()
        {
            string packageDir = CreateArtificialPackage();
            string hash = Manifest.CreateDotFile(packageDir, ManifestFormat.Sha256);

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
            File.WriteAllText(Path.Combine(packageDir, "file.txt"), @"AAA");

            return packageDir;
        }
        #endregion
    }
}