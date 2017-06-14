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

using FluentAssertions;
using Xunit;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Contains test methods for <see cref="FeedExtensions"/>.
    /// </summary>
    public class FeedExtensionsTest
    {
        /// <summary>
        /// Ensures <see cref="Feeds.FeedExtensions.GetImplementation"/> correctly locates <see cref="Implementation"/> in a list of <see cref="Feed"/>s.
        /// </summary>
        [Fact]
        public void TestGetImplementation()
        {
            var digest1 = new ManifestDigest(sha256: "123");
            var implementation1 = new Implementation {ManifestDigest = digest1};
            var feed1 = new Feed {Elements = {implementation1}};
            var digest2 = new ManifestDigest(sha256: "abc");
            var implementation2 = new Implementation {ManifestDigest = digest2};
            var feed2 = new Feed {Elements = {implementation2}};
            var feeds = new[] {feed1, feed2};

            feeds.GetImplementation(digest1, out var feed).Should().Be(implementation1);
            feed.Should().Be(feed1);

            feeds.GetImplementation(digest2, out feed).Should().Be(implementation2);
            feed.Should().Be(feed2);

            feeds.GetImplementation(new ManifestDigest(sha256: "invalid"), out feed).Should().BeNull(because: "No implementation should have been found");
            feed.Should().BeNull(because: "No feed should have been found");
        }
    }
}
