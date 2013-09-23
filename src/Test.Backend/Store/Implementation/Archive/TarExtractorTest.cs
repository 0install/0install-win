/*
 * Copyright 2010-2013 Bastian Eicher, Roland Leopold Walkling
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
using Common.Utils;
using NUnit.Framework;
using System.IO;
using Common.Storage;

namespace ZeroInstall.Store.Implementation.Archive
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
            using (var archive = TestData.GetResource("testArchive.tar"))
                TestExtract(Model.Archive.MimeTypeTar, archive);
        }

        [Test]
        public void TestPlainError()
        {
            Assert.Throws<IOException>(() => TestExtract(Model.Archive.MimeTypeTar, new MemoryStream(_garbageData)));
        }

        [Test]
        public void TestGzCompressed()
        {
            using (var archive = TestData.GetResource("testArchive.tar.gz"))
                TestExtract(Model.Archive.MimeTypeTarGzip, archive);
        }

        [Test]
        public void TestGzCompressedError()
        {
            Assert.Throws<IOException>(() => TestExtract(Model.Archive.MimeTypeTarGzip, new MemoryStream(_garbageData)));
        }

        [Test]
        public void TestBz2Compressed()
        {
            using (var archive = TestData.GetResource("testArchive.tar.bz2"))
                TestExtract(Model.Archive.MimeTypeTarBzip, archive);
        }

        [Test]
        public void TestBz2CompressedError()
        {
            Assert.Throws<IOException>(() => TestExtract(Model.Archive.MimeTypeTarBzip, new MemoryStream(_garbageData)));
        }

        [Test]
        public void TestLzmaCompressed()
        {
            using (var archive = TestData.GetResource("testArchive.tar.lzma"))
                TestExtract(Model.Archive.MimeTypeTarLzma, archive);
        }

        [Test]
        public void TestLzmaCompressedError()
        {
            Assert.Throws<IOException>(() => TestExtract(Model.Archive.MimeTypeTarLzma, new MemoryStream(_garbageData)));
        }

        [Test]
        public void TestRubyGem()
        {
            using (var archive = TestData.GetResource("testArchive.gem"))
                TestExtract(Model.Archive.MimeTypeRubyGem, archive);
        }

        private void TestExtract(string mimeType, Stream archive)
        {
            using (var extractor = Extractor.CreateExtractor(mimeType, archive, _sandbox))
                extractor.RunSync();

            Assert.IsTrue(File.Exists("subdir1/regular"), "Should extract file 'regular'");
            Assert.AreEqual(new DateTime(2000, 1, 1, 12, 0, 0), File.GetLastWriteTimeUtc("subdir1/regular"), "Correct last write time for file 'regular' should be set");

            Assert.IsTrue(File.Exists("subdir2/executable"), "Should extract file 'executable'");
            Assert.AreEqual(new DateTime(2000, 1, 1, 12, 0, 0), File.GetLastWriteTimeUtc("subdir2/executable"), "Correct last write time for file 'executable' should be set");
        }

        [Test]
        public void TestHardlink()
        {
            using (var archive = TestData.GetResource("testArchiveHardlink.tar"))
            using (var extractor = Extractor.CreateExtractor(Model.Archive.MimeTypeTar, archive, _sandbox))
                extractor.RunSync();

            Assert.AreEqual("data", File.ReadAllText("file1"));
            Assert.AreEqual("data", File.ReadAllText("file2"));
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
        /// Tests whether the extractor generates a correct .xbit file for a sample TAR archive containing an executable file.
        /// </summary>
        [Test]
        public void TestExtractUnixArchiveWithExecutable()
        {
            using (var archive = TestData.GetResource("testArchive.tar"))
            using (var extractor = new TarExtractor(archive, _sandbox))
                extractor.RunSync();

            if (MonoUtils.IsUnix)
                Assert.IsTrue(FileUtils.IsExecutable(Path.Combine(_sandbox, "subdir2/executable")), "File 'executable' should be marked as executable");
            else
            {
                string xbitFileContent = File.ReadAllText(Path.Combine(_sandbox, ".xbit")).Trim();
                Assert.AreEqual("/subdir2/executable", xbitFileContent);
            }
        }

        /// <summary>
        /// Tests whether the extractor generates a correct .symlink file for a sample TAR archive containing a symbolic link.
        /// </summary>
        [Test]
        public void TestExtractUnixArchiveWithSymlink()
        {
            using (var archive = TestData.GetResource("testArchive.tar"))
            using (var extractor = new TarExtractor(archive, _sandbox))
                extractor.RunSync();

            string target;
            if (MonoUtils.IsUnix)
                Assert.IsTrue(FileUtils.IsSymlink(Path.Combine(_sandbox, "symlink"), out target));
            else
            {
                string symlinkFileContent = File.ReadAllText(Path.Combine(_sandbox, ".symlink")).Trim();
                Assert.AreEqual("/symlink", symlinkFileContent);
                target = File.ReadAllText(Path.Combine(_sandbox, "symlink"));
            }
            Assert.AreEqual("subdir1/regular", target, "Symlink should point to 'regular'");
        }
    }
}
