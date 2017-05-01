using NanoByte.Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Store.Implementations.Build
{
    /// <summary>
    /// Common base class for tests that compare two directories.
    /// </summary>
    public abstract class CloneTestBase
    {
        protected TemporaryDirectory SourceDirectory { get; private set; }
        protected TemporaryDirectory TargetDirectory { get; private set; }

        [SetUp]
        public void SetUp()
        {
            SourceDirectory = new TemporaryDirectory("0install-unit-tests");
            TargetDirectory = new TemporaryDirectory("0install-unit-tests");
        }

        [TearDown]
        public void TearDown()
        {
            SourceDirectory.Dispose();
            TargetDirectory.Dispose();
        }
    }
}