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
        private static readonly ValidSignature _signature = new ValidSignature("fingerprint", new DateTime(2000, 1, 1));

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

                Assert.AreSame(feed, Target.GetFeed(new FeedUri(feedFile)));
                Assert.IsFalse(Target.Stale);
            }
        }

        [Test]
        public void LocalMissing()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
                Assert.Throws<FileNotFoundException>(() => Target.GetFeed(new FeedUri(Path.Combine(tempDir, "invalid"))));
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

                // ReSharper disable once AccessToDisposedClosure
                _trustManagerMock.Setup(x => x.CheckTrust(data, feed.Uri, null)).Returns(_signature);

                Assert.AreEqual(feed, Target.GetFeed(feed.Uri));
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
                _trustManagerMock.Setup(x => x.CheckTrust(data, feed.Uri, new FeedUri(mirrorServer.FileUri))).Returns(_signature);

                Resolve<Config>().FeedMirror = mirrorServer.ServerUri;
                Assert.AreEqual(feed, Target.GetFeed(feed.Uri));
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
            Assert.AreSame(feed, Target.GetFeed(FeedTest.Test1Uri));
            Assert.IsFalse(Target.Stale);
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
                var data = feedStream.ToArray();
                feedStream.Position = 0;

                _feedCacheMock.Setup(x => x.Add(feed.Uri, data));
                _feedCacheMock.Setup(x => x.GetFeed(feed.Uri)).Returns(feed);
                _feedCacheMock.Setup(x => x.GetSignatures(feed.Uri)).Returns(new[] {_signature});

                // ReSharper disable once AccessToDisposedClosure
                _trustManagerMock.Setup(x => x.CheckTrust(data, feed.Uri, null)).Returns(_signature);

                Target.Refresh = true;
                Assert.AreEqual(feed, Target.GetFeed(feed.Uri));
            }
        }

        [Test]
        public void DetectStaleCached()
        {
            var feed = new Feed();
            _feedCacheMock.Setup(x => x.Contains(FeedTest.Test1Uri)).Returns(true);
            _feedCacheMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(feed);
            new FeedPreferences {LastChecked = DateTime.UtcNow - Resolve<Config>().Freshness}.SaveFor(FeedTest.Test1Uri);

            Assert.IsTrue(Target.IsStale(FeedTest.Test1Uri));
            Assert.AreSame(feed, Target.GetFeed(FeedTest.Test1Uri));
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
                Target.ImportFeed(feedFile, feed.Uri);
            }
        }

        [Test(Description = "Ensures feeds with incorrect URIs are rejected.")]
        public void ImportIncorrectUri()
        {
            var feed = FeedTest.CreateTestFeed();
            var data = feed.ToXmlString().ToStream().ToArray();

            _trustManagerMock.Setup(x => x.CheckTrust(data, new FeedUri("http://invalid/"), null)).Returns(_signature);
            using (var feedFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllBytes(feedFile, data);
                Assert.Throws<UriFormatException>(() => Target.ImportFeed(feedFile, new FeedUri("http://invalid/")));
            }
        }

        [Test(Description = "Ensures replay attacks are detected.")]
        public void ImportReplayAttack()
        {
            var feed = FeedTest.CreateTestFeed();
            var data = SignFeed(feed);

            // Newer signautre present => replay attack
            _feedCacheMock.Setup(x => x.GetSignatures(feed.Uri)).Returns(new[] {new ValidSignature(_signature.Fingerprint, new DateTime(2002, 1, 1))});

            using (var feedFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllBytes(feedFile, data);
                Assert.Throws<ReplayAttackException>(() => Target.ImportFeed(feedFile, feed.Uri));
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
            _trustManagerMock.Setup(x => x.CheckTrust(data, feed.Uri, null)).Returns(_signature);
            return data;
        }
    }
}
