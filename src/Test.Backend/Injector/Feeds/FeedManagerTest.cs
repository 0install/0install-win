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
        private Mock<IHandler> _handlerMock;

        private Policy _policy;
        private LocationsRedirect _redirect;

        [SetUp]
        public void SetUp()
        {
            _cacheMock = new Mock<IFeedCache>(MockBehavior.Strict);
            _openPgpMock = new Mock<IOpenPgp>(MockBehavior.Strict);
            _handlerMock = new Mock<IHandler>(MockBehavior.Loose);
            _policy = new Policy(
                new Config(), new FeedManager(_cacheMock.Object),
                new Mock<IFetcher>().Object, _openPgpMock.Object, new Mock<ISolver>().Object,
                _handlerMock.Object);

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

        [Test(Description = "Ensures valid feeds are correctly imported.")]
        public void TestImportFeed()
        {
            var feed = FeedTest.CreateTestFeed();
            var data = SignFeed(feed, false);
            var trustDB = new TrustDB();
            trustDB.TrustKey("fingerprint", new Domain(feed.Uri.Host));
            trustDB.Save();

            // No previous feed
            _cacheMock.Setup(x => x.GetSignatures(feed.Uri.ToString(), _policy.OpenPgp)).Throws<KeyNotFoundException>().Verifiable();

            _cacheMock.Setup(x => x.Add(feed.Uri.ToString(), data)).Verifiable();
            _policy.FeedManager.ImportFeed(feed.Uri, data, _policy);
        }

        [Test(Description = "Ensures feeds signed with keys not yet trusted are correctly imported.")]
        public void TestImportUntrustedKey()
        {
            var feed = FeedTest.CreateTestFeed();
            var data = SignFeed(feed, false);

            // No previous feed
            _cacheMock.Setup(x => x.GetSignatures(feed.Uri.ToString(), _policy.OpenPgp)).Throws<KeyNotFoundException>().Verifiable();

            // Reject new key
            _handlerMock.Setup(x => x.AskQuestion(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            Assert.Throws<SignatureException>(() => _policy.FeedManager.ImportFeed(feed.Uri, data, _policy));
            Assert.IsFalse(TrustDB.LoadSafe().IsTrusted("fingerprint", new Domain(feed.Uri.Host)));

            // Accept new key
            _handlerMock.Setup(x => x.AskQuestion(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _cacheMock.Setup(x => x.Add(feed.Uri.ToString(), data)).Verifiable();
            _policy.FeedManager.ImportFeed(feed.Uri, data, _policy);
            Assert.IsTrue(TrustDB.LoadSafe().IsTrusted("fingerprint", new Domain(feed.Uri.Host)));
        }

        [Test(Description = "Ensures feeds signed with new keys are correctly imported.")]
        public void TestImportNewKey()
        {
            // ToDo
            throw new InconclusiveException("Test not implemented yet");
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
            var feed = FeedTest.CreateTestFeed();
            var data = SignFeed(feed, false);
            var trustDB = new TrustDB();
            trustDB.TrustKey("fingerprint", new Domain("invalid"));
            trustDB.Save();

            Assert.Throws<InvalidInterfaceIDException>(() => _policy.FeedManager.ImportFeed(new Uri("http://invalid/"), data, _policy));
        }

        [Test(Description = "Ensures replay attacks are detected.")]
        public void TestImportFeedReplayAttack()
        {
            var feed = FeedTest.CreateTestFeed();
            var data = SignFeed(feed, false);
            var trustDB = new TrustDB();
            trustDB.TrustKey("fingerprint", new Domain(feed.Uri.Host));
            trustDB.Save();

            // Newer signautre present => replay attack
            _cacheMock.Setup(x => x.GetSignatures(feed.Uri.ToString(), _policy.OpenPgp)).Returns(new[] {new ValidSignature("fingerprint", new DateTime(2002, 1, 1))}).Verifiable();

            Assert.Throws<ReplayAttackException>(() => _policy.FeedManager.ImportFeed(feed.Uri, data, _policy));
        }

        /// <summary>
        /// Generates a byte array containing a feed and a mock signature. Configures <see cref="_openPgpMock"/> to validate this signature.
        /// </summary>
        /// <param name="feed">The feed to "sign".</param>
        /// <param name="missing">Make <see cref="_openPgpMock"/> report that the key file for the signature is missing and needs to be downloaded.</param>
        /// <returns>A byte array containing the serialized <paramref name="feed"/> and its mock signature.</returns>
        private byte[] SignFeed(Feed feed, bool missing)
        {
            var stream = new MemoryStream();
            feed.Save(stream);
            var feedData = stream.ToArray(); // Only feed data without signature
            var signature = new byte[] {1, 2, 3};

            // Add the Base64 encoded signature to the end of the stream
            var writer = new StreamWriter(stream, FeedUtils.Encoding) {NewLine = "\n", AutoFlush = true};
            writer.Write(FeedUtils.SignatureBlockStart);
            writer.WriteLine(Convert.ToBase64String(signature));
            writer.Write(FeedUtils.SignatureBlockEnd);

            _openPgpMock.Setup(x => x.Verify(feedData, signature)).Returns(new[]
            {
                missing
                    ? (OpenPgpSignature)new MissingKeySignature("id")
                    : new ValidSignature("fingerprint", new DateTime(2000, 1, 1))
            }).Verifiable();

            return stream.ToArray(); // Feed data with signature
        }
    }
}
