/*
 * Copyright 2010-2016 Bastian Eicher
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
using FluentAssertions;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using Xunit;

namespace ZeroInstall.Store.Icons
{
    /// <summary>
    /// Contains test methods for <see cref="DiskIconCache"/>.
    /// </summary>
    public class DiskIconCacheTest : IDisposable
    {
        private readonly TemporaryDirectory _tempDir;
        private readonly DiskIconCache _cache;

        public DiskIconCacheTest()
        {
            // Create a temporary cache
            _tempDir = new TemporaryDirectory("0install-unit-tests");
            _cache = new DiskIconCache(_tempDir);

            // Add some dummy icons to the cache
            File.WriteAllText(Path.Combine(_tempDir, new FeedUri("http://0install.de/feeds/images/test1.png").Escape()), "");
            File.WriteAllText(Path.Combine(_tempDir, new FeedUri("http://0install.de/feeds/images/test2.png").Escape()), "");
            File.WriteAllText(Path.Combine(_tempDir, "http_invalid"), "");
        }

        public void Dispose() => _tempDir.Dispose();

        [Fact]
        public void TestContains()
        {
            _cache.Contains(new Uri("http://0install.de/feeds/images/test1.png")).Should().BeTrue();
            _cache.Contains(new Uri("http://0install.de/feeds/images/test2.png")).Should().BeTrue();
            _cache.Contains(new Uri("http://0install.de/feeds/test/test3.xml")).Should().BeFalse();
        }

        /// <summary>
        /// Ensures <see cref="DiskIconCache.GetIcon"/> works correctly for icons that are already in the cache.
        /// </summary>
        [Fact]
        public void TestGetIconCached()
        {
            const string icon1 = "http://0install.de/feeds/images/test1.png";
            _cache.GetIcon(new Uri(icon1), new SilentTaskHandler())
                .Should().Be(Path.Combine(_tempDir, new FeedUri(icon1).Escape()));
        }

        /// <summary>
        /// Ensures <see cref="DiskIconCache.GetIcon"/> correctly downloads icons that are not already in the cache.
        /// </summary>
        [Fact]
        public void TestGetIconDownload()
        {
            const string iconData = "test";
            using (var server = new MicroServer("icon.png", iconData.ToStream()))
            {
                string path = _cache.GetIcon(server.FileUri, new SilentTaskHandler());
                File.ReadAllText(path).Should().Be(iconData);
            }
        }

        /// <summary>
        /// Ensures <see cref="DiskIconCache.GetIcon"/> returns outdated files from the cache if downloads fail.
        /// </summary>
        [Fact]
        public void TestGetIconDownloadFail()
        {
            const string iconData = "test";
            using (var server = new MicroServer("empty", new MemoryStream()))
            {
                // Write a file to the cache directory, mark it as outdated, use an unreachable/invalid URI
                string prePath = Path.Combine(_tempDir, new FeedUri(server.FileUri + "-invalid").Escape());
                File.WriteAllText(prePath, iconData);
                File.SetLastWriteTimeUtc(prePath, new DateTime(1980, 1, 1));

                string path = _cache.GetIcon(new Uri(server.FileUri + "-invalid"), new SilentTaskHandler());
                File.ReadAllText(path).Should().Be(iconData);
            }
        }

        /// <summary>
        /// Ensures <see cref="DiskIconCache.Remove"/> correctly removes an icon from the cache.
        /// </summary>
        [Fact]
        public void TestRemove()
        {
            _cache.Remove(new Uri("http://0install.de/feeds/images/test1.png"));
            _cache.Contains(new Uri("http://0install.de/feeds/images/test1.png")).Should().BeFalse();
            _cache.Contains(new Uri("http://0install.de/feeds/images/test2.png")).Should().BeTrue();
        }
    }
}
