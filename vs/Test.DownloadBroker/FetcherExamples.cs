using System;
using System.IO;
using NUnit.Framework;
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
        private TemporaryDirectory _storeDir;
        private DirectoryStore _store;
        private Fetcher _fetcher;
        private string _archiveFile;
        private ArchiveProvider _server;

        [SetUp]
        public void SetUp()
        {
            _storeDir = new TemporaryDirectory();
            _store = new DirectoryStore(_storeDir.Path);
            _fetcher = new Fetcher(_store);
            _archiveFile = Path.GetTempFileName();
            _server = new ArchiveProvider(_archiveFile);
            _server.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _server.Dispose();
            File.Delete(_archiveFile);
            _storeDir.Dispose();
        }

        [Test]
        public void ShouldDownloadIntoStore()
        {
            using (var packageDir = new TemporaryDirectory())
            {
                PreparePackageFolder(packageDir.Path);
                CompressPackage(packageDir.Path, _archiveFile);
                Implementation implementation = SynthesizeImplementation(_archiveFile);
                var request = new FetcherRequest(new List<Implementation> {implementation});
                _fetcher.RunSync(request);
                Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
            }
        }

        [Test]
        public void ShouldCorrectlyExtractSelfExtractingArchives()
        {
            using (var packageDir = new TemporaryDirectory())
            {
                PreparePackageFolder(packageDir.Path);
                CompressPackageWithOffset(packageDir.Path, _archiveFile, 0x1000);
                Implementation implementation = SynthesizeImplementation(_archiveFile);
                var request = new FetcherRequest(new List<Implementation> { implementation });
                _fetcher.RunSync(request);
                Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
            }
        }

        private static void PreparePackageFolder(string package)
        {
            File.WriteAllText(Path.Combine(package, "file1"), @"AAAA");
            string inner = Path.Combine(package, "folder1");
            Directory.CreateDirectory(inner);
            File.WriteAllText(Path.Combine(inner, "file2"), @"dskf\nsdf\n");
        }

        private static void CompressPackage(string package, string destination)
        {
            CompressPackageWithOffset(package, destination, 0);
        }

        private static void CompressPackageWithOffset(string package, string destination, long offset)
        {
            using (var file = File.Create(destination))
            {
                file.Position = offset;
                using (var zip = new ZipOutputStream(file))
                {
                    zip.SetLevel(9);
                    string[] files = Directory.GetFiles(package, "*", SearchOption.AllDirectories);
                    foreach (string currentFile in files)
                    {
                        if (!currentFile.StartsWith(package + @"\")) throw new Exception("file in folder didn't contain folder in path");
                        string relativeFileName = currentFile.Remove(0, (package + @"\").Length);
                        var entry = new ZipEntry(relativeFileName);
                        entry.DateTime = File.GetLastWriteTime(currentFile);
                        zip.PutNextEntry(entry);

                        using (var currentFileStream = File.OpenRead(currentFile))
                        using (var currentFileBinary = new BinaryReader(currentFileStream))
                        {
                            zip.Write(currentFileBinary.ReadBytes((int)currentFileStream.Length), 0, (int)currentFileStream.Length);
                        }
                    }
                }
            }
        }

        private static ManifestDigest CreateDigestForArchiveFile(string file)
        {
            using (var extractFolder = new TemporaryDirectory())
            {
                var fastZip = new FastZip
                              {
                                  RestoreDateTimeOnExtract = true
                              };
                fastZip.ExtractZip(file, extractFolder.Path, ".*");
                return new ManifestDigest(Manifest.Generate(extractFolder.Path, ManifestFormat.Sha256).CalculateDigest());
            }
        }

        private static Archive SynthesizeArchive(string zippedPackage)
        {
            var result = new Archive
                         {
                             MimeType = "application/zip",
                             Size = new FileInfo(zippedPackage).Length,
                             Location = new Uri("http://localhost:50222/archives/test.zip")
                         };
            return result;
        }

        private static Implementation SynthesizeImplementation(string archiveFile)
        {
            var archive = SynthesizeArchive(archiveFile);
            var digest = CreateDigestForArchiveFile(archiveFile);
            var result = new Implementation
                         {
                             ManifestDigest = digest,
                             Archives = { archive }
                         };
            return result;
        }
    }
}