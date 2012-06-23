/*
 * Copyright 2010-2012 Bastian Eicher
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
using Moq;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Contains test methods for <see cref="MemoryFeedCache"/>.
    /// </summary>
    [TestFixture]
    public class MemoryFeedCacheTest
    {
        private Mock<IFeedCache> _backingCacheMock;
        private MemoryFeedCache _cache;
        private Feed _feed;

        [SetUp]
        public void SetUp()
        {
            _backingCacheMock = new Mock<IFeedCache>(MockBehavior.Strict);
            _cache = new MemoryFeedCache(_backingCacheMock.Object);

            // Create a dummy feed
            _feed = FeedTest.CreateTestFeed();
            _feed.Uri = new Uri("http://0install.de/feeds/test/test1.xml");
        }

        [TearDown]
        public void TearDown()
        {
            _backingCacheMock.Verify();
        }

        [Test]
        public void TestContains()
        {
            // Expect simple pass-through
            _backingCacheMock.Setup(x => x.Contains("http://0install.de/feeds/test/test1.xml")).Returns(true).Verifiable();
            _backingCacheMock.Setup(x => x.Contains("http://0install.de/feeds/test/test2.xml")).Returns(true).Verifiable();
            _backingCacheMock.Setup(x => x.Contains("http://0install.de/feeds/test/test3.xml")).Returns(false).Verifiable();
            Assert.IsTrue(_cache.Contains("http://0install.de/feeds/test/test1.xml"));
            Assert.IsTrue(_cache.Contains("http://0install.de/feeds/test/test2.xml"));
            Assert.IsFalse(_cache.Contains("http://0install.de/feeds/test/test3.xml"));
        }

        [Test]
        public void TestListAll()
        {
            // Expect simple pass-through
            var feeds = new[] {"http://0install.de/feeds/test/test1.xml", "http://0install.de/feeds/test/test2.xml"};
            _backingCacheMock.Setup(x => x.ListAll()).Returns(feeds).Verifiable();
            CollectionAssert.AreEqual(feeds, _cache.ListAll());
        }

        [Test]
        public void TestGetFeed()
        {
            // Expect pass-through on first access
            _backingCacheMock.Setup(x => x.GetFeed("http://0install.de/feeds/test/test1.xml")).Returns(_feed).Verifiable();
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
            var openPgp = new Mock<IOpenPgp>().Object;

            // Expect pass-through
            _backingCacheMock.Setup(x => x.GetSignatures("http://0install.de/feeds/test/test1.xml", openPgp)).Returns(result).Verifiable();
            var signatures = _cache.GetSignatures("http://0install.de/feeds/test/test1.xml", openPgp);

            CollectionAssert.AreEqual(signatures, result);
        }

        [Test]
        public void TestAdd()
        {
            var feedData1 = _feed.ToArray();

            // Expect pass-through on adding
            _backingCacheMock.Setup(x => x.Add("http://0install.de/feeds/test/test1.xml", feedData1)).Verifiable();
            _cache.Add("http://0install.de/feeds/test/test1.xml", feedData1);

            // Expect no pass-through due to caching on .Add()
            _feed.Normalize(_feed.Uri.ToString());
            Feed firstAccess = _cache.GetFeed("http://0install.de/feeds/test/test1.xml");
            Assert.AreEqual(_feed, firstAccess);
            Feed secondAccess = _cache.GetFeed("http://0install.de/feeds/test/test1.xml");
            Assert.AreSame(firstAccess, secondAccess, "Cache should return identical reference on multiple GetFeed() calls");

            var feedData2 = _feed.ToArray();

            // Expect pass-through on adding
            _backingCacheMock.Setup(x => x.Add("http://0install.de/feeds/test/test1.xml", feedData2)).Verifiable();
            _cache.Add("http://0install.de/feeds/test/test1.xml", feedData2);

            Assert.AreNotSame(firstAccess, _cache.GetFeed("http://0install.de/feeds/test/test1.xml"), "Adding again should overwrite cache entry");
        }

        [Test]
        public void TestRemove()
        {
            var feedData = _feed.ToArray();

            // Expect pass-through on adding
            _backingCacheMock.Setup(x => x.Add("http://0install.de/feeds/test/test1.xml", feedData)).Verifiable();
            _cache.Add("http://0install.de/feeds/test/test1.xml", feedData);

            // Expect no pass-through due to caching on .Add()
            _feed.Normalize(_feed.Uri.ToString());
            Feed firstAccess = _cache.GetFeed("http://0install.de/feeds/test/test1.xml");
            Assert.AreEqual(_feed, firstAccess);
            Feed secondAccess = _cache.GetFeed("http://0install.de/feeds/test/test1.xml");
            Assert.AreSame(firstAccess, secondAccess, "Cache should return identical reference on multiple GetFeed() calls");

            // Expect pass-through on remove
            _backingCacheMock.Setup(x => x.Remove("http://0install.de/feeds/test/test1.xml")).Verifiable();
            _cache.Remove("http://0install.de/feeds/test/test1.xml");

            // Expect pass-through after remove
            _backingCacheMock.Setup(x => x.GetFeed("http://0install.de/feeds/test/test1.xml")).Returns(_feed).Verifiable();
            Assert.AreEqual(_feed, _cache.GetFeed("http://0install.de/feeds/test/test1.xml"));
        }
    }
}
