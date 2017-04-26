/*
 * Copyright 2010 Roland Leopold Walkling, 2012-2016 Bastian Eicher
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
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NUnit.Framework;
using ZeroInstall.FileSystem;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.Implementations.Archives
{
    [TestFixture]
    public class ZipExtractorTest
    {
        private static TestRoot SamplePackageHierarchy => new TestRoot
        {
            new TestFile("file") {Contents = "First file"},
            new TestFile("file2") {Contents = ""},
            new TestDirectory("emptyFolder"),
            new TestDirectory("sub")
            {
                new TestDirectory("folder")
                {
                    new TestFile("nestedFile") {Contents = "File 3\n"},
                    new TestDirectory("nestedFolder")
                    {
                        new TestFile("doublyNestedFile") {Contents = "File 4"}
                    }
                }
            }
        };

        private byte[] _archiveData;
        private TemporaryDirectory _sandbox;

        [SetUp]
        public void SetUp()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            using (var archiveStream = new MemoryStream())
            {
                SamplePackageHierarchy.Build(tempDir);
                using (var generator = new ZipGenerator(tempDir, archiveStream))
                    generator.Run();
                _archiveData = archiveStream.ToArray();
            }

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
            using (var extractor = ArchiveExtractor.Create(typeof(ZipExtractorTest).GetEmbeddedStream("testArchive.zip"), _sandbox, Archive.MimeTypeZip))
                extractor.Run();

            File.Exists(Path.Combine(_sandbox, "subdir1/regular")).Should().BeTrue(because: "Should extract file 'regular'");
            File.GetLastWriteTimeUtc(Path.Combine(_sandbox, "subdir1/regular")).Should().Be(new DateTime(2000, 1, 1, 13, 0, 0), because: "Correct last write time for file 'regular' should be set");

            File.Exists(Path.Combine(_sandbox, "subdir2/executable")).Should().BeTrue(because: "Should extract file 'executable'");
            File.GetLastWriteTimeUtc(Path.Combine(_sandbox, "subdir2/executable")).Should().Be(new DateTime(2000, 1, 1, 13, 0, 0), because: "Correct last write time for file 'executable' should be set");
        }

        [Test]
        public void ExtractionIntoFolder()
        {
            using (var extractor = ArchiveExtractor.Create(new MemoryStream(_archiveData), _sandbox, Archive.MimeTypeZip))
                extractor.Run();

            SamplePackageHierarchy.Verify(_sandbox);
        }

        [Test]
        public void ExtractionOfSubDir()
        {
            using (var extractor = ArchiveExtractor.Create(new MemoryStream(_archiveData), _sandbox, Archive.MimeTypeZip))
            {
                extractor.Extract = "/sub/folder/";
                extractor.Run();
            }

            new TestRoot
            {
                new TestFile("nestedFile") {Contents = "File 3\n"},
                new TestDirectory("nestedFolder")
                {
                    new TestFile("doublyNestedFile") {Contents = "File 4"}
                }
            }.Verify(_sandbox);
        }

        [Test]
        public void EnsureSubDirDoesNotTouchFileNames()
        {
            using (var extractor = ArchiveExtractor.Create(new MemoryStream(_archiveData), _sandbox, Archive.MimeTypeZip))
            {
                extractor.Extract = "/sub/folder/nested";
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

            using (var extractor = ArchiveExtractor.Create(new MemoryStream(_archiveData), _sandbox, Archive.MimeTypeZip))
                extractor.Run();

            File.Exists(Path.Combine(_sandbox, "file0")).Should().BeTrue(because: "Extractor cleaned directory.");
            string file0Content = File.ReadAllText(Path.Combine(_sandbox, "file0"));
            file0Content.Should().Be("This file should not be touched");
            SamplePackageHierarchy.Verify(_sandbox);
        }

        /// <summary>
        /// Tests whether the extractor correctly handles a ZIP archive containing an executable file.
        /// </summary>
        [Test]
        public void TestExtractUnixArchiveWithExecutable()
        {
            using (var extractor = new ZipExtractor(typeof(ZipExtractorTest).GetEmbeddedStream("testArchive.zip"), _sandbox))
                extractor.Run();

            new TestRoot
            {
                new TestDirectory("subdir2")
                {
                    new TestFile("executable") {IsExecutable = true, LastWrite = new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc)}
                }
            }.Verify(_sandbox);
        }

        /// <summary>
        /// TTests whether the extractor correctly handles a ZIP archive containing containing a symbolic link.
        /// </summary>
        [Test]
        public void TestExtractUnixArchiveWithSymlink()
        {
            using (var extractor = new ZipExtractor(typeof(ZipExtractorTest).GetEmbeddedStream("testArchive.zip"), _sandbox))
                extractor.Run();

            new TestRoot
            {
                new TestSymlink("symlink", target: "subdir1/regular"),
                new TestDirectory("subdir1")
                {
                    new TestFile("regular") {LastWrite = new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc)}
                }
            }.Verify(_sandbox);
        }
    }
}
