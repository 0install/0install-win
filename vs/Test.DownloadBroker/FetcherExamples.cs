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
            var dummyImplementation = SynthesizeImplementation(archive);
            var request = new FetcherRequest(new List<Implementation>() {dummyImplementation});
            fetcher.RunSync(request);
            Assert.True(store.Contains(dummyImplementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
        }

        private static Implementation SynthesizeImplementation(FileInfo archiveFile)
        {
            var packageDir = FileHelper.GetTempDirectory();
            SynthesizePackage(packageDir);
            CompressPackage(packageDir, archiveFile);
            var manifest = Manifest.Generate(packageDir, ManifestFormat.Sha256);
            var digest = new ManifestDigest(manifest.CalculateHash());
            var archive = SynthesizeArchive(archiveFile);
            var result = new Implementation()
            {
                ManifestDigest = digest,
                Archives = {archive}
            };
            return result;
        }

        private static void SynthesizePackage(string package)
        {
            File.WriteAllText(Path.Combine(package, "file1"), @"AAAA");
            string inner = Path.Combine(package, "folder1");
            Directory.CreateDirectory(inner);
            File.WriteAllText(Path.Combine(inner, "file2"), @"dskf\nsdf\n");
        }

        private static void CompressPackage(string package, FileInfo destination)
        {
            var fastZip = new FastZip();
            fastZip.CreateZip(destination.FullName, package, true, ".*");
        }

        private static Archive SynthesizeArchive(FileInfo compressedPackage)
        {
            var result = new Archive() {
                MimeType = "application/zip",
                Size = compressedPackage.Length,
                Location = new Uri("http://localhost:50222/archives/test.zip")
            };
            return result;
        }


    }
}