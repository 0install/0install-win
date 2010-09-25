using System;
using NUnit.Framework;
using System.IO;
using Common.Storage;

namespace Common.Archive
{
    [TestFixture]
    class TarTestBasicFunctionality
    {
        TemporaryDirectoryReplacement _sandbox;
        string _oldWorkingDirectory;

        [SetUp]
        public void SetUp()
        {
            _sandbox = new TemporaryDirectoryReplacement(Path.Combine(Path.GetTempPath(), "tarExtraction-Basic"));
            _oldWorkingDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = _sandbox.Path;
        }

        [TearDown]
        public void TearDown()
        {
            Environment.CurrentDirectory = _oldWorkingDirectory;
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
        private TemporaryDirectoryReplacement _sandbox;

        [SetUp]
        public void SetUp()
        {
            _sandbox = new TemporaryDirectoryReplacement(Path.Combine(Path.GetTempPath(), "tarExtraction-Corner"));
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

            string xbitFileContent = File.ReadAllText(Path.Combine(_sandbox.Path, ".xbit")).Trim();
            Assert.AreEqual("/SDL.dll", xbitFileContent);
        }
    }
}
