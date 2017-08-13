/*
 * Copyright 2010-2016 Bastian Eicher
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
