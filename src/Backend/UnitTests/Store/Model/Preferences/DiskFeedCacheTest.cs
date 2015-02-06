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

using System.Collections.Generic;
using System.IO;
using System.Text;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NUnit.Framework;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Store.Model.Preferences
{
    /// <summary>
    /// Contains test methods for <see cref="DiskFeedCache"/>.
    /// </summary>
    [TestFixture]
    public class DiskFeedCacheTest : TestWithMocks
    {
        private TemporaryDirectory _tempDir;
        private DiskFeedCache _cache;
        private Feed _feed1, _feed2;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            // Create a temporary cache
            _tempDir = new TemporaryDirectory("0install-unit-tests");
            _cache = new DiskFeedCache(_tempDir, MockRepository.Create<IOpenPgp>().Object);

            // Add some dummy feeds to the cache
            _feed1 = FeedTest.CreateTestFeed();
            _feed1.Uri = FeedTest.Test1Uri;
            _feed1.SaveXml(Path.Combine(_tempDir, _feed1.Uri.Escape()));
            _feed1.Normalize(_feed1.Uri);
            _feed2 = FeedTest.CreateTestFeed();
            _feed2.Uri = FeedTest.Test2Uri;
            _feed2.SaveXml(Path.Combine(_tempDir, _feed2.Uri.Escape()));
            _feed2.Normalize(_feed2.Uri);
            File.WriteAllText(Path.Combine(_tempDir, "http_invalid"), "");
        }

        [TearDown]
        public override void TearDown()
        {
            _tempDir.Dispose();

            base.TearDown();
        }

        [Test]
        public void TestContains()
        {
            Assert.IsTrue(_cache.Contains(FeedTest.Test1Uri));
            Assert.IsTrue(_cache.Contains(FeedTest.Test2Uri));
            Assert.IsFalse(_cache.Contains(FeedTest.Test3Uri));

            using (var localFeed = new TemporaryFile("0install-unit-tests"))
            {
                _feed1.SaveXml(localFeed);
                Assert.IsTrue(_cache.Contains(new FeedUri(localFeed)), "Should detect local feed files without them actually being in the cache");
            }

            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
                Assert.IsFalse(_cache.Contains(new FeedUri(Path.Combine(tempDir, "feed.xml"))), "Should not detect phantom local feed files");
        }

        [Test]
        public void TestContainsCaseSensitive()
        {
            Assert.IsTrue(_cache.Contains(new FeedUri("http://0install.de/feeds/test/test1.xml")));
            Assert.IsFalse(_cache.Contains(new FeedUri("http://0install.de/feeds/test/Test1.xml")), "Should not be case-sensitive");
        }

        [Test]
        public void TestListAll()
        {
            var feeds = _cache.ListAll();
            CollectionAssert.AreEqual(
                new[] {FeedTest.Test1Uri, FeedTest.Test2Uri},
                feeds);
        }

        [Test]
        public void TestGetFeed()
        {
            var feed = _cache.GetFeed(_feed1.Uri);
            Assert.AreEqual(_feed1, feed);

            using (var localFeed = new TemporaryFile("0install-unit-tests"))
            {
                _feed1.SaveXml(localFeed);
                Assert.AreEqual(_feed1, _cache.GetFeed(new FeedUri(localFeed)), "Should provide local feed files without them actually being in the cache");
            }
        }

        [Test]
        public void TestGetFeedCaseSensitive()
        {
            Assert.DoesNotThrow(() => _cache.GetFeed(new FeedUri("http://0install.de/feeds/test/test1.xml")));
            Assert.Throws<KeyNotFoundException>(() => _cache.GetFeed(new FeedUri("http://0install.de/feeds/test/Test1.xml")), "Should not be case-sensitive");
        }

        [Test]
        public void TestGetSignatures()
        {
            var result = new OpenPgpSignature[0];

            var signatures = _cache.GetSignatures(FeedTest.Test1Uri);

            CollectionAssert.AreEqual(signatures, result);
        }

        [Test]
        public void TestAdd()
        {
            var feed = FeedTest.CreateTestFeed();
            feed.Uri = FeedTest.Test3Uri;

            _cache.Add(feed.Uri, feed.ToArray());

            feed.Normalize(feed.Uri);
            Assert.AreEqual(feed, _cache.GetFeed(feed.Uri));
        }

        [Test]
        public void TestRemove()
        {
            Assert.IsTrue(_cache.Contains(FeedTest.Test1Uri));
            _cache.Remove(FeedTest.Test1Uri);
            Assert.IsFalse(_cache.Contains(FeedTest.Test1Uri));
            Assert.IsTrue(_cache.Contains(FeedTest.Test2Uri));
        }

        /// <summary>
        /// Ensures <see cref="DiskFeedCache"/> can handle feed URIs longer than the OSes maximum supported file path length.
        /// </summary>
        // This feature has been removed to improve performance
        //[Test]
        public void TestTooLongFilename()
        {
            if (!WindowsUtils.IsWindows) Assert.Ignore("Windows systems have a specific upper limit to file path lengths");

            var longHttpUrlBuilder = new StringBuilder(255);
            for (int i = 0; i < 255; i++)
                longHttpUrlBuilder.Append("x");

            var feed = FeedTest.CreateTestFeed();
            feed.Uri = new FeedUri("http://0install.de/feeds/test-" + longHttpUrlBuilder);

            _cache.Add(feed.Uri, feed.ToArray());

            feed.Normalize(feed.Uri);
            Assert.AreEqual(feed, _cache.GetFeed(feed.Uri));

            Assert.IsTrue(_cache.Contains(feed.Uri));
            _cache.Remove(feed.Uri);
            Assert.IsFalse(_cache.Contains(feed.Uri));
        }
    }
}
