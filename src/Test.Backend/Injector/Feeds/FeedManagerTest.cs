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
using System.IO;
using Common.Storage;
using NUnit.Framework;
using NUnit.Mocks;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector.Feeds
{
    /// <summary>
    /// Contains test methods for <see cref="FeedManager"/>.
    /// </summary>
    [TestFixture]
    class FeedManagerTest
    {
        private DynamicMock _cacheMock;
        private DynamicMock _openPgpMock;

        private FeedManager _feedManager;
        private Policy _policy;

        [SetUp]
        public void SetUp()
        {
            _cacheMock = new DynamicMock("MockFeedCache", typeof(IFeedCache));
            _openPgpMock = new DynamicMock("MockOpenPgp", typeof(IOpenPgp));
            _feedManager = new FeedManager((IFeedCache)_cacheMock.MockInstance, (IOpenPgp)_openPgpMock.MockInstance);

            var fetcher = (IFetcher)new DynamicMock("MockFetcher", typeof(IFetcher)).MockInstance;
            var solver = (ISolver)new DynamicMock("MockSolver", typeof(ISolver)).MockInstance;
            _policy = new Policy(new Config(), _feedManager, fetcher, solver, new SilentHandler());
        }

        [TearDown]
        public void TearDown()
        {
            _cacheMock.Verify();
            _openPgpMock.Verify();
        }

        [Test(Description = "Ensures local feed files are loaded correctly.")]
        public void TestLocal()
        {
            var feed = new Feed();

            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                _cacheMock.ExpectAndReturn("GetFeed", feed, tempFile.Path);

                bool stale;
                Assert.AreSame(feed, _feedManager.GetFeed(tempFile.Path, _policy, out stale));
                Assert.IsFalse(stale);
            }
        }

        [Test(Description = "Ensures missing local feed files are reported correctly.")]
        public void TestLocalMissing()
        {
            bool stale;
            Assert.Throws<FileNotFoundException>(() => _feedManager.GetFeed("invalid-" + Path.GetRandomFileName(), _policy, out stale));
        }

        [Test(Description = "Ensures cached feeds that are not stale are returned correctly.")]
        public void TestCachedFresh()
        {
            var feed = new Feed();

            _cacheMock.ExpectAndReturn("Contains", true, "http://0install.de/feeds/test/test1.xml");
            _cacheMock.ExpectAndReturn("GetFeed", feed, "http://0install.de/feeds/test/test1.xml");

            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                // Don't store test settings in real user profile
                Locations.PortableBase = tempDir.Path;
                Locations.IsPortable = true;
                new FeedPreferences {LastChecked = DateTime.UtcNow}.SaveFor("http://0install.de/feeds/test/test1.xml");

                bool stale;
                Assert.AreSame(feed, _feedManager.GetFeed("http://0install.de/feeds/test/test1.xml", _policy, out stale));
                Assert.IsFalse(stale);

                Locations.PortableBase = Locations.InstallBase;
            }
        }

        [Test(Description = "Ensures cached feeds that are stale are returned correctly.")]
        void TestCachedStale()
        {
            var feed = new Feed();

            _cacheMock.ExpectAndReturn("Contains", true, "http://0install.de/feeds/test/test1.xml");
            _cacheMock.ExpectAndReturn("GetFeed", feed, "http://0install.de/feeds/test/test1.xml");

            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                // Don't store test settings in real user profile
                Locations.PortableBase = tempDir.Path;
                Locations.IsPortable = true;
                new FeedPreferences {LastChecked = DateTime.UtcNow - _policy.Config.Freshness}.SaveFor("http://0install.de/feeds/test/test1.xml");

                bool stale;
                Assert.AreSame(feed, _feedManager.GetFeed("http://0install.de/feeds/test/test1.xml", _policy, out stale));
                Assert.IsTrue(stale);

                Locations.PortableBase = Locations.InstallBase;
            }
        }

        //[Test(Description = "Ensures missing feeds are downloaded correctly.")]
        public void TestDownload()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                // Don't store test settings in real user profile
                Locations.PortableBase = tempDir.Path;
                Locations.IsPortable = true;

                // ToDo

                Locations.PortableBase = Locations.InstallBase;
            }
        }
    }
}
