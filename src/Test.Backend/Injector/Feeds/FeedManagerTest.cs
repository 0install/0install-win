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
using Common.Storage;
using NUnit.Framework;
using Moq;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector.Feeds
{
    /// <summary>
    /// Contains test methods for <see cref="FeedManager"/>.
    /// </summary>
    [TestFixture]
    public class FeedManagerTest
    {
        #region Shared
        private Mock<IFeedCache> _cacheMock;
        private Mock<IOpenPgp> _openPgpMock;
        private Policy _policy;

        private LocationsRedirect _redirect;

        [SetUp]
        public void SetUp()
        {
            _cacheMock = new Mock<IFeedCache>(MockBehavior.Strict);
            _openPgpMock = new Mock<IOpenPgp>(MockBehavior.Strict);
            _policy = new Policy(
                new Config(), new FeedManager(_cacheMock.Object),
                new Mock<IFetcher>().Object, _openPgpMock.Object, new Mock<ISolver>().Object, new SilentHandler());

            // Don't store generated executables settings in real user profile
            _redirect = new LocationsRedirect("0install-unit-tests");
        }

        [TearDown]
        public void TearDown()
        {
            _redirect.Dispose();

            _cacheMock.Verify();
            _openPgpMock.Verify();
        }
        #endregion

        [Test(Description = "Ensures local feed files are loaded correctly.")]
        public void TestLocal()
        {
            var feed = new Feed();

            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // ReSharper disable AccessToDisposedClosure
                _cacheMock.Setup(x => x.GetFeed(tempFile.Path)).Returns(feed).Verifiable();
                // ReSharper restore AccessToDisposedClosure

                bool stale;
                Assert.AreSame(feed, _policy.FeedManager.GetFeed(tempFile.Path, _policy, out stale));
                Assert.IsFalse(stale);
            }
        }

        [Test(Description = "Ensures missing local feed files are reported correctly.")]
        public void TestLocalMissing()
        {
            bool stale;
            Assert.Throws<FileNotFoundException>(() => _policy.FeedManager.GetFeed("invalid-" + Path.GetRandomFileName(), _policy, out stale));
        }

        [Test(Description = "Ensures cached feeds that are not stale are returned correctly.")]
        public void TestCachedFresh()
        {
            var feed = new Feed();

            _cacheMock.Setup(x => x.Contains("http://0install.de/feeds/test/test1.xml")).Returns(true).Verifiable();
            _cacheMock.Setup(x => x.GetFeed("http://0install.de/feeds/test/test1.xml")).Returns(feed).Verifiable();

            new FeedPreferences {LastChecked = DateTime.UtcNow}.SaveFor("http://0install.de/feeds/test/test1.xml");

            bool stale;
            Assert.AreSame(feed, _policy.FeedManager.GetFeed("http://0install.de/feeds/test/test1.xml", _policy, out stale));
            Assert.IsFalse(stale);
        }

        [Test(Description = "Ensures cached feeds that are stale are returned correctly.")]
        public void TestCachedStale()
        {
            var feed = new Feed();

            _cacheMock.Setup(x => x.Contains("http://0install.de/feeds/test/test1.xml")).Returns(true).Verifiable();
            _cacheMock.Setup(x => x.GetFeed("http://0install.de/feeds/test/test1.xml")).Returns(feed).Verifiable();

            new FeedPreferences {LastChecked = DateTime.UtcNow - _policy.Config.Freshness}.SaveFor("http://0install.de/feeds/test/test1.xml");

            bool stale;
            Assert.AreSame(feed, _policy.FeedManager.GetFeed("http://0install.de/feeds/test/test1.xml", _policy, out stale));
            Assert.IsTrue(stale);
        }

        [Test(Description = "Ensures feeds without signatures are rejected.")]
        public void TestImportFeedMissingSignature()
        {
            var feed = FeedTest.CreateTestFeed();
            Assert.Throws<SignatureException>(() => _policy.FeedManager.ImportFeed(feed.Uri, feed.ToArray(), _policy));
        }

        [Test(Description = "Ensures feeds with incorrect URIs are rejected.")]
        public void TestImportFeedIncorrectUri()
        {
            var stream = new MemoryStream();
            FeedTest.CreateTestFeed().Save(stream);
            var feedData = stream.ToArray();
            var signature = new byte[] {1, 2, 3};

            // Add the Base64 encoded signature to the end of the stream
            using (var writer = new StreamWriter(stream, FeedUtils.Encoding) {NewLine = "\n"})
            {
                writer.Write(FeedUtils.SignatureBlockStart);
                writer.WriteLine(Convert.ToBase64String(signature));
                writer.Write(FeedUtils.SignatureBlockEnd);
            }

            var trustDB = new TrustDB();
            trustDB.TrustKey("fingerprint", new Domain("invalid"));
            trustDB.Save();
            _openPgpMock.Setup(x => x.Verify(feedData, signature)).Returns(new[] {new ValidSignature("fingerprint", new DateTime(2000, 1, 1))}).Verifiable();

            Assert.Throws<InvalidInterfaceIDException>(() => _policy.FeedManager.ImportFeed(new Uri("http://invalid/"), stream.ToArray(), _policy));
        }
    }
}
