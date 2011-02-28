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
using Common.Storage;
using NUnit.Framework;
using NUnit.Mocks;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Contains test methods for <see cref="MemoryFeedCache"/>.
    /// </summary>
    [TestFixture]
    public class MemoryFeedCacheTest
    {
        private DynamicMock _cacheMock;
        private MemoryFeedCache _cache;
        private Feed _feed;

        [SetUp]
        public void SetUp()
        {
            _cacheMock = new DynamicMock("MockCache", typeof(IFeedCache));
            _cache = new MemoryFeedCache((IFeedCache)_cacheMock.MockInstance);

            // Create a dummy feed
            _feed = FeedTest.CreateTestFeed();
            _feed.Uri = new Uri("http://0install.de/feeds/test/test1.xml");
        }

        [TearDown]
        public void TearDown()
        {
            _cacheMock.Verify();
        }

        [Test]
        public void TestContains()
        {
            // Expect simple pass-through
            _cacheMock.ExpectAndReturn("Contains", true, "http://0install.de/feeds/test/test1.xml");
            _cacheMock.ExpectAndReturn("Contains", true, "http://0install.de/feeds/test/test2.xml");
            _cacheMock.ExpectAndReturn("Contains", false, "http://0install.de/feeds/test/test3.xml");
            Assert.IsTrue(_cache.Contains("http://0install.de/feeds/test/test1.xml"));
            Assert.IsTrue(_cache.Contains("http://0install.de/feeds/test/test2.xml"));
            Assert.IsFalse(_cache.Contains("http://0install.de/feeds/test/test3.xml"));
        }

        [Test]
        public void TestListAll()
        {
            // Expect simple pass-through
            var feeds = new[] {"http://0install.de/feeds/test/test1.xml", "http://0install.de/feeds/test/test2.xml"};
            _cacheMock.ExpectAndReturn("ListAll", feeds);
            CollectionAssert.AreEqual(feeds, _cache.ListAll());
        }

        [Test]
        public void TestGetFeed()
        {
            // Expect pass-through on first access
            _cacheMock.ExpectAndReturn("GetFeed", _feed, "http://0install.de/feeds/test/test1.xml");
            Feed firstAccess = _cache.GetFeed("http://0install.de/feeds/test/test1.xml");
            Assert.AreEqual(_feed, firstAccess);

            // Expect  in-memory cache on second access
            Feed secondAccess = _cache.GetFeed("http://0install.de/feeds/test/test1.xml");
            Assert.AreSame(firstAccess, secondAccess);
        }

        [Test]
        public void TestAdd()
        {
            using (var feedFile = new TemporaryFile("0install-unit-tests"))
            {
                // Expect pass-through on adding
                _cacheMock.ExpectAndReturn("Add", null, "http://0install.de/feeds/test/test1.xml", feedFile.Path);
                _feed.Save(feedFile.Path);
                _cache.Add("http://0install.de/feeds/test/test1.xml", feedFile.Path);
            }

            // Expect in-memory cache on get
            Feed firstAccess = _cache.GetFeed("http://0install.de/feeds/test/test1.xml");
            Assert.AreEqual(_feed, firstAccess);
            Feed secondAccess = _cache.GetFeed("http://0install.de/feeds/test/test1.xml");
            Assert.AreSame(firstAccess, secondAccess, "Cache should return identical reference on multiple GetFeed() calls");

            using (var feedFile = new TemporaryFile("0install-unit-tests"))
            {
                // Expect pass-through on adding
                _cacheMock.ExpectAndReturn("Add", null, "http://0install.de/feeds/test/test1.xml", feedFile.Path);
                _feed.Save(feedFile.Path);
                _cache.Add("http://0install.de/feeds/test/test1.xml", feedFile.Path);
            }

            Assert.AreNotSame(firstAccess, _cache.GetFeed("http://0install.de/feeds/test/test1.xml"), "Adding again should overwrite cache entry");
        }

        [Test]
        public void TestRemove()
        {
            using (var feedFile = new TemporaryFile("0install-unit-tests"))
            {
                // Expect pass-through on adding
                _cacheMock.ExpectAndReturn("Add", null, "http://0install.de/feeds/test/test1.xml", feedFile.Path);
                _feed.Save(feedFile.Path);
                _cache.Add("http://0install.de/feeds/test/test1.xml", feedFile.Path);
            }

            // Expect in-memory cache on get
            Assert.AreEqual(_feed, _cache.GetFeed("http://0install.de/feeds/test/test1.xml"));

            // Expect pass-through on remove
            _cacheMock.Expect("Remove", "http://0install.de/feeds/test/test1.xml");
            _cache.Remove("http://0install.de/feeds/test/test1.xml");

            // Expect pass-through after remove
            _cacheMock.ExpectAndReturn("GetFeed", _feed, "http://0install.de/feeds/test/test1.xml");
            Assert.AreEqual(_feed, _cache.GetFeed("http://0install.de/feeds/test/test1.xml"));
        }
    }
}
