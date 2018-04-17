// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using Moq;
using Xunit;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Contains integration tests for <see cref="List"/>.
    /// </summary>
    public class ListTest : CliCommandTest<List>
    {
        private Mock<IFeedCache> FeedCacheMock => GetMock<IFeedCache>();

        [Fact] // Ensures calling with no arguments returns all feeds in the cache.
        public void TestNoArgs()
        {
            FeedCacheMock.Setup(x => x.ListAll()).Returns(new[] {Fake.Feed1Uri, Fake.Feed2Uri});
            RunAndAssert(new[] {Fake.Feed1Uri.ToStringRfc(), Fake.Feed2Uri.ToStringRfc()}, ExitCode.OK);
        }

        [Fact] // Ensures calling with a single argument returns a filtered list of feeds in the cache.
        public void TestPattern()
        {
            FeedCacheMock.Setup(x => x.ListAll()).Returns(new[] {Fake.Feed1Uri, Fake.Feed2Uri});
            RunAndAssert(new[] {Fake.Feed2Uri.ToStringRfc()}, ExitCode.OK, "test2");
        }
    }
}
