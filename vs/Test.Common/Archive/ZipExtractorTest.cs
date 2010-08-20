using System;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using Common.Storage;
using Common.Helpers;

namespace Common.Archive
{
    [TestFixture]
    class BasicFunctionality
    {
        byte[] _archiveData;
        TemporaryDirectoryReplacement _sandbox;
        HierarchyEntry _package;
        string _oldWorkingDirectory;

        [SetUp]
        public void SetUp()
        {
            var packageBuilder = BuildSamplePackageHierarchy();
            GenerateArchiveDataFromPackage(packageBuilder);
            SetUpSandboxFolder();
        }

        private PackageBuilder BuildSamplePackageHierarchy()
        {
            var packageBuilder = new PackageBuilder()
                .AddFile("file1", "First file")
                .AddFile("file2", new byte[] { 0 });
            packageBuilder.AddFolder("emptyFolder");
            packageBuilder.AddFolder("folder1")
                .AddFile("nestedFile", "File 3\n")
                .AddFolder("nestedFolder").AddFile("doublyNestedFile", "File 4");
            _package = packageBuilder.Hierarchy;
            return packageBuilder;
        }

        private void GenerateArchiveDataFromPackage(PackageBuilder packageBuilder)
        {
            using (var archiveStream = new MemoryStream())
            {
                packageBuilder.GeneratePackageArchive(archiveStream);
                _archiveData = archiveStream.ToArray();
            }
        }

        private void SetUpSandboxFolder()
        {
            _sandbox = new TemporaryDirectoryReplacement(Path.Combine(Path.GetTempPath(), "zipExtraction-Basic"));
            _oldWorkingDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = _sandbox.Path;
        }

        [TearDown]
        public void TearDown()
        {
            TearDownSandboxFolder();
        }

        private void TearDownSandboxFolder()
        {
            Environment.CurrentDirectory = _oldWorkingDirectory;
            _sandbox.Dispose();
        }

        [Test]
        public void ExtractionIntoFolder()
        {
            using (var extractor = Extractor.CreateExtractor("application/zip", new MemoryStream(_archiveData), 0, "extractedArchive"))
                extractor.RunSync();

            Assert.IsTrue(Directory.Exists("extractedArchive"));
            var comparer = new CompareHierarchyToExtractedFolder("extractedArchive");
            _package.AcceptVisitor(comparer);
        }

        class CompareHierarchyToExtractedFolder : HierarchyVisitor
        {
            string folder;

            public CompareHierarchyToExtractedFolder(string folderToCompare)
            {
                folder = folderToCompare;
            }

            public override void VisitFile(FileEntry entry)
            {
                string extractedPosition = Path.Combine(folder, entry.RelativePath);
                Assert.IsTrue(File.Exists(extractedPosition), "File " + extractedPosition + " does not exist.");
                byte[] fileData = File.ReadAllBytes(extractedPosition);
                Assert.AreEqual(entry.Content, fileData, "Different content in file " + extractedPosition);
            }

            public override void VisitFolder(FolderEntry entry)
            {
                string extractedPosition = Path.Combine(folder, entry.RelativePath);
                Assert.IsTrue(Directory.Exists(extractedPosition), "Directory " + extractedPosition + " does not exist.");
                visitChildren(entry);
            }
        }

        [Test]
        public void ExtractionOfSubDir()
        {
            using (var extractor = Extractor.CreateExtractor("application/zip", new MemoryStream(_archiveData), 0, "extractedArchive"))
            {
                extractor.SubDir = "folder1";
                extractor.RunSync();
            }

            Assert.IsTrue(Directory.Exists(Path.Combine("extractedArchive", "nestedFolder")));
            Assert.IsTrue(File.Exists(Path.Combine("extractedArchive", "nestedFile")));
            Assert.IsFalse(File.Exists(Path.Combine("extractedArchive", "file1")));
            Assert.IsFalse(File.Exists(Path.Combine("extractedArchive", "file2")));
        }

        [Test]
        public void TestExtractOverwritingExistingItems()
        {
            Directory.CreateDirectory("destination");
            File.WriteAllText("destination/file1", "Wrong content");
            File.WriteAllText("destination/file0", "This file should not be touched");
            using (var extractor = Extractor.CreateExtractor("application/zip", new MemoryStream(_archiveData), 0, "destination"))
                extractor.RunSync();

            Assert.IsTrue(File.Exists("destination/file0"), "Extractor cleaned directory.");
            string file0Content = File.ReadAllText("destination/file0");
            Assert.AreEqual("This file should not be touched", file0Content);
            var comparer = new CompareHierarchyToExtractedFolder("destination");
            _package.AcceptVisitor(comparer);
        }

        [Test]
        public void TestListContent()
        {
            using (var extractor = Extractor.CreateExtractor("application/zip", new MemoryStream(_archiveData), 0, "temp"))
            {
                var entryList = new List<string> { "file1", "file2", "emptyFolder" + Path.DirectorySeparatorChar, "folder1" + Path.DirectorySeparatorChar,
                    "folder1" + Path.DirectorySeparatorChar + "nestedFile", "folder1" + Path.DirectorySeparatorChar + "nestedFolder" + Path.DirectorySeparatorChar,
                    "folder1" + Path.DirectorySeparatorChar + "nestedFolder" + Path.DirectorySeparatorChar + "doublyNestedFile" };

                var archiveContentList = (List<string>)extractor.ListContent();

                Assert.IsTrue(entryList.Count == archiveContentList.Count, "Extractor listed wrong number of entries.");

                foreach (string entry in entryList)
                {
                    Assert.IsTrue(archiveContentList.Contains(entry), "Extractor did not list archive entry: " + entry);
                }
            }
        }

        [Test]
        public void TestListDirectories()
        {
            using (var extractor = Extractor.CreateExtractor("application/zip", new MemoryStream(_archiveData), 0, "temp"))
            {
                var entryList = new List<string> { "emptyFolder" + Path.DirectorySeparatorChar, "folder1" + Path.DirectorySeparatorChar,
                    "folder1" + Path.DirectorySeparatorChar + "nestedFolder" + Path.DirectorySeparatorChar };

                var archiveContentList = (List<string>)extractor.ListDirectories();

                Assert.IsTrue(entryList.Count == archiveContentList.Count, "Extractor listed wrong number of directories.");

                foreach (string entry in entryList)
                {
                    Assert.IsTrue(archiveContentList.Contains(entry), "Extractor did not list archive subdirectory: " + entry);
                }
            }
        }
    }

    [TestFixture]
    class CornerCases
    {
        private TemporaryDirectoryReplacement _sandbox;

        [SetUp]
        public void SetUp()
        {
            _sandbox = new TemporaryDirectoryReplacement(Path.Combine(Path.GetTempPath(), "zipExtraction-Corner"));
        }

        [TearDown]
        public void TearDown()
        {
            _sandbox.Dispose();
        }

        [Test]
        public void TestRejectParentDirectoryEntry()
        {
            var builder = new PackageBuilder();
            builder.AddFolder("..");

            using (var archiveStream = File.Create(Path.Combine(_sandbox.Path, "ar.zip")))
            {
                builder.GeneratePackageArchive(archiveStream);
                archiveStream.Seek(0, SeekOrigin.Begin);
                var extractor = new ZipExtractor(archiveStream, 0, "extractedArchive");
                Assert.Throws<IOException>(extractor.RunSync, "ZipExtractor must not accept archives with '..' as entry");
                archiveStream.Dispose();
            }
        }

        [Test]
        public void TestExtractEmptyFile()
        {
            var builder = new PackageBuilder()
                .AddFile("emptyFile", new byte[] { });

            using(var archiveStream = File.Create(Path.Combine(_sandbox.Path, "ar.zip")))
            {
                builder.GeneratePackageArchive(archiveStream);
                archiveStream.Seek(0, SeekOrigin.Begin);
                var extractor = new ZipExtractor(archiveStream, 0, "extractedArchive");

                const string message = "ZipExtractor should correctly extract empty files in an archive";
                Assert.DoesNotThrow(extractor.RunSync);
                Assert.IsTrue(File.Exists(Path.Combine("extractedArchive", "emptyFile")), message);
                Assert.AreEqual(new byte[] { }, File.ReadAllBytes(Path.Combine("extractedArchive", "emptyFile")), message);
            }
        }
    }
}
