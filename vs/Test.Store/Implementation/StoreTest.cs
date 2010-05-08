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
        private TemporaryDirectory _cache;
        private DirectoryStore _store;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _cache = new TemporaryDirectory();
            _store = new DirectoryStore(_cache.Path);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _cache.Dispose();
        }

        [Test]
        public void ShouldTellIfItContainsAnImplementation()
        {
            string packageDir = CreateArtificialPackage();
            string hash = CreateManifestForPackage(packageDir);
            MovePackageToCache(packageDir, hash);

            Assert.True(_store.Contains(new ManifestDigest(null, null, hash)));
        }

        [Test]
        public void ShouldAllowToAddFolder()
        {
            string packageDir = CreateArtificialPackage();
            ManifestDigest digest = ComputeDigestForPackage(packageDir);

            _store.Add(packageDir, digest);
            Assert.True(_store.Contains(digest), "After adding, Store must contain the added package");
        }

        [Test]
        public void ShouldThrowOnAddWithEmptyDigest()
        {
            string package = CreateArtificialPackage();
            Assert.Throws(typeof(ArgumentException), delegate { _store.Add(package, new ManifestDigest(null, null, null)); });
        }

        [Test]
        public void ShouldReturnCorrectPathOfPackageInCache()
        {
            string packageDir = CreateArtificialPackage();
            CreateManifestForPackage(packageDir);
            string hash = CreateManifestForPackage(packageDir);
            string packageInCache = MovePackageToCache(packageDir, hash);

            Assert.AreEqual(_store.GetPath(new ManifestDigest(null, null, hash)), packageInCache, "Store must return the correct path for Implementations it contains");
        }

        [Test]
        public void ShouldThrowWhenRequestedPathOfUncontainedPackage()
        {
            Assert.Throws(typeof(ImplementationNotFoundException), () => _store.GetPath(new ManifestDigest(null, null, "invalid")));
        }

        private string MovePackageToCache(string packageDir, string hash)
        {
            string packageInCache = Path.Combine(_cache.Path, "sha256=" + hash);
            Directory.Move(packageDir, packageInCache);
            return packageInCache;
        }

        private static string CreateArtificialPackage()
        {
            string packageDir = FileHelper.GetTempDirectory();
            string contentFile = FileHelper.GetUniqueFileName(packageDir);
            CreateAndPopulateFile(contentFile);
            return packageDir;
        }
        private static void CreateAndPopulateFile(string filePath)
        {
            using (FileStream contentFile = File.Create(filePath))
            {
                for (int i = 0; i < 1000; ++i)
                    contentFile.WriteByte((byte)'A');
            }
        }

        private static string CreateManifestForPackage(string packageDir)
        {
            string manifestPath = Path.Combine(packageDir, ".manifest");
            Manifest manifest = Manifest.Generate(packageDir, ManifestFormat.Sha256);
            return manifest.Save(manifestPath);
        }

        private static ManifestDigest ComputeDigestForPackage(string packageDir)
        {
            string temporaryManifest = FileHelper.GetUniqueFileName(Path.GetTempPath());
            try
            {
                var manifest = Manifest.Generate(packageDir, ManifestFormat.Sha256);
                var hash = manifest.Save(temporaryManifest);
                return new ManifestDigest(hash);
            }
            finally
            {
                File.Delete(temporaryManifest);
            }
        }
    }
}