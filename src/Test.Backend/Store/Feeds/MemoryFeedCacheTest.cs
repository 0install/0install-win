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
using System.Collections.Generic;
using System.IO;
using System.Web;
using Common.Storage;
using NUnit.Framework;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Contains test methods for <see cref="MemoryFeedCache"/>.
    /// </summary>
    [TestFixture]
    public class MemoryFeedCacheTest
    {
        private TemporaryDirectory _tempDir;
        private MemoryFeedCache _cache;
        private Feed _feed1, _feed2;

        [SetUp]
        public void SetUp()
        {
            // ToDo: Replace underlying cache with a mock

            // Create a temporary cache
            _tempDir = new TemporaryDirectory("0install-unit-tests");
            _cache = new MemoryFeedCache(new DiskFeedCache(_tempDir.Path));

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

        [Test]
        public void TestContains()
        {
            Assert.IsTrue(_cache.Contains("http://0install.de/feeds/test/test1.xml"));
            Assert.IsTrue(_cache.Contains("http://0install.de/feeds/test/test2.xml"));
            Assert.IsFalse(_cache.Contains("http://0install.de/feeds/test/test3.xml"));
        }

        [Test]
        public void TestListAll()
        {
            var feeds = _cache.ListAll();
            CollectionAssert.AreEqual(
                new[] { new Uri("http://0install.de/feeds/test/test1.xml"), new Uri("http://0install.de/feeds/test/test2.xml") },
                feeds);
        }

        [Test]
        public void TestGet()
        {
            var feed = _cache.GetFeed(_feed1.Uri.ToString());
            Assert.AreEqual(_feed1, feed);
        }

        [Test]
        public void TestAdd()
        {
            // ToDo: Implement
        }

        [Test]
        public void TestRemove()
        {
            _cache.Remove("http://0install.de/feeds/test/test1.xml");
            Assert.Throws<KeyNotFoundException>(() => _cache.Remove("http://0install.de/feeds/test/test1.xml"));
            Assert.IsFalse(_cache.Contains("http://0install.de/feeds/test/test1.xml"));
            Assert.IsTrue(_cache.Contains("http://0install.de/feeds/test/test2.xml"));
        }
    }
}
