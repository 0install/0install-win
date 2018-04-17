// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using Moq;

namespace ZeroInstall
{
    /// <summary>
    /// Common base class for test fixtures that use a <see cref="Moq.MockRepository"/>.
    /// </summary>
    public abstract class TestWithMocks : IDisposable
    {
        protected readonly MockRepository MockRepository = new MockRepository(MockBehavior.Strict);

        /// <summary>
        /// Creates a new <see cref="Mock"/> for a specific type. Multiple requests for the same type return new mock instances each time.
        /// </summary>
        /// <remarks>All created <see cref="Mock"/>s are automatically <see cref="Mock.Verify"/>d after the test completes.</remarks>
        protected Mock<T> CreateMock<T>() where T : class => MockRepository.Create<T>();

        public virtual void Dispose() => MockRepository.VerifyAll();
    }
}
