using System;
using NanoByte.Common.Storage;

namespace ZeroInstall.Store.Implementations.Build
{
    /// <summary>
    /// Common base class for tests that compare two directories.
    /// </summary>
    public abstract class CloneTestBase : IDisposable
    {
        protected readonly TemporaryDirectory SourceDirectory = new TemporaryDirectory("0install-unit-tests");
        protected readonly TemporaryDirectory TargetDirectory = new TemporaryDirectory("0install-unit-tests");

        public void Dispose()
        {
            SourceDirectory.Dispose();
            TargetDirectory.Dispose();
        }
    }
}