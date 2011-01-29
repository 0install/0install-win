/*
 * Copyright 2010 Roland Leopold Walkling
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using NUnit.Framework;
using System.IO;
using Common.Storage;
using Common.Utils;

namespace ZeroInstall.Store.Implementation.Archive
{
    [TestFixture]
    public class ZipTestBasicFunctionality
    {
        #region Helpers
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
        #endregion

        byte[] _archiveData;
        TemporaryDirectory _extractDir;
        HierarchyEntry _package;

        [SetUp]
        public void SetUp()
        {
            var packageBuilder = BuildSamplePackageHierarchy();
            GenerateArchiveDataFromPackage(packageBuilder);
            _extractDir = new TemporaryDirectory("0install-unit-tests");
        }

        [TearDown]
        public void TearDown()
        {
            _extractDir.Dispose();
        }

        [Test]
        public void ExtractionIntoFolder()
        {
            using (var extractor = Extractor.CreateExtractor("application/zip", new MemoryStream(_archiveData), _extractDir.Path))
                extractor.RunSync();

            Assert.IsTrue(Directory.Exists(_extractDir.Path));
            var comparer = new CompareHierarchyToExtractedFolder(_extractDir.Path);
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
                VisitChildren(entry);
            }
        }

        [Test]
        public void ExtractionOfSubDir()
        {
            using (var extractor = Extractor.CreateExtractor("application/zip", new MemoryStream(_archiveData), _extractDir.Path))
            {
                extractor.SubDir = "folder1";
                extractor.RunSync();
            }

            Assert.IsTrue(Directory.Exists(Path.Combine(_extractDir.Path, "nestedFolder")));
            Assert.AreEqual(PackageBuilder.DefaultDate, Directory.GetLastWriteTimeUtc(Path.Combine(_extractDir.Path, "nestedFolder")));
            Assert.IsTrue(File.Exists(Path.Combine(_extractDir.Path, "nestedFile")));
            Assert.IsFalse(File.Exists(Path.Combine(_extractDir.Path, "file1")));
            Assert.IsFalse(File.Exists(Path.Combine(_extractDir.Path, "file2")));
        }

        [Test]
        public void TestExtractOverwritingExistingItems()
        {
            File.WriteAllText(Path.Combine(_extractDir.Path, "file1"), "Wrong content");
            File.WriteAllText(Path.Combine(_extractDir.Path, "file0"), "This file should not be touched");
            using (var extractor = Extractor.CreateExtractor("application/zip", new MemoryStream(_archiveData), _extractDir.Path))
                extractor.RunSync();

            Assert.IsTrue(File.Exists(Path.Combine(_extractDir.Path, "file0")), "Extractor cleaned directory.");
            string file0Content = File.ReadAllText(Path.Combine(_extractDir.Path, "file0"));
            Assert.AreEqual("This file should not be touched", file0Content);
            var comparer = new CompareHierarchyToExtractedFolder(_extractDir.Path);
            _package.AcceptVisitor(comparer);
        }
    }

    [TestFixture]
    class TestZipCornerCases
    {
        private TemporaryDirectory _extractDir;

        [SetUp]
        public void SetUp()
        {
            _extractDir = new TemporaryDirectory("0install-unit-tests");
        }

        [TearDown]
        public void TearDown()
        {
            _extractDir.Dispose();
        }

        /// <summary>
        /// Tests whether the zip extractor generates a correct .xbit file for
        /// an example of a unix archive containing an executable file.
        /// </summary>
        [Test]
        public void TestExtractUnixArchiveWithExecutable()
        {
            using (var archive = TestData.GetSdlZipArchiveStream())
            using (var extractor = new ZipExtractor(archive, _extractDir.Path))
                extractor.RunSync();

            if (MonoUtils.IsUnix)
            {
                Assert.IsTrue(FileUtils.IsExecutable(Path.Combine(_extractDir.Path, "SDL.dll")));
            }
            else
            {
                string xbitFileContent = File.ReadAllText(Path.Combine(_extractDir.Path, ".xbit")).Trim();
                Assert.AreEqual("/SDL.dll", xbitFileContent);
            }
        }

        [Test]
        public void TestRejectParentDirectoryEntry()
        {
            var builder = new PackageBuilder();
            builder.AddFolder("..");

            using (var archiveStream = File.Create(Path.Combine(_extractDir.Path, "ar.zip")))
            {
                builder.GeneratePackageArchive(archiveStream);
                archiveStream.Seek(0, SeekOrigin.Begin);
                var extractor = new ZipExtractor(archiveStream, _extractDir.Path);
                Assert.Throws<IOException>(extractor.RunSync, "ZipExtractor must not accept archives with '..' as entry");
                archiveStream.Dispose();
            }
        }

        [Test]
        public void TestExtractEmptyFile()
        {
            var builder = new PackageBuilder()
                .AddFile("emptyFile", new byte[] {});

            using(var archiveStream = File.Create(Path.Combine(_extractDir.Path, "ar.zip")))
            {
                builder.GeneratePackageArchive(archiveStream);
                archiveStream.Seek(0, SeekOrigin.Begin);
                var extractor = new ZipExtractor(archiveStream, _extractDir.Path);

                const string message = "ZipExtractor should correctly extract empty files in an archive";
                Assert.DoesNotThrow(extractor.RunSync);
                Assert.IsTrue(File.Exists(Path.Combine(_extractDir.Path, "emptyFile")), message);
                Assert.AreEqual(new byte[] {}, File.ReadAllBytes(Path.Combine(_extractDir.Path, "emptyFile")), message);
            }
        }
    }
}
