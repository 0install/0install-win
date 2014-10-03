/*
 * Copyright 2010-2014 Bastian Eicher, Roland Leopold Walkling
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
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
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
            using (var extractor = Extractor.FromStream(TestData.GetResource("testArchive.cab"), sandbox, Archive.MimeTypeCab))
            {
                extractor.Run();

                string filePath = Path.Combine(sandbox, "file");
                Assert.IsTrue(File.Exists(filePath), "Should extract file 'file'");
                Assert.AreEqual(new DateTime(2000, 1, 1, 13, 0, 0), File.GetLastWriteTimeUtc(filePath), "Correct last write time should be set");
                Assert.AreEqual("abc", File.ReadAllText(filePath));

                filePath = Path.Combine(sandbox, Path.Combine("folder1", "file"));
                Assert.IsTrue(File.Exists(filePath), "Should extract file 'dir/file'");
                Assert.AreEqual(new DateTime(2000, 1, 1, 13, 0, 0), File.GetLastWriteTimeUtc(filePath), "Correct last write time should be set");
                Assert.AreEqual("def", File.ReadAllText(filePath));
            }
        }

        [Test]
        public void TestExtractSubDir()
        {
            if (!WindowsUtils.IsWindows) Assert.Ignore("CAB extraction relies on a Win32 API and therefore will not work on non-Windows platforms");

            using (var sandbox = new TemporaryDirectory("0install-unit-tests"))
            using (var extractor = Extractor.FromStream(TestData.GetResource("testArchive.cab"), sandbox, Archive.MimeTypeCab))
            {
                extractor.SubDir = "folder1";
                extractor.Run();

                string filePath = Path.Combine(sandbox, "file");
                Assert.IsTrue(File.Exists(filePath), "Should extract file 'dir/file'");
                Assert.AreEqual(new DateTime(2000, 1, 1, 13, 0, 0), File.GetLastWriteTimeUtc(filePath), "Correct last write time should be set");
                Assert.AreEqual("def", File.ReadAllText(filePath));
            }
        }

        private static readonly byte[] _garbageData = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16};

        [Test]
        public void TestExtractInvalidData()
        {
            if (!WindowsUtils.IsWindows) Assert.Ignore("CAB extraction relies on a Win32 API and therefore will not work on non-Windows platforms");

            using (var sandbox = new TemporaryDirectory("0install-unit-tests"))
                Assert.Throws<IOException>(() => Extractor.FromStream(new MemoryStream(_garbageData), sandbox, Archive.MimeTypeCab));
        }
    }
}
