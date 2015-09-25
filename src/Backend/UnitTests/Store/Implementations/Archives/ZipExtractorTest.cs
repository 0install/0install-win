/*
 * Copyright 2010 Roland Leopold Walkling, 2012-2015 Bastian Eicher
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
using FluentAssertions;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NUnit.Framework;
using ZeroInstall.Store.Model;

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
            using (var extractor = Extractor.Create(this.GetEmbedded("testArchive.zip"), _sandbox, Archive.MimeTypeZip))
                extractor.Run();

            File.Exists(Path.Combine(_sandbox, "subdir1/regular")).Should().BeTrue(because: "Should extract file 'regular'");
            File.GetLastWriteTimeUtc(Path.Combine(_sandbox, "subdir1/regular")).Should().Be(new DateTime(2000, 1, 1, 13, 0, 0), because: "Correct last write time for file 'regular' should be set");

            File.Exists(Path.Combine(_sandbox, "subdir2/executable")).Should().BeTrue(because: "Should extract file 'executable'");
            File.GetLastWriteTimeUtc(Path.Combine(_sandbox, "subdir2/executable")).Should().Be(new DateTime(2000, 1, 1, 13, 0, 0), because: "Correct last write time for file 'executable' should be set");
        }

        [Test]
        public void ExtractionIntoFolder()
        {
            using (var extractor = Extractor.Create(new MemoryStream(_archiveData), _sandbox, Archive.MimeTypeZip))
                extractor.Run();

            Directory.Exists(_sandbox).Should().BeTrue();
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
                File.Exists(extractedPosition).Should().BeTrue(because: "File " + extractedPosition + " does not exist.");
                byte[] fileData = File.ReadAllBytes(extractedPosition);
                fileData.Should().Equal(entry.Content, because: "Different content in file " + extractedPosition);
            }

            public override void VisitFolder(FolderEntry entry)
            {
                string extractedPosition = Path.Combine(_folder, entry.RelativePath);
                Directory.Exists(extractedPosition).Should().BeTrue(because: "Directory " + extractedPosition + " does not exist.");
                VisitChildren(entry);
            }
        }

        [Test]
        public void ExtractionOfSubDir()
        {
            using (var extractor = Extractor.Create(new MemoryStream(_archiveData), _sandbox, Archive.MimeTypeZip))
            {
                extractor.SubDir = "/sub/folder/";
                extractor.Run();
            }

            Directory.Exists(Path.Combine(_sandbox, "nestedFolder")).Should().BeTrue();
            Directory.GetLastWriteTimeUtc(Path.Combine(_sandbox, "nestedFolder")).Should().Be(PackageBuilder.DefaultDate);
            File.Exists(Path.Combine(_sandbox, "nestedFile")).Should().BeTrue();
            File.Exists(Path.Combine(_sandbox, "file1")).Should().BeFalse();
            File.Exists(Path.Combine(_sandbox, "file2")).Should().BeFalse();
        }

        [Test]
        public void EnsureSubDirDoesNotTouchFileNames()
        {
            using (var extractor = Extractor.Create(new MemoryStream(_archiveData), _sandbox, Archive.MimeTypeZip))
            {
                extractor.SubDir = "/sub/folder/nested";
                extractor.Run();
            }

            Directory.Exists(Path.Combine(_sandbox, "Folder")).Should().BeFalse(because: "Should not apply subdir matching to part of filename");
            File.Exists(Path.Combine(_sandbox, "File")).Should().BeFalse(because: "Should not apply subdir matching to part of filename");
        }

        [Test]
        public void TestExtractOverwritingExistingItems()
        {
            File.WriteAllText(Path.Combine(_sandbox, "file1"), @"Wrong content");
            File.WriteAllText(Path.Combine(_sandbox, "file0"), @"This file should not be touched");

            using (var extractor = Extractor.Create(new MemoryStream(_archiveData), _sandbox, Archive.MimeTypeZip))
                extractor.Run();

            File.Exists(Path.Combine(_sandbox, "file0")).Should().BeTrue(because: "Extractor cleaned directory.");
            string file0Content = File.ReadAllText(Path.Combine(_sandbox, "file0"));
            file0Content.Should().Be("This file should not be touched");
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

        /// <summary>
        /// Tests whether the extractor generates a correct <see cref="FlagUtils.SymlinkFile"/> file for a sample ZIP archive containing an executable file.
        /// </summary>
        [Test]
        public void TestExtractUnixArchiveWithExecutable()
        {
            using (var extractor = new ZipExtractor(this.GetEmbedded("testArchive.zip"), _sandbox))
                extractor.Run();

            if (UnixUtils.IsUnix)
                FileUtils.IsExecutable(Path.Combine(_sandbox, "subdir2/executable")).Should().BeTrue(because: "File 'executable' should be marked as executable");
            else
            {
                string xbitFileContent = File.ReadAllText(Path.Combine(_sandbox, FlagUtils.XbitFile)).Trim();
                xbitFileContent.Should().Be("/subdir2/executable");
            }
        }

        /// <summary>
        /// Tests whether the extractor generates a correct <see cref="FlagUtils.SymlinkFile"/> file for a sample ZIP archive containing a symbolic link.
        /// </summary>
        [Test]
        public void TestExtractUnixArchiveWithSymlink()
        {
            using (var extractor = new ZipExtractor(this.GetEmbedded("testArchive.zip"), _sandbox))
                extractor.Run();

            string target;
            string source = Path.Combine(_sandbox, "symlink");
            if (UnixUtils.IsUnix) FileUtils.IsSymlink(source, out target).Should().BeTrue();
            else CygwinUtils.IsSymlink(source, out target).Should().BeTrue();

            target.Should().Be("subdir1/regular", because: "Symlink should point to 'regular'");
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
                using (var extractor = new ZipExtractor(archiveStream, _sandbox))
                    extractor.Invoking(x => x.Run()).ShouldThrow<IOException>(because: "ZipExtractor must not accept archives with '..' as entry");
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

                const string message = "ZipExtractor should correctly extract empty files in an archive";
                using (var extractor = new ZipExtractor(archiveStream, _sandbox))
                    extractor.Invoking(x => x.Run()).ShouldNotThrow();
                File.Exists(Path.Combine(_sandbox, "emptyFile")).Should().BeTrue(because: message);
                File.ReadAllBytes(Path.Combine(_sandbox, "emptyFile")).Should().BeEmpty(because: message);
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

                const string message = "ZipExtractor should correctly extract empty files in an archive to custom destination";
                using (var extractor = new ZipExtractor(archiveStream, _sandbox) {Destination = "custom"})
                    extractor.Invoking(x => x.Run()).ShouldNotThrow();
                File.Exists(Path.Combine(_sandbox, "custom", "emptyFile")).Should().BeTrue(because: message);
                File.ReadAllBytes(Path.Combine(_sandbox, "custom", "emptyFile")).Should().BeEmpty(because: message);
            }
        }
    }
}
