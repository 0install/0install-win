/*
 * Copyright 2010-2015 Bastian Eicher
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
using Moq;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NUnit.Framework;
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
    [TestFixture]
    public class FeedManagerTest : TestWithContainer<FeedManager>
    {
        private Mock<IFeedCache> _feedCacheMock;
        private Mock<ITrustManager> _trustManagerMock;

        protected override void Register(AutoMockContainer container)
        {
            base.Register(container);

            _feedCacheMock = container.GetMock<IFeedCache>();
            _trustManagerMock = container.GetMock<ITrustManager>();
        }

        [Test]
        public void Local()
        {
            var feed = new Feed();

            using (var feedFile = new TemporaryFile("0install-unit-tests"))
            {
                // ReSharper disable once AccessToDisposedClosure
                _feedCacheMock.Setup(x => x.GetFeed(new FeedUri(feedFile))).Returns(feed);

                Assert.AreSame(feed, Target[new FeedUri(feedFile)]);
                Assert.IsFalse(Target.Stale);
            }
        }

        [Test]
        public void LocalMissing()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
                // ReSharper disable once UnusedVariable
                Assert.Throws<FileNotFoundException>(() => { var _ = Target[new FeedUri(Path.Combine(tempDir, "invalid"))]; });
        }

        [Test]
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

                Assert.IsTrue(Target.IsStale(feed.Uri), "Non-cached feeds should be reported as stale");

                // No previous feed
                _feedCacheMock.Setup(x => x.Contains(feed.Uri)).Returns(false);
                _feedCacheMock.Setup(x => x.GetSignatures(feed.Uri)).Throws<KeyNotFoundException>();

                _feedCacheMock.Setup(x => x.Add(feed.Uri, data));
                _feedCacheMock.Setup(x => x.GetFeed(feed.Uri)).Returns(feed);

                _trustManagerMock.Setup(x => x.CheckTrust(data, feed.Uri, It.IsAny<string>())).Returns(OpenPgpUtilsTest.TestSignature);

                Assert.AreEqual(feed, Target[feed.Uri]);
            }
        }

        [Test]
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
                _feedCacheMock.Setup(x => x.Contains(feedUri)).Returns(false);

                // ReSharper disable once UnusedVariable
                Assert.Throws<InvalidDataException>(() => { var _ = Target[feedUri]; });
            }
        }

        [Test]
        public void DownloadFromMirror()
        {
            var feed = FeedTest.CreateTestFeed();
            feed.Uri = new FeedUri("http://invalid/directory/feed.xml");
            var data = feed.ToXmlString().ToStream().ToArray();

            // No previous feed
            _feedCacheMock.Setup(x => x.Contains(feed.Uri)).Returns(false);
            _feedCacheMock.Setup(x => x.GetSignatures(feed.Uri)).Throws<KeyNotFoundException>();

            _feedCacheMock.Setup(x => x.Add(feed.Uri, data));
            _feedCacheMock.Setup(x => x.GetFeed(feed.Uri)).Returns(feed);
            using (var mirrorServer = new MicroServer("feeds/http/invalid/directory%23feed.xml/latest.xml", new MemoryStream(data)))
            {
                // ReSharper disable once AccessToDisposedClosure
                _trustManagerMock.Setup(x => x.CheckTrust(data, feed.Uri, It.IsAny<string>())).Returns(OpenPgpUtilsTest.TestSignature);

                Config.FeedMirror = mirrorServer.ServerUri;
                Assert.AreEqual(feed, Target[feed.Uri]);
            }
        }

        [Test]
        public void DetectFreshCached()
        {
            var feed = new Feed();
            _feedCacheMock.Setup(x => x.Contains(FeedTest.Test1Uri)).Returns(true);
            _feedCacheMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(feed);
            new FeedPreferences {LastChecked = DateTime.UtcNow}.SaveFor(FeedTest.Test1Uri);

            Assert.IsFalse(Target.IsStale(FeedTest.Test1Uri));
            Assert.AreSame(feed, Target[FeedTest.Test1Uri]);
            Assert.IsFalse(Target.Stale);
        }

        [Test]
        public void ServeFromInMemoryCache()
        {
            DetectFreshCached();

            // ReSharper disable once UnusedVariable
            var _ = Target[FeedTest.Test1Uri];

            _feedCacheMock.Verify(x => x.GetFeed(FeedTest.Test1Uri), Times.Once(),
                failMessage: "Underlying cache was accessed more than once instead of being handled by the in-memory cache.");
        }

        [Test]
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

        [Test]
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
                _feedCacheMock.Setup(x => x.Contains(feed.Uri)).Returns(true);
                _feedCacheMock.Setup(x => x.GetFeed(feed.Uri)).Returns(feed);
                Assert.AreEqual(feed, Target[feed.Uri]);

                AssertRefreshData(feed, feedData);
            }
        }

        private void AssertRefreshData(Feed feed, byte[] feedData)
        {
            _feedCacheMock.Setup(x => x.Add(feed.Uri, feedData));
            _feedCacheMock.Setup(x => x.GetFeed(feed.Uri)).Returns(feed);
            _feedCacheMock.Setup(x => x.GetSignatures(feed.Uri)).Returns(new[] {OpenPgpUtilsTest.TestSignature});

            // ReSharper disable once AccessToDisposedClosure
            _trustManagerMock.Setup(x => x.CheckTrust(feedData, feed.Uri, It.IsAny<string>())).Returns(OpenPgpUtilsTest.TestSignature);

            Target.Refresh = true;
            Assert.AreEqual(feed, Target[feed.Uri]);
        }

        [Test]
        public void DetectStaleCached()
        {
            var feed = new Feed();
            _feedCacheMock.Setup(x => x.Contains(FeedTest.Test1Uri)).Returns(true);
            _feedCacheMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(feed);
            new FeedPreferences {LastChecked = DateTime.UtcNow - Config.Freshness}.SaveFor(FeedTest.Test1Uri);

            Assert.IsTrue(Target.IsStale(FeedTest.Test1Uri));
            Assert.AreSame(feed, Target[FeedTest.Test1Uri]);
            Assert.IsTrue(Target.Stale);
        }

        [Test(Description = "Ensures valid feeds are correctly imported.")]
        public void Import()
        {
            var feed = FeedTest.CreateTestFeed();
            var data = SignFeed(feed);

            // No previous feed
            _feedCacheMock.Setup(x => x.GetSignatures(feed.Uri)).Throws<KeyNotFoundException>();

            _feedCacheMock.Setup(x => x.Add(feed.Uri, data));
            using (var feedFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllBytes(feedFile, data);
                Target.ImportFeed(feedFile);
            }
        }

        [Test(Description = "Ensures replay attacks are detected.")]
        public void ImportReplayAttack()
        {
            var feed = FeedTest.CreateTestFeed();
            var data = SignFeed(feed);

            // Newer signautre present => replay attack
            _feedCacheMock.Setup(x => x.GetSignatures(feed.Uri)).Returns(new[] {new ValidSignature(OpenPgpUtilsTest.TestKeyID, OpenPgpUtilsTest.TestFingerprint, new DateTime(2002, 1, 1))});

            using (var feedFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllBytes(feedFile, data);
                Assert.Throws<ReplayAttackException>(() => Target.ImportFeed(feedFile));
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
            _trustManagerMock.Setup(x => x.CheckTrust(data, feed.Uri, It.IsAny<string>())).Returns(OpenPgpUtilsTest.TestSignature);
            return data;
        }
    }
}
