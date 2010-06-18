using System;
using System.Text;
using NUnit.Framework;
using ZeroInstall.DownloadBroker;
using System.IO;
using ZeroInstall.Store.Utilities;

namespace Common.Archive
{
    [TestFixture]
    class BasicFunctionality
    {
        byte[] _archiveData;
        TemporaryReplacement _sandbox;
        HierarchyEntry _package;
        string _oldWorkingDirectory;

        [SetUp]
        public void SetUp()
        {
            var packageBuilder = new PackageBuilder()
                .AddFile("file1", Encoding.ASCII.GetBytes("First file"))
                .AddFile("file2", new byte[] { });
            packageBuilder.AddFolder("emptyFolder");
            packageBuilder.AddFolder("folder1")
                .AddFile("nestedFile", Encoding.ASCII.GetBytes("File 3\n"))
                .AddFolder("nestedFolder").AddFile("doublyNestedFile", Encoding.ASCII.GetBytes("File 4"));
            _package = packageBuilder.Hierarchy;

            using (var archiveStream = new MemoryStream())
            {
                packageBuilder.GeneratePackageArchive(archiveStream);
                _archiveData = archiveStream.ToArray();
            }

            _sandbox = new TemporaryReplacement(Path.Combine(Path.GetTempPath(), "zipExtraction-Basic"));
            _oldWorkingDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = _sandbox.Path;
        }

        [TearDown]
        public void TearDown()
        {
            Environment.CurrentDirectory = _oldWorkingDirectory;
            _sandbox.Dispose();
        }

        [Test]
        public void ExtractionIntoFolder()
        {
            File.WriteAllBytes("a.zip", _archiveData);
            using (var archiveStream = new MemoryStream(_archiveData))
            {
                var extractor = new ZipExtractor(archiveStream);
                extractor.ExtractTo("extractedArchive");

                Assert.IsTrue(Directory.Exists("extractedArchive"));
                HierarchyEntry.EntryHandler compareDirectory = delegate(HierarchyEntry entry)
                {
                    string extractedPosition = Path.Combine("extractedArchive", entry.RelativePath);
                    if (entry is FileEntry)
                    {
                        var fileEntry = (FileEntry)entry;
                        Assert.IsTrue(File.Exists(extractedPosition));
                        byte[] fileData = File.ReadAllBytes(extractedPosition);
                        Assert.AreEqual(fileEntry.Content, fileData);
                    }
                    else if (entry is EntryContainer)
                    {
                        Assert.IsTrue(Directory.Exists(extractedPosition));
                    }
                };
                _package.RecurseInto(compareDirectory);
            }
        }
    }

    [TestFixture]
    class CornerCases
    {
        private TemporaryReplacement _sandbox;

        [SetUp]
        public void SetUp()
        {
            _sandbox = new TemporaryReplacement(Path.Combine(Path.GetTempPath(), "zipExtraction-Corner"));
        }

        [TearDown]
        public void TearDown()
        {
            //_sandbox.Dispose();
        }

        [Test]
        public void TestRejectParentDirectoryEntry()
        {
            var builder = new PackageBuilder();
            builder.AddFolder("..");

            var archiveStream = File.Create(Path.Combine(_sandbox.Path, "ar.zip"));
            builder.GeneratePackageArchive(archiveStream);
            archiveStream.Seek(0, SeekOrigin.Begin);
            var extractor = new ZipExtractor(archiveStream);
            Assert.Throws<InvalidArchiveException>(() => extractor.ExtractTo("extractedArchive"), "ZipExtractor must not accept archives with '..' as entry");
            archiveStream.Dispose();
        }
    }
}
