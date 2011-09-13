/*
 * Copyright 2010-2011 Bastian Eicher
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
        public void TestGetSignautes()
        {
            var result = new OpenPgpSignature[0];
            var openPgpMock = new DynamicMock(typeof(IOpenPgp));
            var openPgp = (IOpenPgp)openPgpMock.MockInstance;

            // Expect pass-through
            _cacheMock.ExpectAndReturn("GetSignatures", result, "http://0install.de/feeds/test/test1.xml", openPgp);
            var signatures = _cache.GetSignatures("http://0install.de/feeds/test/test1.xml", openPgp);

            CollectionAssert.AreEqual(signatures, result);
            openPgpMock.Verify();
        }

        [Test]
        public void TestAdd()
        {
            using (var feedStream = new MemoryStream())
            {
                _feed.Save(feedStream);
                feedStream.Position = 0;

                // Expect pass-through on adding
                _cacheMock.Expect("Add", "http://0install.de/feeds/test/test1.xml", feedStream);
                _cache.Add("http://0install.de/feeds/test/test1.xml", feedStream);
            }

            // Expect no pass-through due to caching on .Add()
            _feed.Simplify();
            Feed firstAccess = _cache.GetFeed("http://0install.de/feeds/test/test1.xml");
            Assert.AreEqual(_feed, firstAccess);
            Feed secondAccess = _cache.GetFeed("http://0install.de/feeds/test/test1.xml");
            Assert.AreSame(firstAccess, secondAccess, "Cache should return identical reference on multiple GetFeed() calls");

            using (var feedStream = new MemoryStream())
            {
                _feed.Save(feedStream);
                feedStream.Position = 0;

                // Expect pass-through on adding
                _cacheMock.Expect("Add", "http://0install.de/feeds/test/test1.xml", feedStream);
                _cache.Add("http://0install.de/feeds/test/test1.xml", feedStream);
            }

            Assert.AreNotSame(firstAccess, _cache.GetFeed("http://0install.de/feeds/test/test1.xml"), "Adding again should overwrite cache entry");
        }

        [Test]
        public void TestRemove()
        {
            using (var feedStream = new MemoryStream())
            {
                _feed.Save(feedStream);
                feedStream.Position = 0;

                // Expect pass-through on adding
                _cacheMock.Expect("Add", "http://0install.de/feeds/test/test1.xml", feedStream);
                _cache.Add("http://0install.de/feeds/test/test1.xml", feedStream);
            }

            // Expect no pass-through due to caching on .Add()
            _feed.Simplify();
            Feed firstAccess = _cache.GetFeed("http://0install.de/feeds/test/test1.xml");
            Assert.AreEqual(_feed, firstAccess);
            Feed secondAccess = _cache.GetFeed("http://0install.de/feeds/test/test1.xml");
            Assert.AreSame(firstAccess, secondAccess, "Cache should return identical reference on multiple GetFeed() calls");

            // Expect pass-through on remove
            _cacheMock.Expect("Remove", "http://0install.de/feeds/test/test1.xml");
            _cache.Remove("http://0install.de/feeds/test/test1.xml");

            // Expect pass-through after remove
            _cacheMock.ExpectAndReturn("GetFeed", _feed, "http://0install.de/feeds/test/test1.xml");
            Assert.AreEqual(_feed, _cache.GetFeed("http://0install.de/feeds/test/test1.xml"));
        }
    }
}
