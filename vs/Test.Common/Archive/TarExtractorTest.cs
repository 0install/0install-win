using System;
using NUnit.Framework;
using System.IO;
using Common.Storage;
using Common.Helpers;

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
    }
}
