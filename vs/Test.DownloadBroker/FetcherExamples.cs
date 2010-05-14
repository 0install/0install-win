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
        private FileInfo archiveFile;
        private ArchiveProvider server;

        [SetUp]
        public void SetUp()
        {
            storeDir = new TemporaryDirectory();
            store = new DirectoryStore(storeDir.Path);
            fetcher = new Fetcher(store);
            archiveFile = new FileInfo(FileHelper.GetUniqueFileName(storeDir.Path) + ".zip");
            server = new ArchiveProvider(archiveFile.FullName);
            server.Start();
        }

        [TearDown]
        public void TearDown()
        {
            server.Dispose();
            archiveFile.Delete();
            storeDir.Dispose();
        }

        [Test]
        public void ShouldDownloadIntoStore()
        {
            var packageDir = FileHelper.GetTempDirectory();
            ManifestDigest digest;
            SynthesizePackage(packageDir, out digest);
            CompressPackage(packageDir, archiveFile);
            Archive archive = SynthesizeArchive(archiveFile);
            Implementation implementation = SynthesizeImplementation(archive, digest);
            var request = new FetcherRequest(new List<Implementation>() { implementation });
            fetcher.RunSync(request);
            Assert.True(store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
        }

        private static void SynthesizePackage(string package, out ManifestDigest digest)
        {
            File.WriteAllText(Path.Combine(package, "file1"), @"AAAA");
            string inner = Path.Combine(package, "folder1");
            Directory.CreateDirectory(inner);
            File.WriteAllText(Path.Combine(inner, "file2"), @"dskf\nsdf\n");
            var manifest = Manifest.Generate(package, ManifestFormat.Sha256);
            digest = new ManifestDigest(manifest.CalculateHash());
        }

        private static void CompressPackage(string package, FileInfo destination)
        {
            var fastZip = new FastZip();
            fastZip.CreateZip(destination.FullName, package, true, ".*");
        }

        private static Archive SynthesizeArchive(FileInfo zippedPackage)
        {
            var result = new Archive()
            {
                MimeType = "application/zip",
                Size = zippedPackage.Length,
                Location = new Uri("http://localhost:50222/archives/test.zip")
            };
            return result;
        }

        private static Implementation SynthesizeImplementation(Archive archive, ManifestDigest digest)
        {
            var result = new Implementation()
            {
                ManifestDigest = digest,
                Archives = {archive}
            };
            return result;
        }
    }
}