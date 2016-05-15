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
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.Implementations.Archives
{
    [TestFixture]
    public class CabExtractorTest
    {
        [Test]
        public void TestExtract()
        {
            if (!WindowsUtils.IsWindows) Assert.Ignore("CAB extraction relies on a Win32 API and therefore will not work on non-Windows platforms");

            using (var sandbox = new TemporaryDirectory("0install-unit-tests"))
            using (var extractor = ArchiveExtractor.Create(typeof(CabExtractorTest).GetEmbeddedStream("testArchive.cab"), sandbox, Archive.MimeTypeCab))
            {
                extractor.Run();

                string filePath = Path.Combine(sandbox, "file");
                File.Exists(filePath).Should().BeTrue(because: "Should extract file 'file'");
                File.GetLastWriteTimeUtc(filePath).Should().Be(new DateTime(2000, 1, 1, 13, 0, 0), because: "Correct last write time should be set");
                File.ReadAllText(filePath).Should().Be("abc");

                filePath = Path.Combine(sandbox, Path.Combine("folder1", "file"));
                File.Exists(filePath).Should().BeTrue(because: "Should extract file 'dir/file'");
                File.GetLastWriteTimeUtc(filePath).Should().Be(new DateTime(2000, 1, 1, 13, 0, 0), because: "Correct last write time should be set");
                File.ReadAllText(filePath).Should().Be("def");
            }
        }

        [Test]
        public void TestExtractSubDir()
        {
            if (!WindowsUtils.IsWindows) Assert.Ignore("CAB extraction relies on a Win32 API and therefore will not work on non-Windows platforms");

            using (var sandbox = new TemporaryDirectory("0install-unit-tests"))
            using (var extractor = ArchiveExtractor.Create(typeof(CabExtractorTest).GetEmbeddedStream("testArchive.cab"), sandbox, Archive.MimeTypeCab))
            {
                extractor.SubDir = "folder1";
                extractor.Run();

                string filePath = Path.Combine(sandbox, "file");
                File.Exists(filePath).Should().BeTrue(because: "Should extract file 'dir/file'");
                File.GetLastWriteTimeUtc(filePath).Should().Be(new DateTime(2000, 1, 1, 13, 0, 0), because: "Correct last write time should be set");
                File.ReadAllText(filePath).Should().Be("def");
            }
        }

        private static readonly byte[] _garbageData = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16};

        [Test]
        public void TestExtractInvalidData()
        {
            if (!WindowsUtils.IsWindows) Assert.Ignore("CAB extraction relies on a Win32 API and therefore will not work on non-Windows platforms");

            using (var sandbox = new TemporaryDirectory("0install-unit-tests"))
                Assert.Throws<IOException>(() => ArchiveExtractor.Create(new MemoryStream(_garbageData), sandbox, Archive.MimeTypeCab));
        }
    }
}
