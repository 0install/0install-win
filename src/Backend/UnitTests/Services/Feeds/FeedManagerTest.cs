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
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using Xunit;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Services.Feeds
{
    /// <summary>
    /// Contains test methods for <see cref="FeedManager"/>.
    /// </summary>
    public class FeedManagerTest : TestWithContainer<FeedManager>
    {
        private Mock<IFeedCache> FeedCacheMock => GetMock<IFeedCache>();
        private Mock<ITrustManager> TrustManagerMock => GetMock<ITrustManager>();

        private readonly Feed _feedPreNormalize;
        private readonly Feed _feedPostNormalize;

        public FeedManagerTest()
        {
            _feedPreNormalize = FeedTest.CreateTestFeed();

            _feedPostNormalize = _feedPreNormalize.Clone();
            _feedPostNormalize.Normalize(_feedPreNormalize.Uri);
        }

        [Fact]
        public void Local()
        {
            using (var feedFile = new TemporaryFile("0install-unit-tests"))
            {
                _feedPreNormalize.SaveXml(feedFile);

                var result = Sut[new FeedUri(feedFile)];
                result.Should().Be(_feedPostNormalize);
                Sut.Stale.Should().BeFalse();
            }
        }

        [Fact]
        public void LocalMissing()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
                Assert.Throws<FileNotFoundException>(() => Sut[new FeedUri(Path.Combine(tempDir, "invalid"))]);
        }

        [Fact]
        public void Download()
        {
            var feed = FeedTest.CreateTestFeed();
            var feedStream = new MemoryStream();
            using (var server = new MicroServer("feed.xml", feedStream))
            {
                feed.Uri = new FeedUri(server.FileUri);
                feed.SaveXml(feedStream);
                var data = feedStream.ToArray();
                feedStream.Position = 0;

                Sut.IsStale(feed.Uri).Should().BeTrue(because: "Non-cached feeds should be reported as stale");

                // No previous feed
                FeedCacheMock.Setup(x => x.Contains(feed.Uri)).Returns(false);
                FeedCacheMock.Setup(x => x.GetSignatures(feed.Uri)).Throws<KeyNotFoundException>();

                FeedCacheMock.Setup(x => x.Add(feed.Uri, data));
                FeedCacheMock.Setup(x => x.GetFeed(feed.Uri)).Returns(feed);

                TrustManagerMock.Setup(x => x.CheckTrust(data, feed.Uri, It.IsAny<string>())).Returns(OpenPgpUtilsTest.TestSignature);

                Sut[feed.Uri].Should().Be(feed);
            }
        }

        [Fact]
        public void DownloadIncorrectUri()
        {
            var feed = FeedTest.CreateTestFeed();
            var feedStream = new MemoryStream();
            feed.SaveXml(feedStream);
            feedStream.Position = 0;

            using (var server = new MicroServer("feed.xml", feedStream))
            {
                var feedUri = new FeedUri(server.FileUri);

                // No previous feed
                FeedCacheMock.Setup(x => x.Contains(feedUri)).Returns(false);

                Assert.Throws<InvalidDataException>(() => Sut[feedUri]);
            }
        }

        [Fact]
        public void DownloadFromMirror()
        {
            var feed = FeedTest.CreateTestFeed();
            feed.Uri = new FeedUri("http://invalid/directory/feed.xml");
            var data = feed.ToXmlString().ToStream().ToArray();

            // No previous feed
            FeedCacheMock.Setup(x => x.Contains(feed.Uri)).Returns(false);
            FeedCacheMock.Setup(x => x.GetSignatures(feed.Uri)).Throws<KeyNotFoundException>();

            FeedCacheMock.Setup(x => x.Add(feed.Uri, data));
            FeedCacheMock.Setup(x => x.GetFeed(feed.Uri)).Returns(feed);
            using (var mirrorServer = new MicroServer("feeds/http/invalid/directory%23feed.xml/latest.xml", new MemoryStream(data)))
            {
                // ReSharper disable once AccessToDisposedClosure
                TrustManagerMock.Setup(x => x.CheckTrust(data, feed.Uri, It.IsAny<string>())).Returns(OpenPgpUtilsTest.TestSignature);

                Config.FeedMirror = mirrorServer.ServerUri;
                Sut[feed.Uri].Should().Be(feed);
            }
        }

        [Fact]
        public void DetectFreshCached()
        {
            FeedCacheMock.Setup(x => x.Contains(FeedTest.Test1Uri)).Returns(true);
            FeedCacheMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(_feedPreNormalize);
            new FeedPreferences {LastChecked = DateTime.UtcNow}.SaveFor(FeedTest.Test1Uri);

            Sut.IsStale(FeedTest.Test1Uri).Should().BeFalse();
            Sut[FeedTest.Test1Uri].Should().Be(_feedPostNormalize);
            Sut.Stale.Should().BeFalse();
        }

        [Fact]
        public void ServeFromInMemoryCache()
        {
            DetectFreshCached();

            // ReSharper disable once UnusedVariable
            var _ = Sut[FeedTest.Test1Uri];

            FeedCacheMock.Verify(x => x.GetFeed(FeedTest.Test1Uri), Times.Once(),
                failMessage: "Underlying cache was accessed more than once instead of being handled by the in-memory cache.");
        }

        [Fact]
        public void Refresh()
        {
            var feed = FeedTest.CreateTestFeed();
            var feedStream = new MemoryStream();
            using (var server = new MicroServer("feed.xml", feedStream))
            {
                feed.Uri = new FeedUri(server.FileUri);
                feed.SaveXml(feedStream);
                var feedData = feedStream.ToArray();
                feedStream.Position = 0;

                AssertRefreshData(feed, feedData);
            }
        }

        [Fact]
        public void RefreshClearsInMemoryCache()
        {
            var feed = FeedTest.CreateTestFeed();
            var feedStream = new MemoryStream();
            using (var server = new MicroServer("feed.xml", feedStream))
            {
                feed.Uri = new FeedUri(server.FileUri);
                feed.SaveXml(feedStream);
                var feedData = feedStream.ToArray();
                feedStream.Position = 0;

                // Cause feed to become in-memory cached
                FeedCacheMock.Setup(x => x.Contains(feed.Uri)).Returns(true);
                FeedCacheMock.Setup(x => x.GetFeed(feed.Uri)).Returns(feed);
                Sut[feed.Uri].Should().Be(feed);

                AssertRefreshData(feed, feedData);
            }
        }

        private void AssertRefreshData(Feed feed, byte[] feedData)
        {
            FeedCacheMock.Setup(x => x.Add(feed.Uri, feedData));
            FeedCacheMock.Setup(x => x.GetFeed(feed.Uri)).Returns(feed);
            FeedCacheMock.Setup(x => x.GetSignatures(feed.Uri)).Returns(new[] {OpenPgpUtilsTest.TestSignature});

            // ReSharper disable once AccessToDisposedClosure
            TrustManagerMock.Setup(x => x.CheckTrust(feedData, feed.Uri, It.IsAny<string>())).Returns(OpenPgpUtilsTest.TestSignature);

            Sut.Refresh = true;
            Sut[feed.Uri].Should().Be(feed);
        }

        [Fact]
        public void DetectStaleCached()
        {
            var feed = new Feed {Name = "Mock feed"};
            FeedCacheMock.Setup(x => x.Contains(FeedTest.Test1Uri)).Returns(true);
            FeedCacheMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(feed);
            new FeedPreferences {LastChecked = DateTime.UtcNow - Config.Freshness}.SaveFor(FeedTest.Test1Uri);

            Sut.IsStale(FeedTest.Test1Uri).Should().BeTrue();
            Sut[FeedTest.Test1Uri].Should().BeSameAs(feed);
            Sut.Stale.Should().BeTrue();
        }

        [Fact] // Ensures valid feeds are correctly imported.
        public void Import()
        {
            var feed = FeedTest.CreateTestFeed();
            var data = SignFeed(feed);

            // No previous feed
            FeedCacheMock.Setup(x => x.GetSignatures(feed.Uri)).Throws<KeyNotFoundException>();

            FeedCacheMock.Setup(x => x.Add(feed.Uri, data));
            using (var feedFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllBytes(feedFile, data);
                Sut.ImportFeed(feedFile);
            }
        }

        [Fact] // Ensures replay attacks are detected.
        public void ImportReplayAttack()
        {
            var feed = FeedTest.CreateTestFeed();
            var data = SignFeed(feed);

            // Newer signautre present => replay attack
            FeedCacheMock.Setup(x => x.GetSignatures(feed.Uri)).Returns(new[]
            {
                new ValidSignature(OpenPgpUtilsTest.TestKeyID, OpenPgpUtilsTest.TestFingerprint, new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            });

            using (var feedFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllBytes(feedFile, data);
                Assert.Throws<ReplayAttackException>(() => Sut.ImportFeed(feedFile));
            }
        }

        /// <summary>
        /// Generates a byte array containing a feed and a mock signature. Configures <see cref="IOpenPgp"/> to validate this signature.
        /// </summary>
        /// <param name="feed">The feed to "sign".</param>
        /// <returns>A byte array containing the serialized <paramref name="feed"/> and its mock signature.</returns>
        private byte[] SignFeed(Feed feed)
        {
            var data = feed.ToXmlString().ToStream().ToArray();
            TrustManagerMock.Setup(x => x.CheckTrust(data, feed.Uri, It.IsAny<string>())).Returns(OpenPgpUtilsTest.TestSignature);
            return data;
        }
    }
}
