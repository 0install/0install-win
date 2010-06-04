using System;
using System.IO;
using NUnit.Framework;
using Common.Storage;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Model;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using ZeroInstall.Store.Utilities;

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
        private TemporaryReplacement _storeDir;
        private DirectoryStore _store;
        private Fetcher _fetcher;
        private string _archiveFile;
        private ArchiveProvider _server;

        [SetUp]
        public void SetUp()
        {
            _storeDir = new TemporaryReplacement(Path.Combine(Path.GetTempPath(), "store"));
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
                Implementation implementation = SynthesizeImplementation(_archiveFile, 0);
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
                        if (!currentFile.StartsWith(package + @"\")) throw new Exception("file in folder not prefixed by the folder's path.");
                        string relativeFileName = currentFile.Remove(0, (package + @"\").Length);
                        var entry = new ZipEntry(relativeFileName);
                        entry.DateTime = File.GetLastWriteTimeUtc(currentFile);
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