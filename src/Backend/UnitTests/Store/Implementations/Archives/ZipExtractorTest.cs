/*
 * Copyright 2010 Roland Leopold Walkling, 2012-2014 Bastian Eicher
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

using System;
using System.IO;
using System.Linq;
using NanoByte.Common.Storage;
using NanoByte.Common.Utils;
using NUnit.Framework;

namespace ZeroInstall.Store.Implementations.Archives
{
    [TestFixture]
    public class ZipExtractorTestBasicFunctionality
    {
        #region Helpers
        private PackageBuilder BuildSamplePackageHierarchy()
        {
            var packageBuilder = new PackageBuilder()
                .AddFile("file1", "First file")
                .AddFile("file2", new byte[] {0});
            packageBuilder.AddFolder("emptyFolder");
            packageBuilder.AddFolder("sub").AddFolder("folder")
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

        private byte[] _archiveData;
        private TemporaryDirectory _sandbox;
        private HierarchyEntry _package;

        [SetUp]
        public void SetUp()
        {
            var packageBuilder = BuildSamplePackageHierarchy();
            GenerateArchiveDataFromPackage(packageBuilder);
            _sandbox = new TemporaryDirectory("0install-unit-tests");
        }

        [TearDown]
        public void TearDown()
        {
            _sandbox.Dispose();
        }

        [Test(Description = "Tests whether the extractor correctly restores files including their last changed timestamps.")]
        public void TestFileExtract()
        {
            using (var extractor = Extractor.FromStream(TestData.GetResource("testArchive.zip"), _sandbox, Model.Archive.MimeTypeZip))
                extractor.Run();

            Assert.IsTrue(File.Exists(Path.Combine(_sandbox, "subdir1/regular")), "Should extract file 'regular'");
            Assert.AreEqual(new DateTime(2000, 1, 1, 13, 0, 0), File.GetLastWriteTimeUtc(Path.Combine(_sandbox, "subdir1/regular")), "Correct last write time for file 'regular' should be set");

            Assert.IsTrue(File.Exists(Path.Combine(_sandbox, "subdir2/executable")), "Should extract file 'executable'");
            Assert.AreEqual(new DateTime(2000, 1, 1, 13, 0, 0), File.GetLastWriteTimeUtc(Path.Combine(_sandbox, "subdir2/executable")), "Correct last write time for file 'executable' should be set");
        }

        [Test]
        public void ExtractionIntoFolder()
        {
            using (var extractor = Extractor.FromStream(new MemoryStream(_archiveData), _sandbox, Model.Archive.MimeTypeZip))
                extractor.Run();

            Assert.IsTrue(Directory.Exists(_sandbox));
            var comparer = new CompareHierarchyToExtractedFolder(_sandbox);
            _package.AcceptVisitor(comparer);
        }

        private class CompareHierarchyToExtractedFolder : HierarchyVisitor
        {
            private readonly string _folder;

            public CompareHierarchyToExtractedFolder(string folderToCompare)
            {
                _folder = folderToCompare;
            }

            public override void VisitFile(FileEntry entry)
            {
                string extractedPosition = Path.Combine(_folder, entry.RelativePath);
                Assert.IsTrue(File.Exists(extractedPosition), "File " + extractedPosition + " does not exist.");
                byte[] fileData = File.ReadAllBytes(extractedPosition);
                Assert.AreEqual(entry.Content, fileData, "Different content in file " + extractedPosition);
            }

            public override void VisitFolder(FolderEntry entry)
            {
                string extractedPosition = Path.Combine(_folder, entry.RelativePath);
                Assert.IsTrue(Directory.Exists(extractedPosition), "Directory " + extractedPosition + " does not exist.");
                VisitChildren(entry);
            }
        }

        [Test]
        public void ExtractionOfSubDir()
        {
            using (var extractor = Extractor.FromStream(new MemoryStream(_archiveData), _sandbox, Model.Archive.MimeTypeZip))
            {
                extractor.SubDir = "/sub/folder/";
                extractor.Run();
            }

            Assert.IsTrue(Directory.Exists(Path.Combine(_sandbox, "nestedFolder")));
            Assert.AreEqual(PackageBuilder.DefaultDate, Directory.GetLastWriteTimeUtc(Path.Combine(_sandbox, "nestedFolder")));
            Assert.IsTrue(File.Exists(Path.Combine(_sandbox, "nestedFile")));
            Assert.IsFalse(File.Exists(Path.Combine(_sandbox, "file1")));
            Assert.IsFalse(File.Exists(Path.Combine(_sandbox, "file2")));
        }

        [Test]
        public void EnsureSubDirDoesNotTouchFileNames()
        {
            using (var extractor = Extractor.FromStream(new MemoryStream(_archiveData), _sandbox, Model.Archive.MimeTypeZip))
            {
                extractor.SubDir = "/sub/folder/nested";
                extractor.Run();
            }

            Assert.IsFalse(Directory.Exists(Path.Combine(_sandbox, "Folder")), "Should not apply subdir matching to part of filename");
            Assert.IsFalse(File.Exists(Path.Combine(_sandbox, "File")), "Should not apply subdir matching to part of filename");
        }

        [Test]
        public void TestExtractOverwritingExistingItems()
        {
            File.WriteAllText(Path.Combine(_sandbox, "file1"), @"Wrong content");
            File.WriteAllText(Path.Combine(_sandbox, "file0"), @"This file should not be touched");

            using (var extractor = Extractor.FromStream(new MemoryStream(_archiveData), _sandbox, Model.Archive.MimeTypeZip))
                extractor.Run();

            Assert.IsTrue(File.Exists(Path.Combine(_sandbox, "file0")), "Extractor cleaned directory.");
            string file0Content = File.ReadAllText(Path.Combine(_sandbox, "file0"));
            Assert.AreEqual("This file should not be touched", file0Content);
            var comparer = new CompareHierarchyToExtractedFolder(_sandbox);
            _package.AcceptVisitor(comparer);
        }
    }

    [TestFixture]
    public class ZipExtractorTestCornerCases
    {
        private TemporaryDirectory _sandbox;

        [SetUp]
        public void SetUp()
        {
            _sandbox = new TemporaryDirectory("0install-unit-tests");
        }

        [TearDown]
        public void TearDown()
        {
            _sandbox.Dispose();
        }

        [Test(Description = "Tests whether the extractor generates a correct .symlink file for a sample ZIP archive containing an executable file.")]
        public void TestExtractUnixArchiveWithExecutable()
        {
            using (var archive = TestData.GetResource("testArchive.zip"))
            {
                var extractor = new ZipExtractor(archive, _sandbox);
                extractor.Run();
            }

            if (UnixUtils.IsUnix)
                Assert.IsTrue(FileUtils.IsExecutable(Path.Combine(_sandbox, "subdir2/executable")), "File 'executable' should be marked as executable");
            else
            {
                string xbitFileContent = File.ReadAllText(Path.Combine(_sandbox, ".xbit")).Trim();
                Assert.AreEqual("/subdir2/executable", xbitFileContent);
            }
        }

        [Test(Description = "Tests whether the extractor generates a correct .symlink file for a sample ZIP archive containing a symbolic link.")]
        public void TestExtractUnixArchiveWithSymlink()
        {
            using (var archive = TestData.GetResource("testArchive.zip"))
            {
                var extractor = new ZipExtractor(archive, _sandbox);
                extractor.Run();
            }

            string target;
            if (UnixUtils.IsUnix)
                Assert.IsTrue(FileUtils.IsSymlink(Path.Combine(_sandbox, "symlink"), out target));
            else
            {
                string symlinkFileContent = File.ReadAllText(Path.Combine(_sandbox, ".symlink")).Trim();
                Assert.AreEqual("/symlink", symlinkFileContent);
                target = File.ReadAllText(Path.Combine(_sandbox, "symlink"));
            }
            Assert.AreEqual("subdir1/regular", target, "Symlink should point to 'regular'");
        }

        [Test]
        public void TestRejectParentDirectoryEntry()
        {
            var builder = new PackageBuilder();
            builder.AddFolder("..");

            using (var archiveStream = File.Create(Path.Combine(_sandbox, "ar.zip")))
            {
                builder.GeneratePackageArchive(archiveStream);
                archiveStream.Seek(0, SeekOrigin.Begin);
                var extractor = new ZipExtractor(archiveStream, _sandbox);
                Assert.Throws<IOException>(() => extractor.Run(), "ZipExtractor must not accept archives with '..' as entry");
                archiveStream.Dispose();
            }
        }

        [Test]
        public void TestExtractEmptyFile()
        {
            var builder = new PackageBuilder()
                .AddFile("emptyFile", new byte[] {});

            using (var archiveStream = File.Create(Path.Combine(_sandbox, "ar.zip")))
            {
                builder.GeneratePackageArchive(archiveStream);
                archiveStream.Seek(0, SeekOrigin.Begin);
                var extractor = new ZipExtractor(archiveStream, _sandbox);

                const string message = "ZipExtractor should correctly extract empty files in an archive";
                Assert.DoesNotThrow(() => extractor.Run());
                Assert.IsTrue(File.Exists(Path.Combine(_sandbox, "emptyFile")), message);
                Assert.AreEqual(new byte[] {}, File.ReadAllBytes(Path.Combine(_sandbox, "emptyFile")), message);
            }
        }

        [Test]
        public void TestExtractToCustomDestination()
        {
            var builder = new PackageBuilder()
                .AddFile("emptyFile", new byte[] {});

            using (var archiveStream = File.Create(Path.Combine(_sandbox, "ar.zip")))
            {
                builder.GeneratePackageArchive(archiveStream);
                archiveStream.Seek(0, SeekOrigin.Begin);
                var extractor = new ZipExtractor(archiveStream, _sandbox) {Destination = "custom"};

                const string message = "ZipExtractor should correctly extract empty files in an archive to custom destination";
                Assert.DoesNotThrow(() => extractor.Run());
                Assert.IsTrue(File.Exists(new[] {_sandbox, "custom", "emptyFile"}.Aggregate(Path.Combine)), message);
                Assert.AreEqual(new byte[] {}, File.ReadAllBytes(new[] {_sandbox, "custom", "emptyFile"}.Aggregate(Path.Combine)), message);
            }
        }
    }
}
