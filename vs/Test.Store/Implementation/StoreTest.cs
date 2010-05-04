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
            string cachePath = Locations.GetUserCacheDir("0install");
            using (var cache = new TemporaryReplacement(cachePath))
                Assert.DoesNotThrow(delegate { new Store(); }, "Store must be default constructible");
        }

        [Test]
        public void ShouldRejectInexistantPath()
        {
            string path = DirectoryHelper.FindInexistantPath(Path.GetFullPath("test-store"));
            Assert.Throws<DirectoryNotFoundException>(delegate { new Store(path); }, "Store must throw DirectoryNotFoundException created with non-existing path");
        }
    }

    [TestFixture]
    public class StoreFunctionality
    {
        private TemporaryReplacement _cache;
        private Store _store;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _cache = new TemporaryReplacement("test-store");
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
            using (var temporaryDir = new TemporaryReplacement("temp"))
            {
                string packageDir = Path.Combine(temporaryDir.Path, "package");
                System.IO.Directory.CreateDirectory(packageDir);

                string contentFilePath = Path.Combine(packageDir, "content");
                PopulateFile(contentFilePath);

                string manifestPath = Path.Combine(packageDir, ".manifest");
                NewManifest manifest = NewManifest.Generate(packageDir, SHA256.Create());
                manifest.Save(manifestPath);
                string hash = FileHelper.ComputeHash(manifestPath, SHA256.Create());

                System.IO.Directory.Move(packageDir, Path.Combine(_cache.Path, "sha256=" + hash));

                Assert.True(_store.Contains(new ManifestDigest(null, null, hash)));
            }
        }

        private static void PopulateFile(string filePath)
        {
            using (FileStream contentFile = System.IO.File.Create(filePath))
            {
                for (int i = 0; i < 1000; ++i)
                    contentFile.WriteByte((byte)'A');
            }
        }
    }
}