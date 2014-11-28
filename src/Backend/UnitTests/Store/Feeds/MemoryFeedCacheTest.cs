/*
 * Copyright 2010-2014 Bastian Eicher
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
using NUnit.Framework;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Contains test methods for <see cref="MemoryFeedCache"/>.
    /// </summary>
    [TestFixture]
    public class MemoryFeedCacheTest : TestWithMocks
    {
        private Mock<IFeedCache> _backingCacheMock;
        private MemoryFeedCache _cache;
        private Feed _feed;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _backingCacheMock = MockRepository.Create<IFeedCache>();
            _cache = new MemoryFeedCache(_backingCacheMock.Object);

            // Create a dummy feed
            _feed = FeedTest.CreateTestFeed();
            _feed.Uri = FeedTest.Test1Uri;
        }

        [Test]
        public void TestContains()
        {
            // Expect simple pass-through
            _backingCacheMock.Setup(x => x.Contains(FeedTest.Test1Uri)).Returns(true);
            _backingCacheMock.Setup(x => x.Contains(FeedTest.Test2Uri)).Returns(true);
            _backingCacheMock.Setup(x => x.Contains(FeedTest.Test3Uri)).Returns(false);
            Assert.IsTrue(_cache.Contains(FeedTest.Test1Uri));
            Assert.IsTrue(_cache.Contains(FeedTest.Test2Uri));
            Assert.IsFalse(_cache.Contains(FeedTest.Test3Uri));
        }

        [Test]
        public void TestListAll()
        {
            // Expect simple pass-through
            var feeds = new[] {FeedTest.Test1Uri, FeedTest.Test2Uri};
            _backingCacheMock.Setup(x => x.ListAll()).Returns(feeds);
            CollectionAssert.AreEqual(feeds, _cache.ListAll());
        }

        [Test]
        public void TestGetFeed()
        {
            // Expect pass-through on first access
            _backingCacheMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(_feed);
            Feed firstAccess = _cache.GetFeed(FeedTest.Test1Uri);
            Assert.AreEqual(_feed, firstAccess);

            // Expect  in-memory cache on second access
            Feed secondAccess = _cache.GetFeed(FeedTest.Test1Uri);
            Assert.AreSame(firstAccess, secondAccess);
        }

        [Test]
        public void TestGetSignautes()
        {
            var result = new OpenPgpSignature[0];

            // Expect pass-through
            _backingCacheMock.Setup(x => x.GetSignatures(FeedTest.Test1Uri)).Returns(result);
            var signatures = _cache.GetSignatures(FeedTest.Test1Uri);

            CollectionAssert.AreEqual(signatures, result);
        }

        [Test]
        public void TestAdd()
        {
            var feedData1 = _feed.ToArray();

            // Expect pass-through on adding
            _backingCacheMock.Setup(x => x.Add(FeedTest.Test1Uri, feedData1));
            _cache.Add(FeedTest.Test1Uri, feedData1);

            // Expect no pass-through due to caching on .Add()
            _feed.Normalize(_feed.Uri);
            Feed firstAccess = _cache.GetFeed(FeedTest.Test1Uri);
            Assert.AreEqual(_feed, firstAccess);
            Feed secondAccess = _cache.GetFeed(FeedTest.Test1Uri);
            Assert.AreSame(firstAccess, secondAccess, "Cache should return identical reference on multiple GetFeed() calls");

            var feedData2 = _feed.ToArray();

            // Expect pass-through on adding
            _backingCacheMock.Setup(x => x.Add(FeedTest.Test1Uri, feedData2));
            _cache.Add(FeedTest.Test1Uri, feedData2);

            Assert.AreNotSame(firstAccess, _cache.GetFeed(FeedTest.Test1Uri), "Adding again should overwrite cache entry");
        }

        [Test]
        public void TestRemove()
        {
            var feedData = _feed.ToArray();

            // Expect pass-through on adding
            _backingCacheMock.Setup(x => x.Add(FeedTest.Test1Uri, feedData));
            _cache.Add(FeedTest.Test1Uri, feedData);

            // Expect no pass-through due to caching on .Add()
            _feed.Normalize(_feed.Uri);
            Feed firstAccess = _cache.GetFeed(FeedTest.Test1Uri);
            Assert.AreEqual(_feed, firstAccess);
            Feed secondAccess = _cache.GetFeed(FeedTest.Test1Uri);
            Assert.AreSame(firstAccess, secondAccess, "Cache should return identical reference on multiple GetFeed() calls");

            // Expect pass-through on remove
            _backingCacheMock.Setup(x => x.Remove(FeedTest.Test1Uri));
            _cache.Remove(FeedTest.Test1Uri);

            // Expect pass-through after remove
            _backingCacheMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(_feed);
            Assert.AreEqual(_feed, _cache.GetFeed(FeedTest.Test1Uri));
        }
    }
}
