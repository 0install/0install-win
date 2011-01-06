/*
 * Copyright 2010 Bastian Eicher
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

using System;
using NUnit.Framework;
using NUnit.Mocks;
using ZeroInstall.Fetchers;
using ZeroInstall.Launcher.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Launcher
{
    /// <summary>
    /// Contains test methods for <see cref="Controller"/>.
    /// </summary>
    [TestFixture]
    public class ControllerTest
    {
        #region Shared
        // Dummy data used by the tests
        private const string TestUri = "http://0install.de/feeds/test/test1.xml";

        private DynamicMock _solverMock;
        private DynamicMock _cacheMock;
        private DynamicMock _storeMock;
        private DynamicMock _fetcherMock;

        private ISolver TestSolver { get { return (ISolver)_solverMock.MockInstance; } }
        private Policy CreateTestPolicy()
        {
            return new Policy(new FeedManager((IFeedCache)_cacheMock.MockInstance), (IFetcher)_fetcherMock.MockInstance);
        }
        
        [SetUp]
        public void SetUp()
        {
            // Prepare mock objects that will be injected with methods in the tests
            _solverMock = new DynamicMock("MockSolver", typeof(ISolver));
            _cacheMock = new DynamicMock("MockCache", typeof(IFeedCache));
            _storeMock = new DynamicMock("StoreCache", typeof(IStore));
            _fetcherMock = new DynamicMock("MockFetcher", typeof(IFetcher));
            _fetcherMock.SetReturnValue("get_Store", _storeMock.MockInstance);
        }

        [TearDown]
        public void TearDown()
        {
            // Ensure no method calls were left out
            _solverMock.Verify();
            _cacheMock.Verify();
            _storeMock.Verify();
            _fetcherMock.Verify();
        }
        #endregion

        /// <summary>
        /// Ensures the <see cref="Controller"/> constructor, <see cref="Controller.GetSelections"/> and <see cref="Controller.GetExecutor"/> throw the correct exceptions.
        /// </summary>
        [Test]
        public void TestExceptions()
        {
            Assert.Throws<ArgumentException>(() => new Controller("invalid", SolverProvider.Default, Policy.CreateDefault(), new SilentHandler()));

            var controller = new Controller("http://nothing", SolverProvider.Default, Policy.CreateDefault(), new SilentHandler());
            Assert.Throws<InvalidOperationException>(() => controller.GetSelections(), "GetSelections should depend on Solve being called first");
            Assert.Throws<InvalidOperationException>(() => controller.GetExecutor(), "GetRun should depend on Solve being called first");
        }

        [Test]
        public void TestSolve()
        {
            var policy = CreateTestPolicy();

            var handler = new SilentHandler();
            _solverMock.ExpectAndReturn("Solve", SelectionsTest.CreateTestSelections(), TestUri, policy, handler);
            var controller = new Controller(TestUri, TestSolver, policy, handler);
            controller.Solve();
        }

        [Test]
        public void TestGetSetSelections()
        {
            var controller = new Controller("http://nothing", TestSolver, CreateTestPolicy(), new SilentHandler());
            
            var selections = SelectionsTest.CreateTestSelections();
            controller.SetSelections(selections);
            Assert.AreEqual(selections, controller.GetSelections());
        }

        /// <summary>
        /// Ensures that <see cref="Controller.ListUncachedImplementations"/> correctly finds <see cref="Model.Implementation"/>s not cached in an <see cref="IStore"/>.
        /// </summary>
        [Test]
        public void TestListUncachedImplementations()
        {
            var feed = FeedTest.CreateTestFeed();
            var selections = SelectionsTest.CreateTestSelections();

            _cacheMock.ExpectAndReturn("GetFeed", feed, new Uri("http://0install.de/feeds/test/sub1.xml"));

            // Pretend the first implementation isn't cached but the second is
            _storeMock.ExpectAndReturn("Contains", false, selections.Implementations[0].ManifestDigest);
            _storeMock.ExpectAndReturn("Contains", true, selections.Implementations[1].ManifestDigest);

            var controller = new Controller(TestUri, TestSolver, CreateTestPolicy(), new SilentHandler());
            controller.SetSelections(selections);

            // Only the first implementation should be listed as uncached
            CollectionAssert.AreEquivalent(new[] {feed.Elements[0]}, controller.ListUncachedImplementations());
        }

        /// <summary>
        /// Ensures that <see cref="Controller.DownloadUncachedImplementations"/> correctly tries to download <see cref="Model.Implementation"/>s not cached in an <see cref="IStore"/>.
        /// </summary>
        [Test]
        public void TestDownloadUncachedImplementations()
        {
            var feed = FeedTest.CreateTestFeed();
            var selections = SelectionsTest.CreateTestSelections();
            var handler = new SilentHandler();

            _cacheMock.ExpectAndReturn("GetFeed", feed, new Uri("http://0install.de/feeds/test/sub1.xml"));

            // Pretend the first implementation isn't cached but the second is
            _storeMock.ExpectAndReturn("Contains", false, selections.Implementations[0].ManifestDigest);
            _storeMock.ExpectAndReturn("Contains", true, selections.Implementations[1].ManifestDigest);

            _fetcherMock.Expect("RunSync", new FetchRequest(new[] {(Implementation)feed.Elements[0]}), handler);

            var controller = new Controller(TestUri, TestSolver, CreateTestPolicy(), handler);
            controller.SetSelections(selections);

            // Only the first implementation should be "downloaded"
            controller.DownloadUncachedImplementations();
        }
    }
}
