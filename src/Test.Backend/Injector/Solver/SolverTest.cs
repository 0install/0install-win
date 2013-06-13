/*
 * Copyright 2010-2013 Bastian Eicher
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

using Common.Storage;
using NUnit.Framework;
using ZeroInstall.Model;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Contains common code for testing specific <see cref="ISolver"/> implementations.
    /// </summary>
    public abstract class SolverTest<T> : TestWithResolver<T> where T : class, ISolver
    {
        #region Helpers
        private static Feed CreateTestFeed()
        {
            return new Feed
            {
                Name = "Test",
                Summaries = {"Test"},
                Elements = {new Implementation {ID = "test", Version = new ImplementationVersion("1.0"), LocalPath = ".", Main = "test"}}
            };
        }
        #endregion

        [Test]
        public void TestBasic()
        {
            using (var feedFile = new TemporaryFile("0install-unit-tests"))
            {
                var testFeed = CreateTestFeed();
                testFeed.Normalize(feedFile);
                var feedManagerMock = Resolver.GetMock<IFeedManager>();
                bool temp = false;
                feedManagerMock.Setup(x => x.GetFeed(feedFile, ref temp)).Returns(testFeed).Verifiable();
                testFeed.SaveXml(feedFile);

                bool staleFeeds;

                ProvideCancellationToken();
                var selections = Target.Solve(new Requirements {InterfaceID = feedFile}, out staleFeeds);
            }
        }
    }
}
