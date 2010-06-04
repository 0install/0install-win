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
                
                builder.AddFile("file1", Encoding.UTF8.GetBytes(@"AAAA"));
                builder.AddFolder("folder1").AddFile("file2",  Encoding.UTF8.GetBytes(@"dskf\nsdf\n"));

                using (var f = File.Create(_archiveFile))
                {
                    builder.GeneratePackageArchive(f);
                }
                Implementation implementation = SynthesizeImplementation(_archiveFile, 0);
                var request = new FetcherRequest(new List<Implementation> {implementation});
                _fetcher.RunSync(request);
                Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
        }

        [Test]
        public void ShouldCorrectlyExtractSelfExtractingArchives()
        {
            using (var packageDir = new TemporaryDirectory())
            {
                PackageBuilder builder = new PackageBuilder();
                builder.AddFile("file1",  Encoding.UTF8.GetBytes(@"AAAA"));
                builder.AddFolder("folder1").AddFile("file2",  Encoding.UTF8.GetBytes(@"dskf\nsdf\n"));

                using (var archiveStream = File.Create(_archiveFile))
                {
                    archiveStream.Seek(0x1000, SeekOrigin.Begin);
                    builder.GeneratePackageArchive(archiveStream);
                }
                Implementation implementation = SynthesizeImplementation(_archiveFile, 0x1000);
                var request = new FetcherRequest(new List<Implementation> { implementation });
                _fetcher.RunSync(request);
                Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
            }
        }

        [Test]
        public void ShouldGenerateCorrectXbitFile()
        {
            var creationDate = new DateTime(2010, 5, 31, 0, 0, 0, 0, DateTimeKind.Utc);
            using (var file = File.Create(_archiveFile))
            using (var zip = new ZipOutputStream(file))
            {
                zip.SetLevel(9);
                var entry = new ZipEntry("executable")
                            {
                                DateTime = creationDate,
                                HostSystem = (int)HostSystemID.Unix
                            };
                const int xBitFlag = 0x0040;
                entry.ExternalFileAttributes |= xBitFlag;
                zip.PutNextEntry(entry);

                zip.WriteByte(50);
                zip.WriteByte(50);
            }


            ManifestDigest digest;
            using (var package = new TemporaryDirectory())
            {
                string contentExecutable = Path.Combine(package.Path, "executable");
                File.WriteAllBytes(contentExecutable, new byte[] { 0 });
                File.SetLastWriteTimeUtc(contentExecutable, creationDate);
                File.WriteAllText(Path.Combine(package.Path, ".xbit"), "/executable");
                digest = CreateDigestForFolder(package.Path);
            }
            var archive = SynthesizeArchive(_archiveFile, 0);
            var implementation = new Implementation
            {
                ManifestDigest = digest,
                Archives = { archive }
            };

            var request = new FetcherRequest(new List<Implementation> { implementation });
            _fetcher.RunSync(request);
            Assert.True(_store.Contains(implementation.ManifestDigest), "Fetcher must make the requested implementation available in its associated store");
            
        }

        private static ManifestDigest CreateDigestForArchiveFile(string file, int offset)
        {
            using (var fileStream = File.OpenRead(file))
            using (var extractFolder = new TemporaryDirectory())
            {
                fileStream.Seek(offset, SeekOrigin.Begin);
                using (var zip = new ZipInputStream(fileStream))
                {
                    ZipEntry entry;

                    while ((entry = zip.GetNextEntry()) != null)
                    {
                        if (entry.IsDirectory)
                        {
                            Directory.CreateDirectory(Path.Combine(extractFolder.Path, entry.Name));
                        }
                        else if (entry.IsFile)
                        {
                            string currentFile = Path.Combine(extractFolder.Path, entry.Name);
                            Directory.CreateDirectory(Path.GetDirectoryName(currentFile));
                            var binaryEntry = new BinaryReader(zip);
                            File.WriteAllBytes(currentFile, binaryEntry.ReadBytes((int)entry.Size));
                            File.SetLastWriteTimeUtc(currentFile, entry.DateTime);
                        }
                    }
                }
                return CreateDigestForFolder(extractFolder.Path);
            }
        }

        private static ManifestDigest CreateDigestForFolder(string folder)
        {
            return new ManifestDigest(Manifest.Generate(folder, ManifestFormat.Sha256).CalculateDigest());
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

        private static Implementation SynthesizeImplementation(string archiveFile, int offset)
        {
            var archive = SynthesizeArchive(archiveFile, offset);
            var digest = CreateDigestForArchiveFile(archiveFile, offset);
            var result = new Implementation
                         {
                             ManifestDigest = digest,
                             Archives = { archive }
                         };
            return result;
        }
    }
}