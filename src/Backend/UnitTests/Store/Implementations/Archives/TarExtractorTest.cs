/*
 * Copyright 2010-2016 Bastian Eicher, Roland Leopold Walkling
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

namespace ZeroInstall.Store.Implementations.Archives
{
    [TestFixture]
    public class TarExtractorTestBasicFunctionality
    {
        private TemporaryDirectory _sandbox;
        private static readonly byte[] _garbageData = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16};

        [SetUp]
        public void SetUp()
        {
            _sandbox = new TemporaryWorkingDirectory("0install-unit-tests");
        }

        [TearDown]
        public void TearDown()
        {
            _sandbox.Dispose();
        }

        [Test]
        public void TestPlain()
        {
            TestExtract(Model.Archive.MimeTypeTar, this.GetEmbedded("testArchive.tar"));
        }

        [Test]
        public void TestPlainError()
        {
            Assert.Throws<IOException>(() => TestExtract(Model.Archive.MimeTypeTar, new MemoryStream(_garbageData)));
        }

        [Test]
        public void TestGzCompressed()
        {
            TestExtract(Model.Archive.MimeTypeTarGzip, this.GetEmbedded("testArchive.tar.gz"));
        }

        [Test]
        public void TestGzCompressedError()
        {
            Assert.Throws<IOException>(() => TestExtract(Model.Archive.MimeTypeTarGzip, new MemoryStream(_garbageData)));
        }

        [Test]
        public void TestBz2Compressed()
        {
            TestExtract(Model.Archive.MimeTypeTarBzip, this.GetEmbedded("testArchive.tar.bz2"));
        }

        [Test]
        public void TestBz2CompressedError()
        {
            Assert.Throws<IOException>(() => TestExtract(Model.Archive.MimeTypeTarBzip, new MemoryStream(_garbageData)));
        }

        [Test]
        public void TestXzCompressed()
        {
            if (!WindowsUtils.IsWindows) Assert.Ignore(".xz decompression is currently only available on Windows");

            TestExtract(Model.Archive.MimeTypeTarXz, this.GetEmbedded("testArchive.tar.xz"));
        }

        [Test]
        public void TestLzmaCompressed()
        {
            TestExtract(Model.Archive.MimeTypeTarLzma, this.GetEmbedded("testArchive.tar.lzma"));
        }

        [Test]
        public void TestLzmaCompressedOnDisk()
        {
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                this.GetEmbedded("testArchive.tar.lzma").CopyToFile(tempFile);

                using (var stream = File.OpenRead(tempFile))
                    TestExtract(Model.Archive.MimeTypeTarLzma, stream);
            }
        }

        [Test]
        public void TestLzmaCompressedError()
        {
            Assert.Throws<IOException>(() => TestExtract(Model.Archive.MimeTypeTarLzma, new MemoryStream(_garbageData)));
        }

        [Test]
        public void TestRubyGem()
        {
            TestExtract(Model.Archive.MimeTypeRubyGem, this.GetEmbedded("testArchive.gem"));
        }

        private void TestExtract(string mimeType, Stream archive)
        {
            using (var extractor = ArchiveExtractor.Create(archive, _sandbox, mimeType))
                extractor.Run();

            File.Exists("subdir1/regular").Should().BeTrue(because: "Should extract file 'regular'");
            File.GetLastWriteTimeUtc("subdir1/regular").Should().Be(new DateTime(2000, 1, 1, 12, 0, 0), because: "Correct last write time for file 'regular' should be set");

            File.Exists("subdir2/executable").Should().BeTrue(because: "Should extract file 'executable'");
            File.GetLastWriteTimeUtc("subdir2/executable").Should().Be(new DateTime(2000, 1, 1, 12, 0, 0), because: "Correct last write time for file 'executable' should be set");
        }
    }

    [TestFixture]
    public class TarExtractorTestCornerCases
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
        /// Tests whether the extractor generates a correct <see cref="FlagUtils.XbitFile"/> for a sample TAR archive containing an executable file.
        /// </summary>
        [Test]
        public void TestExtractUnixArchiveWithExecutable()
        {
            using (var extractor = new TarExtractor(this.GetEmbedded("testArchive.tar"), _sandbox))
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
        /// Tests whether the extractor generates a <see cref="FileUtils.IsSymlink(string)"/> entry for a sample TAR archive containing a symbolic link.
        /// </summary>
        [Test]
        public void TestExtractUnixArchiveWithSymlink()
        {
            using (var extractor = new TarExtractor(this.GetEmbedded("testArchive.tar"), _sandbox))
                extractor.Run();

            string target;
            string source = Path.Combine(_sandbox, "symlink");
            if (UnixUtils.IsUnix) FileUtils.IsSymlink(source, out target).Should().BeTrue();
            else CygwinUtils.IsSymlink(source, out target).Should().BeTrue();

            target.Should().Be("subdir1/regular", because: "Symlink should point to 'regular'");
        }

        /// <summary>
        /// Tests whether the extractor creates an on-disk hardlink for a sample TAR archive containing a hardlink.
        /// </summary>
        [Test]
        public void TestExtractUnixArchiveWithHardlink()
        {
            using (var extractor = new TarExtractor(this.GetEmbedded("testArchive.tar"), _sandbox))
                extractor.Run();

            FileUtils.AreHardlinked(Path.Combine(_sandbox, "subdir1", "regular"), Path.Combine(_sandbox, "hardlink"))
                .Should().BeTrue(because: "'regular' and 'hardlink' should be hardlinked together");
        }
    }
}
