using System;
using System.IO;
using NUnit.Framework;
using Common.Storage;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Model;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using ZeroInstall.Store.Utilities;
using System.Text;
using System.Reflection;

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
        private TemporaryReplacement _testFolder;
        private TemporaryDirectory _storeDir;
        private DirectoryStore _store;
        private Fetcher _fetcher;
        private string _archiveFile;
        private ArchiveProvider _server;

        [SetUp]
        public void SetUp()
        {
            _testFolder = new TemporaryReplacement(Path.Combine(Path.GetTempPath(), "test-sandbox"));
            _storeDir = new TemporaryDirectory(Path.Combine(_testFolder.Path, "store"));
            _store = new DirectoryStore(_storeDir.Path);
            _fetcher = new Fetcher(_store);
            _archiveFile = Path.Combine(_testFolder.Path, "archive.zip");
            _server = new ArchiveProvider(_archiveFile);
            _server.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _server.Dispose();
            _storeDir.Dispose();
            _testFolder.Dispose();
        }

        [Test]
        public void ShouldDownloadIntoStore()
        {
            PackageBuilder builder = new PackageBuilder();

            builder.AddFile("file1", Encoding.UTF8.GetBytes(@"AAAA")).
                AddFolder("folder1").AddFile("file2", Encoding.UTF8.GetBytes(@"dskf\nsdf\n")).
                AddFolder("folder2").AddFile("file3", new byte[] { 55, 55, 55 });

            using (var f = File.Create(_archiveFile))
            {
                builder.GeneratePackageArchive(f);
            }
            Implementation implementation = SynthesizeImplementation(_archiveFile, 0, builder.ComputePackageDigest());
            var request = new FetcherRequest(new List<Implementation> { implementation });
            _fetcher.RunSync(request);
            Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
        }

        [Test]
        public void ShouldCorrectlyExtractSelfExtractingArchives()
        {
            PackageBuilder builder = new PackageBuilder();
            builder.AddFile("file1", Encoding.UTF8.GetBytes(@"AAAA"));
            builder.AddFolder("folder1").AddFile("file2", Encoding.UTF8.GetBytes(@"dskf\nsdf\n"));

            using (var archiveStream = File.Create(_archiveFile))
            {
                archiveStream.Seek(0x1000, SeekOrigin.Begin);
                builder.GeneratePackageArchive(archiveStream);
            }
            Implementation implementation = SynthesizeImplementation(_archiveFile, 0x1000, builder.ComputePackageDigest());
            var request = new FetcherRequest(new List<Implementation> { implementation });
            _fetcher.RunSync(request);
            Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
        }

        [Test]
        public void ShouldGenerateCorrectXbitFile()
        {
            var thisAssembly = Assembly.GetAssembly(typeof (DownloadFunctionality));
            DateTime readmeLastWrite, sdlDllLastWrite;

            using (var sdlArchive = thisAssembly.GetManifestResourceStream(typeof(DownloadFunctionality), "sdlArchive.zip"))
            {
                var reader = new BinaryReader(sdlArchive);
                File.WriteAllBytes(_archiveFile, reader.ReadBytes((int)sdlArchive.Length));

                sdlArchive.Seek(0, SeekOrigin.Begin);
                var zip = new ZipFile(sdlArchive);
                readmeLastWrite = zip.GetEntry("README-SDL.txt").DateTime;
                sdlDllLastWrite = zip.GetEntry("SDL.dll").DateTime;
            }


            var builder = new PackageBuilder();
            using (var readme = thisAssembly.GetManifestResourceStream(typeof(DownloadFunctionality), "README-SDL.txt"))
            {
                var reader = new BinaryReader(readme);
                builder.AddFile("README-SDL.txt", reader.ReadBytes((int)readme.Length), readmeLastWrite);
            }
            using (var SDLdll = thisAssembly.GetManifestResourceStream(typeof(DownloadFunctionality), "SDL.dll"))
            {
                var reader = new BinaryReader(SDLdll);
                builder.AddExecutable("SDL.dll", reader.ReadBytes((int)SDLdll.Length), sdlDllLastWrite);
            }

            var implementation = SynthesizeImplementation(_archiveFile, 0, builder.ComputePackageDigest());
            var request = new FetcherRequest(new List<Implementation> { implementation });
            _fetcher.RunSync(request);
            Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
        }

        private static Archive SynthesizeArchive(string zippedPackage, int offset)
        {
            var result = new Archive
                         {
                             StartOffset = offset,
                             MimeType = "application/zip",
                             Size = new FileInfo(zippedPackage).Length,
                             Location = new Uri("http://localhost:50222/archives/test.zip")
                         };
            return result;
        }

        private static Implementation SynthesizeImplementation(string archiveFile, int offset, ManifestDigest digest)
        {
            var archive = SynthesizeArchive(archiveFile, offset);
            var result = new Implementation
                         {
                             ManifestDigest = digest,
                             Archives = { archive }
                         };
            return result;
        }
    }
}