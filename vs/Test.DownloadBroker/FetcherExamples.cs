using System.IO;
using NUnit.Framework;
using NUnit.Mocks;
using Common.Storage;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Model;
using System.Collections.Generic;

namespace ZeroInstall.DownloadBroker
{
    [TestFixture]
    public class FetcherCreation
    {
        [Test]
        public void ShouldProvideDefaultSingleton()
        {
            Assert.NotNull(Fetcher.Default, "Fetcher must provide a Default singleton");
        }
    }

    [TestFixture]
    public class DownloadFunctionality
    {
        private TemporaryDirectory storeDir;
        private DirectoryStore store;
        private Fetcher fetcher;

        [SetUp]
        public void SetUp()
        {
            storeDir = new TemporaryDirectory();
            store = new DirectoryStore(storeDir.Path);
            fetcher = new Fetcher(store);
        }

        [TearDown]
        public void TearDown()
        {
            storeDir.Dispose();
        }

        [Test]
        public void ShouldDownloadIntoStore()
        {
            var dummyImplementation = ImplementationTest.CreateTestImplementation();
            var request = new FetcherRequest(new List<Implementation> { dummyImplementation });
            fetcher.RunSync(request);
            Assert.True(store.Contains(dummyImplementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
        }
    }
}