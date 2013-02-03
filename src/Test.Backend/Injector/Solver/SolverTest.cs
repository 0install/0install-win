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
using Moq;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Contains common code for testing specific <see cref="ISolver"/> implementations.
    /// </summary>
    public abstract class SolverTest
    {
        #region Shared
        private readonly ISolver _solver;
        //private Mock<IFeedCache> _cacheMock;

        private Policy _policy;
        private LocationsRedirect _redirect;

        [SetUp]
        public void SetUp()
        {
            // Don't store generated executables settings in real user profile
            _redirect = new LocationsRedirect("0install-unit-tests");

            //_cacheMock = new Mock<IFeedCache>(MockBehavior.Strict);
            _policy = new Policy(
                new Config(), new FeedManager(FeedCacheProvider.CreateDefault()),
                new Mock<IFetcher>().Object, new Mock<IOpenPgp>().Object, _solver, new SilentHandler());
        }

        [TearDown]
        public void TearDown()
        {
            _redirect.Dispose();

            //_cacheMock.Verify();
        }
        #endregion

        protected SolverTest(ISolver solver)
        {
            _solver = solver;
        }

        private static Feed CreateTestFeed()
        {
            return new Feed {Name = "Test", Summaries = {"Test"}, Elements = {new Implementation {ID = "test", Version = new ImplementationVersion("1.0"), LocalPath = ".", Main = "test"}}};
        }

        [Test]
        public void TestBasic()
        {
            using (var feedFile = new TemporaryFile("0install-unit-tests"))
            {
                var testFeed = CreateTestFeed();
                testFeed.Normalize(feedFile.Path);
                //_cacheMock.Setup(x => x.Contains(feedFile.Path)).Returns(true);
                //_cacheMock.Setup(x => x.GetFeed(feedFile.Path)).Returns(testFeed);
                testFeed.SaveXml(feedFile.Path);

                bool staleFeeds;
                /*var selections =*/
                _solver.Solve(new Requirements {InterfaceID = feedFile.Path}, _policy, out staleFeeds);
            }
        }
    }
}
