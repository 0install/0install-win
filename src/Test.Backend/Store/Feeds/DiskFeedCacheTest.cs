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
using System.Web;
using Common.Storage;
using NUnit.Framework;
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
            _feed1.Save(Path.Combine(_tempDir.Path, HttpUtility.UrlEncode(_feed1.UriString)));
            _feed2 = FeedTest.CreateTestFeed();
            _feed2.Uri = new Uri("http://0install.de/feeds/test/test2.xml");
            _feed2.Save(Path.Combine(_tempDir.Path, HttpUtility.UrlEncode(_feed2.UriString)));
            File.WriteAllText(Path.Combine(_tempDir.Path, "http_invalid"), "");
        }

        [TearDown]
        public void TearDown()
        {
            _tempDir.Dispose();
        }

        /// <summary>
        /// Ensures <see cref="DiskFeedCache.Contains"/> correctly determines whether a feed is in the cache.
        /// </summary>
        [Test]
        public void TestContains()
        {
            Assert.IsTrue(_cache.Contains(new Uri("http://0install.de/feeds/test/test1.xml")));
            Assert.IsTrue(_cache.Contains(new Uri("http://0install.de/feeds/test/test2.xml")));
            Assert.IsFalse(_cache.Contains(new Uri("http://0install.de/feeds/test/test3.xml")));
        }

        /// <summary>
        /// Ensures that <see cref="DiskFeedCache.ListAll"/> correctly distiguishes invalid entries in the cache.
        /// </summary>
        [Test]
        public void TestListAll()
        {
            var feeds = _cache.ListAll();
            CollectionAssert.AreEqual(
                new[] {new Uri("http://0install.de/feeds/test/test1.xml"), new Uri("http://0install.de/feeds/test/test2.xml")},
                feeds);
        }

        /// <summary>
        /// Ensures <see cref="DiskFeedCache.GetFeed"/> correctly retreives <see cref="Feed"/>s from the cache.
        /// </summary>
        [Test]
        public void TestGet()
        {
            var feed = _cache.GetFeed(_feed1.Uri);
            Assert.AreEqual(_feed1, feed);
        }

        /// <summary>
        /// Ensures that <see cref="DiskFeedCache.GetAll"/> correctly loads all cached feeds.
        /// </summary>
        [Test]
        public void TestGetAll()
        {
            var feeds = _cache.GetAll();
            CollectionAssert.AreEqual(new[] { _feed1, _feed2 }, feeds);
        }

        /// <summary>
        /// Ensures that <see cref="DiskFeedCache.Add"/> correctly adds new feeds and detects replay attacks.
        /// </summary>
        // Test deactivated because feature isn't implemented yet
        //[Test]
        public void TestAdd()
        {
            // ToDo: Implement
        }
    }
}
