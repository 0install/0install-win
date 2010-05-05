using System;
using System.IO;
using System.Security.Cryptography;
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
        public void DefaultConstructorShouldFailIfCacheDirInexistant()
        {
            string cache = Locations.GetUserCacheDir("0install");
            using (new TemporaryMove(cache))
                Assert.Throws<DirectoryNotFoundException>(delegate { new Store(); }, "Store must throw DirectoryNotFoundException created with non-existing path");
        }

        [Test]
        public void ShouldAcceptAnExistingPath()
        {
            using (var dir = new TemporaryReplacement(Path.GetFullPath("test-store")))
                Assert.DoesNotThrow(delegate { new Store(dir.Path); }, "Store must instantiate given an existing path");
        }

        [Test]
        public void ShouldAcceptRelativePath()
        {
            using (var dir = new TemporaryReplacement("relative-path"))
            {
                Assert.False(Path.IsPathRooted(dir.Path), "Internal assertion: Test path must be relative");
                Assert.DoesNotThrow(delegate { new Store(dir.Path); }, "Store must accept relative paths");
            }
        }

        [Test]
        public void ShouldProvideDefaultConstructor()
        {
            string cachePath = Locations.GetUserCacheDir("0install.net");
            using (var cache = new TemporaryReplacement(cachePath))
            {
                System.IO.Directory.CreateDirectory(Path.Combine(cache.Path, "implementations"));
                Assert.DoesNotThrow(delegate { new Store(); }, "Store must be default constructible");
            }
        }

        [Test]
        public void ShouldRejectInexistantPath()
        {
            string path = Path.GetFullPath(FileHelper.GetUniqueFileName(Path.GetTempPath()));
            Assert.Throws<DirectoryNotFoundException>(delegate { new Store(path); }, "Store must throw DirectoryNotFoundException created with non-existing path");
        }
    }

    [TestFixture]
    public class StoreFunctionality
    {
        private TemporaryDirectory _cache;
        private Store _store;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _cache = new TemporaryDirectory();
            _store = new Store(_cache.Path);
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
            string manifestPath = Path.Combine(packageDir, ".manifest");
            NewManifest manifest = NewManifest.Generate(packageDir, SHA256.Create());
            string hash = manifest.Save(manifestPath);

            System.IO.Directory.Move(packageDir, Path.Combine(_cache.Path, "sha256=" + hash));

            Assert.True(_store.Contains(new ManifestDigest(null, null, hash)));
        }

        [Test]
        public void ShouldAllowToAddFolder()
        {
            string packageDir = CreateArtificialPackage();
            ManifestDigest digest = ComputeDigestForPackage(packageDir);

            _store.Add(packageDir, digest);
            Assert.True(_store.Contains(digest), "After adding, store must contain the added package");
        }

        [Test]
        public void ShouldThrowOnAddWithEmptyDigest()
        {
            string package = CreateArtificialPackage();
            Assert.Throws(typeof (ArgumentException), delegate { _store.Add(package, new ManifestDigest(null, null, null)); });
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
            using (FileStream contentFile = System.IO.File.Create(filePath))
            {
                for (int i = 0; i < 1000; ++i)
                    contentFile.WriteByte((byte)'A');
            }
        }

        private static ManifestDigest ComputeDigestForPackage(string packageDir)
        {
            string temporaryManifest = FileHelper.GetUniqueFileName(Path.GetTempPath());
            try
            {
                var manifest = NewManifest.Generate(packageDir, SHA256.Create());
                var hash = manifest.Save(temporaryManifest);
                return new ManifestDigest(null, null, hash);
            }
            finally
            {
                System.IO.File.Delete(temporaryManifest);
            }
        }
    }
}