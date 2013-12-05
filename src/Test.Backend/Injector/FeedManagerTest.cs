/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common;
using Common.Storage;
using Common.Utils;
using Moq;
using NUnit.Framework;
using ZeroInstall.Model;
using ZeroInstall.Model.Preferences;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Contains test methods for <see cref="FeedManager"/>.
    /// </summary>
    [TestFixture]
    public class FeedManagerTest : TestWithResolver<FeedManager>
    {
        #region Shared
        private Mock<IFeedCache> _feedCacheMock;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _feedCacheMock = Resolver.GetMock<IFeedCache>();
        }
        #endregion

        [Test]
        public void Local()
        {
            var feed = new Feed();

            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // ReSharper disable once AccessToDisposedClosure
                _feedCacheMock.Setup(x => x.GetFeed(tempFile)).Returns(feed).Verifiable();

                Assert.AreSame(feed, Target.GetFeed(tempFile));
                Assert.IsFalse(Target.Stale);
            }
        }

        [Test]
        public void LocalMissing()
        {
            Assert.Throws<FileNotFoundException>(() => Target.GetFeed("invalid-" + Path.GetRandomFileName()));
        }

        [Test]
        public void Download()
        {
            var feed = FeedTest.CreateTestFeed();
            var feedStream = new MemoryStream();
            using (var server = new MicroServer("feed.xml", feedStream))
            {
                feed.Uri = server.FileUri;
                feed.SaveXml(feedStream);
                var data = feedStream.ToArray();
                feedStream.Position = 0;

                // No previous feed
                _feedCacheMock.Setup(x => x.Contains(feed.Uri.ToString())).Returns(false).Verifiable();
                _feedCacheMock.Setup(x => x.GetSignatures(feed.Uri.ToString())).Throws<KeyNotFoundException>().Verifiable();

                _feedCacheMock.Setup(x => x.Add(feed.Uri.ToString(), data)).Verifiable();
                _feedCacheMock.Setup(x => x.GetFeed(feed.Uri.ToString())).Returns(feed).Verifiable();

                // ReSharper disable once AccessToDisposedClosure
                Resolver.GetMock<ITrustManager>().Setup(x => x.CheckTrust(feed.Uri, data, null))
                    .Returns(new ValidSignature("fingerprint", new DateTime(2000, 1, 1)));

                ProvideCancellationToken();
                Assert.AreEqual(feed, Target.GetFeed(feed.Uri.ToString()));
            }
        }

        [Test]
        public void DownloadFromMirror()
        {
            var feed = FeedTest.CreateTestFeed();
            feed.Uri = new Uri("http://invalid/directory/feed.xml");
            var data = feed.ToXmlString().ToStream().ToArray();

            // No previous feed
            _feedCacheMock.Setup(x => x.Contains(feed.Uri.ToString())).Returns(false).Verifiable();
            _feedCacheMock.Setup(x => x.GetSignatures(feed.Uri.ToString())).Throws<KeyNotFoundException>().Verifiable();

            _feedCacheMock.Setup(x => x.Add(feed.Uri.ToString(), data)).Verifiable();
            _feedCacheMock.Setup(x => x.GetFeed(feed.Uri.ToString())).Returns(feed).Verifiable();
            using (var mirrorServer = new MicroServer("feeds/http/invalid/directory%23feed.xml/latest.xml", new MemoryStream(data)))
            {
                // ReSharper disable once AccessToDisposedClosure
                Resolver.GetMock<ITrustManager>().Setup(x => x.CheckTrust(feed.Uri, data, mirrorServer.FileUri))
                    .Returns(new ValidSignature("fingerprint", new DateTime(2000, 1, 1)));

                Config.FeedMirror = mirrorServer.ServerUri;
                ProvideCancellationToken();
                Assert.AreEqual(feed, Target.GetFeed(feed.Uri.ToString()));
            }
        }

        [Test]
        public void DetectFreshCached()
        {
            var feed = new Feed();
            _feedCacheMock.Setup(x => x.Contains("http://test/feed.xml")).Returns(true).Verifiable();
            _feedCacheMock.Setup(x => x.GetFeed("http://test/feed.xml")).Returns(feed).Verifiable();
            new FeedPreferences {LastChecked = DateTime.UtcNow}.SaveFor("http://test/feed.xml");

            Assert.AreSame(feed, Target.GetFeed("http://test/feed.xml"));
            Assert.IsFalse(Target.Stale);
        }

        [Test]
        public void Refresh()
        {
            var feed = FeedTest.CreateTestFeed();
            var feedStream = new MemoryStream();
            using (var server = new MicroServer("feed.xml", feedStream))
            {
                feed.Uri = server.FileUri;
                feed.SaveXml(feedStream);
                var data = feedStream.ToArray();
                feedStream.Position = 0;

                _feedCacheMock.Setup(x => x.Add(feed.Uri.ToString(), data)).Verifiable();
                _feedCacheMock.Setup(x => x.GetFeed(feed.Uri.ToString())).Returns(feed).Verifiable();

                // ReSharper disable once AccessToDisposedClosure
                Resolver.GetMock<ITrustManager>().Setup(x => x.CheckTrust(feed.Uri, data, null))
                    .Returns(new ValidSignature("fingerprint", new DateTime(2000, 1, 1)));

                ProvideCancellationToken();
                Target.Refresh = true;
                Assert.AreEqual(feed, Target.GetFeed(feed.Uri.ToString()));
            }
        }

        [Test]
        public void DetectStaleCached()
        {
            var feed = new Feed();
            _feedCacheMock.Setup(x => x.Contains("http://test/feed.xml")).Returns(true).Verifiable();
            _feedCacheMock.Setup(x => x.GetFeed("http://test/feed.xml")).Returns(feed).Verifiable();
            new FeedPreferences {LastChecked = DateTime.UtcNow - Config.Freshness}.SaveFor("http://test/feed.xml");

            Assert.AreSame(feed, Target.GetFeed("http://test/feed.xml"));
            Assert.IsTrue(Target.Stale);
        }

        [Test(Description = "Ensures valid feeds are correctly imported.")]
        public void Import()
        {
            var feed = FeedTest.CreateTestFeed();
            var data = SignFeed(feed);

            // No previous feed
            _feedCacheMock.Setup(x => x.GetSignatures(feed.Uri.ToString())).Throws<KeyNotFoundException>().Verifiable();

            _feedCacheMock.Setup(x => x.Add(feed.Uri.ToString(), data)).Verifiable();
            Target.ImportFeed(feed.Uri, null, data);
        }

        [Test(Description = "Ensures feeds with incorrect URIs are rejected.")]
        public void ImportIncorrectUri()
        {
            var feed = FeedTest.CreateTestFeed();
            var data = feed.ToXmlString().ToStream().ToArray();

            Assert.Throws<InvalidInterfaceIDException>(() => Target.ImportFeed(new Uri("http://invalid/"), null, data));
        }

        [Test(Description = "Ensures replay attacks are detected.")]
        public void ImportReplayAttack()
        {
            var feed = FeedTest.CreateTestFeed();
            var data = SignFeed(feed);

            // Newer signautre present => replay attack
            _feedCacheMock.Setup(x => x.GetSignatures(feed.Uri.ToString())).Returns(new[] {new ValidSignature("fingerprint", new DateTime(2002, 1, 1))}).Verifiable();

            Assert.Throws<ReplayAttackException>(() => Target.ImportFeed(feed.Uri, null, data));
        }

        /// <summary>
        /// Generates a byte array containing a feed and a mock signature. Configures <see cref="IOpenPgp"/> to validate this signature.
        /// </summary>
        /// <param name="feed">The feed to "sign".</param>
        /// <returns>A byte array containing the serialized <paramref name="feed"/> and its mock signature.</returns>
        private byte[] SignFeed(Feed feed)
        {
            var data = feed.ToXmlString().ToStream().ToArray();
            Resolver.GetMock<ITrustManager>().Setup(x => x.CheckTrust(feed.Uri, data, null)).
                Returns(new ValidSignature("fingerprint", new DateTime(2000, 1, 1)));
            return data;
        }
    }
}
