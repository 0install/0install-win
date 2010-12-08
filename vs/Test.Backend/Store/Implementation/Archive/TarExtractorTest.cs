/*
 * Copyright 2010 Bastian Eicher, Roland Leopold Walkling
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
    public class TarTestBasicFunctionality
    {
        TemporaryDirectory _sandbox;
        string _oldWorkingDirectory;

        [SetUp]
        public void SetUp()
        {
            _sandbox = new TemporaryDirectory("0install-unit-tests");
            _oldWorkingDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = _sandbox.Path;
        }

        [TearDown]
        public void TearDown()
        {
            Environment.CurrentDirectory = _oldWorkingDirectory;
            _sandbox.Dispose();
        }

        [Test]
        public void TestGzCompressed()
        {
            using (var archive = TestData.GetSdlTarGzArchiveStream())
            using (var extractor = new TarGzExtractor(archive, _sandbox.Path))
                extractor.RunSync();

            Assert.IsTrue(File.Exists("SDL.dll"));
            Assert.IsTrue(File.Exists("README-SDL.txt"));
        }

        [Test]
        public void TestBz2Compressed()
        {
            using (var archive = TestData.GetSdlTarBz2ArchiveStream())
            using (var extractor = new TarBz2Extractor(archive, _sandbox.Path))
                extractor.RunSync();

            Assert.IsTrue(File.Exists("SDL.dll"));
            Assert.IsTrue(File.Exists("README-SDL.txt"));
        }

        [Test]
        public void TestLzmaCompressed()
        {
            using (var archive = TestData.GetSdlTarLzmaArchiveStream())
            using (var extractor = new TarLzmaExtractor(archive, _sandbox.Path))
                extractor.RunSync();

            Assert.IsTrue(File.Exists("SDL.dll"));
            Assert.IsTrue(File.Exists("README-SDL.txt"));
        }
    }

    [TestFixture]
    class TarTestCornerCases
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
            using (var archive = TestData.GetSdlTarArchiveStream())
            using (var extractor = new TarExtractor(archive, _sandbox.Path))
                extractor.RunSync();

            if (MonoUtils.IsUnix)
            {
                Assert.IsTrue(FileUtils.IsExecutable(Path.Combine(_sandbox.Path, "SDL.dll")));
            }
            else
            {
                string xbitFileContent = File.ReadAllText(Path.Combine(_sandbox.Path, ".xbit")).Trim();
                Assert.AreEqual("/SDL.dll", xbitFileContent);
            }
        }
    }
}
