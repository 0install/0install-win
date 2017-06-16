/*
 * Copyright 2010-2016 Bastian Eicher
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
using Xunit;
using ZeroInstall.Store.Implementations.Build;

namespace ZeroInstall.Store.Implementations.Archives
{
    [Collection("WorkingDir")]
    public class TarExtractorTest : IDisposable
    {
        private static readonly byte[] _garbageData = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16};

        private readonly TemporaryDirectory _sandbox = new TemporaryWorkingDirectory("0install-unit-tests");
        public void Dispose() => _sandbox.Dispose();

        [Fact]
        public void TestPlain()
        {
            TestExtract(Model.Archive.MimeTypeTar, typeof(TarExtractorTest).GetEmbeddedStream("testArchive.tar"));
        }

        [Fact]
        public void TestPlainError()
        {
            Assert.Throws<IOException>(() => TestExtract(Model.Archive.MimeTypeTar, new MemoryStream(_garbageData)));
        }

        [Fact]
        public void TestGzCompressed()
        {
            TestExtract(Model.Archive.MimeTypeTarGzip, typeof(TarExtractorTest).GetEmbeddedStream("testArchive.tar.gz"));
        }

        [Fact]
        public void TestGzCompressedError()
        {
            Assert.Throws<IOException>(() => TestExtract(Model.Archive.MimeTypeTarGzip, new MemoryStream(_garbageData)));
        }

        [Fact]
        public void TestBz2Compressed()
        {
            TestExtract(Model.Archive.MimeTypeTarBzip, typeof(TarExtractorTest).GetEmbeddedStream("testArchive.tar.bz2"));
        }

        [Fact]
        public void TestBz2CompressedError()
        {
            Assert.Throws<IOException>(() => TestExtract(Model.Archive.MimeTypeTarBzip, new MemoryStream(_garbageData)));
        }

        [SkippableFact]
        public void TestXzCompressed()
        {
            Skip.IfNot(WindowsUtils.IsWindows, ".xz decompression is currently only available on Windows");

            TestExtract(Model.Archive.MimeTypeTarXz, typeof(TarExtractorTest).GetEmbeddedStream("testArchive.tar.xz"));
        }

        [Fact]
        public void TestLzmaCompressed()
        {
            TestExtract(Model.Archive.MimeTypeTarLzma, typeof(TarExtractorTest).GetEmbeddedStream("testArchive.tar.lzma"));
        }

        [Fact]
        public void TestLzmaCompressedOnDisk()
        {
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                typeof(TarExtractorTest).GetEmbeddedStream("testArchive.tar.lzma").CopyToFile(tempFile);

                using (var stream = File.OpenRead(tempFile))
                    TestExtract(Model.Archive.MimeTypeTarLzma, stream);
            }
        }

        [Fact]
        public void TestLzmaCompressedError()
        {
            Assert.Throws<IOException>(() => TestExtract(Model.Archive.MimeTypeTarLzma, new MemoryStream(_garbageData)));
        }

        [Fact]
        public void TestRubyGem()
        {
            TestExtract(Model.Archive.MimeTypeRubyGem, typeof(TarExtractorTest).GetEmbeddedStream("testArchive.gem"));
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

    public class TarExtractorTestCornerCases : IDisposable
    {
        private readonly TemporaryDirectory _sandbox = new TemporaryDirectory("0install-unit-tests");
        public void Dispose() => _sandbox.Dispose();

        /// <summary>
        /// Tests whether the extractor generates a correct <see cref="FlagUtils.XbitFile"/> for a sample TAR archive containing an executable file.
        /// </summary>
        [Fact]
        public void TestExtractUnixArchiveWithExecutable()
        {
            using (var extractor = new TarExtractor(typeof(TarExtractorTest).GetEmbeddedStream("testArchive.tar"), _sandbox))
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
        [Fact]
        public void TestExtractUnixArchiveWithSymlink()
        {
            using (var extractor = new TarExtractor(typeof(TarExtractorTest).GetEmbeddedStream("testArchive.tar"), _sandbox))
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
        [Fact]
        public void TestExtractUnixArchiveWithHardlink()
        {
            using (var extractor = new TarExtractor(typeof(TarExtractorTest).GetEmbeddedStream("testArchive.tar"), _sandbox))
                extractor.Run();

            FileUtils.AreHardlinked(Path.Combine(_sandbox, "subdir1", "regular"), Path.Combine(_sandbox, "hardlink"))
                .Should().BeTrue(because: "'regular' and 'hardlink' should be hardlinked together");
        }
    }
}
