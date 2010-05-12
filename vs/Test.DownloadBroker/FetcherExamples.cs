using System;
using System.IO;
using C5;
using Common.Helpers;
using NUnit.Framework;
using NUnit.Mocks;
using Common.Storage;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Model;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;

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
        private FileInfo archive;
        private ArchiveProvider server;

        [SetUp]
        public void SetUp()
        {
            storeDir = new TemporaryDirectory();
            store = new DirectoryStore(storeDir.Path);
            fetcher = new Fetcher(store);
            archive = new FileInfo(FileHelper.GetUniqueFileName(storeDir.Path) + ".zip");
            server = new ArchiveProvider(archive.FullName);
            server.Start();
        }

        [TearDown]
        public void TearDown()
        {
            server.Dispose();
            archive.Delete();
            storeDir.Dispose();
        }

        [Test]
        public void ShouldDownloadIntoStore()
        {
            var dummyImplementation = ImplementationTest.CreateTestImplementation();
            dummyImplementation.Archives.Clear();
            using (var archiveStream = archive.OpenWrite())
            using (var zipStream = new ZipOutputStream(archiveStream))
            {
                var entry = new ZipEntry("archived-file");
                zipStream.PutNextEntry(entry);

                using (var writer = new StreamWriter(zipStream))
                {
                    writer.WriteLine("AAAA");
                }
            }
            dummyImplementation.Archives.Add(new Archive() {Location = new Uri("http://localhost:50222/archives/test.zip")});
            var request = new FetcherRequest(new List<Implementation> { dummyImplementation });
            fetcher.RunSync(request);
            Assert.True(store.Contains(dummyImplementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
        }
    }
}