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
            SetUpSandboxFolder();
        }

        private void SetUpSandboxFolder()
        {
            _sandbox = new TemporaryDirectoryReplacement(Path.Combine(Path.GetTempPath(), "tarExtraction-Basic"));
            _oldWorkingDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = _sandbox.Path;
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
        /// Tests whether the tar extractor generates a correct .xbit file for
        /// an example of a unix archive containing an executable file.
        /// </summary>
        [Test]
        public void TestExtractUnixArchiveWithExecutable()
        {
            using (var archive = TestData.GetSdlTarArchiveStream())
                using (var extractor = new TarExtractor(archive, 0, _sandbox.Path))
                    extractor.RunSync();

            string xbitFileContent = File.ReadAllText(Path.Combine(_sandbox.Path, ".xbit")).Trim();
            Assert.AreEqual("/SDL.dll", xbitFileContent);
        }
    }
}
