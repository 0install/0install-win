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
using System.IO;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using NUnit.Framework;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.Icons
{
    /// <summary>
    /// Contains test methods for <see cref="DiskIconCache"/>.
    /// </summary>
    [TestFixture]
    public class DiskIconCacheTest
    {
        private TemporaryDirectory _tempDir;
        private DiskIconCache _cache;

        [SetUp]
        public void SetUp()
        {
            // Create a temporary cache
            _tempDir = new TemporaryDirectory("0install-unit-tests");
            _cache = new DiskIconCache(_tempDir);

            // Add some dummy icons to the cache
            File.WriteAllText(Path.Combine(_tempDir, ModelUtils.Escape("http://0install.de/feeds/images/test1.png")), "");
            File.WriteAllText(Path.Combine(_tempDir, ModelUtils.Escape("http://0install.de/feeds/images/test2.png")), "");
            File.WriteAllText(Path.Combine(_tempDir, "http_invalid"), "");
        }

        [TearDown]
        public void TearDown()
        {
            _tempDir.Dispose();
        }

        [Test]
        public void TestContains()
        {
            Assert.IsTrue(_cache.Contains(new Uri("http://0install.de/feeds/images/test1.png")));
            Assert.IsTrue(_cache.Contains(new Uri("http://0install.de/feeds/images/test2.png")));
            Assert.IsFalse(_cache.Contains(new Uri("http://0install.de/feeds/test/test3.xml")));
        }

        [Test]
        public void TestListAll()
        {
            var icons = _cache.ListAll();
            CollectionAssert.AreEqual(
                new[] {"http://0install.de/feeds/images/test1.png", "http://0install.de/feeds/images/test2.png"},
                icons);
        }

        /// <summary>
        /// Ensures <see cref="DiskIconCache.GetIcon"/> works correctly for icons that are already in the cache.
        /// </summary>
        [Test]
        public void TestGetIconCached()
        {
            const string icon1 = "http://0install.de/feeds/images/test1.png";
            Assert.AreEqual(Path.Combine(_tempDir, ModelUtils.Escape(icon1)), _cache.GetIcon(new Uri(icon1), new SilentTaskHandler()));
        }

        /// <summary>
        /// Ensures <see cref="DiskIconCache.GetIcon"/> correctly downloads icons that are not already in the cache.
        /// </summary>
        [Test]
        public void TestGetIconDownload()
        {
            const string iconData = "test";
            using (var server = new MicroServer("icon.png", iconData.ToStream()))
            {
                string path = _cache.GetIcon(server.FileUri, new SilentTaskHandler());
                Assert.AreEqual(iconData, File.ReadAllText(path));
            }
        }

        /// <summary>
        /// Ensures <see cref="DiskIconCache.GetIcon"/> returns outdated files from the cache if downloads fail.
        /// </summary>
        [Test]
        public void TestGetIconDownloadFail()
        {
            const string iconData = "test";
            using (var server = new MicroServer("empty", new MemoryStream()))
            {
                // Write a file to the cache directory, mark it as outdated, use an unreachable/invalid URI
                string prePath = Path.Combine(_tempDir, ModelUtils.Escape(server.FileUri + "-invalid"));
                File.WriteAllText(prePath, iconData);
                File.SetLastWriteTimeUtc(prePath, new DateTime(1980, 1, 1));

                string path = _cache.GetIcon(new Uri(server.FileUri + "-invalid"), new SilentTaskHandler());
                Assert.AreEqual(iconData, File.ReadAllText(path));
            }
        }

        /// <summary>
        /// Ensures <see cref="DiskIconCache.Remove"/> correctly removes an icon from the cache.
        /// </summary>
        [Test]
        public void TestRemove()
        {
            _cache.Remove(new Uri("http://0install.de/feeds/images/test1.png"));
            Assert.IsFalse(_cache.Contains(new Uri("http://0install.de/feeds/images/test1.png")));
            Assert.IsTrue(_cache.Contains(new Uri("http://0install.de/feeds/images/test2.png")));
        }
    }
}
