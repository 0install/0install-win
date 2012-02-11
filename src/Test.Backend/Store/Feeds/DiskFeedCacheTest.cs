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
using System.IO;
using System.Text;
using Common.Storage;
using Common.Utils;
using NUnit.Framework;
using Moq;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Contains test methods for <see cref="DiskFeedCache"/>.
    /// </summary>
    [TestFixture]
    public class DiskFeedCacheTest
    {
        private TemporaryDirectory _tempDir;
        private DiskFeedCache _cache;
        private Feed _feed1, _feed2;

        [SetUp]
        public void SetUp()
        {
            // Create a temporary cache
            _tempDir = new TemporaryDirectory("0install-unit-tests");
            _cache = new DiskFeedCache(_tempDir.Path);

            // Add some dummy feeds to the cache
            _feed1 = FeedTest.CreateTestFeed();
            _feed1.Uri = new Uri("http://0install.de/feeds/test/test1.xml");
            _feed1.Save(Path.Combine(_tempDir.Path, ModelUtils.Escape(_feed1.UriString)));
            _feed1.Simplify();
            _feed2 = FeedTest.CreateTestFeed();
            _feed2.Uri = new Uri("http://0install.de/feeds/test/test2.xml");
            _feed2.Save(Path.Combine(_tempDir.Path, ModelUtils.Escape(_feed2.UriString)));
            _feed2.Simplify();
            File.WriteAllText(Path.Combine(_tempDir.Path, "http_invalid"), "");
        }

        [TearDown]
        public void TearDown()
        {
            _tempDir.Dispose();
        }

        [Test]
        public void TestContains()
        {
            Assert.IsTrue(_cache.Contains("http://0install.de/feeds/test/test1.xml"));
            Assert.IsTrue(_cache.Contains("http://0install.de/feeds/test/test2.xml"));
            Assert.IsFalse(_cache.Contains("http://0install.de/feeds/test/test3.xml"));

            using (var localFeed = new TemporaryFile("0install-unit-tests"))
            {
                _feed1.Save(localFeed.Path);
                Assert.IsTrue(_cache.Contains(localFeed.Path), "Should detect local feed files without them actually being in the cache");
            }

            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
                Assert.IsFalse(_cache.Contains(Path.Combine(tempDir.Path, "feed.xml")), "Should not detect phantom local feed files");
        }

        [Test]
        public void TestListAll()
        {
            var feeds = _cache.ListAll();
            CollectionAssert.AreEqual(
                new[] {new Uri("http://0install.de/feeds/test/test1.xml"), new Uri("http://0install.de/feeds/test/test2.xml")},
                feeds);
        }

        [Test]
        public void TestGetFeed()
        {
            var feed = _cache.GetFeed(_feed1.Uri.ToString());
            Assert.AreEqual(_feed1, feed);

            using (var localFeed = new TemporaryFile("0install-unit-tests"))
            {
                _feed1.Save(localFeed.Path);
                Assert.AreEqual(_feed1, _cache.GetFeed(localFeed.Path), "Should provide local feed files without them actually being in the cache");
            }
        }

        [Test]
        public void TestGetSignatures()
        {
            var result = new OpenPgpSignature[0];

            var signatures = _cache.GetSignatures("http://0install.de/feeds/test/test1.xml", new Mock<IOpenPgp>().Object);

            CollectionAssert.AreEqual(signatures, result);
        }

        [Test]
        public void TestAdd()
        {
            var feed = FeedTest.CreateTestFeed();
            feed.Uri = new Uri("http://0install.de/feeds/test/test3.xml");

            _cache.Add(feed.Uri.ToString(), feed.ToArray());

            feed.Simplify();
            Assert.AreEqual(feed, _cache.GetFeed(feed.Uri.ToString()));
        }

        [Test]
        public void TestRemove()
        {
            _cache.Remove("http://0install.de/feeds/test/test1.xml");
            Assert.DoesNotThrow(() => _cache.Remove("http://0install.de/feeds/test/test1.xml"));
            Assert.IsFalse(_cache.Contains("http://0install.de/feeds/test/test1.xml"));
            Assert.IsTrue(_cache.Contains("http://0install.de/feeds/test/test2.xml"));
        }

        /// <summary>
        /// Ensures <see cref="DiskFeedCache"/> can handle feed URIs longer than the OSes maximum supported file path length.
        /// </summary>
        // This feature has been removed to improve performance
        //[Test]
        public void TestTooLongFilename()
        {
            if (!WindowsUtils.IsWindows) throw new InconclusiveException("Windows systems have a specific upper limit to file path lengths");

            var longHttpUrlBuilder = new StringBuilder(255);
            for (int i = 0; i < 255; i++)
                longHttpUrlBuilder.Append("x");

            var feed = FeedTest.CreateTestFeed();
            feed.Uri = new Uri("http://0install.de/feeds/test-" + longHttpUrlBuilder);

            _cache.Add(feed.Uri.ToString(), feed.ToArray());

            feed.Simplify();
            Assert.AreEqual(feed, _cache.GetFeed(feed.Uri.ToString()));

            Assert.IsTrue(_cache.Contains(feed.Uri.ToString()));
            _cache.Remove(feed.Uri.ToString());
            Assert.IsFalse(_cache.Contains(feed.Uri.ToString()));
        }
    }
}
