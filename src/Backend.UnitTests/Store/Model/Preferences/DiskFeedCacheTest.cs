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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Utils;
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
            _feed1.Uri = new Uri("http://0install.de/feeds/test/test1.xml");
            _feed1.SaveXml(Path.Combine(_tempDir, ModelUtils.Escape(_feed1.UriString)));
            _feed1.Normalize(Path.Combine(_tempDir, ModelUtils.Escape(_feed1.UriString)));
            _feed2 = FeedTest.CreateTestFeed();
            _feed2.Uri = new Uri("http://0install.de/feeds/test/test2.xml");
            _feed2.SaveXml(Path.Combine(_tempDir, ModelUtils.Escape(_feed2.UriString)));
            _feed2.Normalize(Path.Combine(_tempDir, ModelUtils.Escape(_feed2.UriString)));
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
            Assert.IsTrue(_cache.Contains("http://0install.de/feeds/test/test1.xml"));
            Assert.IsTrue(_cache.Contains("http://0install.de/feeds/test/test2.xml"));
            Assert.IsFalse(_cache.Contains("http://0install.de/feeds/test/test3.xml"));

            using (var localFeed = new TemporaryFile("0install-unit-tests"))
            {
                _feed1.SaveXml(localFeed);
                Assert.IsTrue(_cache.Contains(localFeed), "Should detect local feed files without them actually being in the cache");
            }

            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
                Assert.IsFalse(_cache.Contains(Path.Combine(tempDir, "feed.xml")), "Should not detect phantom local feed files");
        }

        [Test]
        public void TestContainsCaseSensitive()
        {
            Assert.IsTrue(_cache.Contains("http://0install.de/feeds/test/test1.xml"));
            Assert.IsFalse(_cache.Contains("http://0install.de/feeds/test/Test1.xml"), "Should not be case-sensitive");
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
                _feed1.SaveXml(localFeed);
                Assert.AreEqual(_feed1, _cache.GetFeed(localFeed), "Should provide local feed files without them actually being in the cache");
            }
        }

        [Test]
        public void TestGetFeedCaseSensitive()
        {
            Assert.DoesNotThrow(() => _cache.GetFeed("http://0install.de/feeds/test/test1.xml"));
            Assert.Throws<KeyNotFoundException>(() => _cache.GetFeed("http://0install.de/feeds/test/Test1.xml"), "Should not be case-sensitive");
        }

        [Test]
        public void TestGetSignatures()
        {
            var result = new OpenPgpSignature[0];

            var signatures = _cache.GetSignatures("http://0install.de/feeds/test/test1.xml");

            CollectionAssert.AreEqual(signatures, result);
        }

        [Test]
        public void TestAdd()
        {
            var feed = FeedTest.CreateTestFeed();
            feed.Uri = new Uri("http://0install.de/feeds/test/test3.xml");

            _cache.Add(feed.Uri.ToString(), feed.ToArray());

            feed.Normalize(feed.Uri.ToString());
            Assert.AreEqual(feed, _cache.GetFeed(feed.Uri.ToString()));
        }

        [Test]
        public void TestRemove()
        {
            Assert.IsTrue(_cache.Contains("http://0install.de/feeds/test/test1.xml"));
            _cache.Remove("http://0install.de/feeds/test/test1.xml");
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
            if (!WindowsUtils.IsWindows) Assert.Ignore("Windows systems have a specific upper limit to file path lengths");

            var longHttpUrlBuilder = new StringBuilder(255);
            for (int i = 0; i < 255; i++)
                longHttpUrlBuilder.Append("x");

            var feed = FeedTest.CreateTestFeed();
            feed.Uri = new Uri("http://0install.de/feeds/test-" + longHttpUrlBuilder);

            _cache.Add(feed.Uri.ToString(), feed.ToArray());

            feed.Normalize(feed.Uri.ToString());
            Assert.AreEqual(feed, _cache.GetFeed(feed.Uri.ToString()));

            Assert.IsTrue(_cache.Contains(feed.Uri.ToString()));
            _cache.Remove(feed.Uri.ToString());
            Assert.IsFalse(_cache.Contains(feed.Uri.ToString()));
        }
    }
}
