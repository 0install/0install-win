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
using System.IO;
using System.Text;
using FluentAssertions;
using Moq;
using NanoByte.Common.Streams;
using NUnit.Framework;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Services.Feeds
{
    /// <summary>
    /// Contains test methods for <see cref="TrustManager"/>.
    /// </summary>
    [TestFixture]
    public class TrustManagerTest : TestWithContainer<TrustManager>
    {
        #region Constants
        private const string FeedText = "Feed data\n";
        private readonly byte[] _feedBytes = Encoding.UTF8.GetBytes(FeedText);
        private static readonly byte[] _signatureBytes = Encoding.UTF8.GetBytes("Signature data");
        private static readonly string _signatureBase64 = Convert.ToBase64String(_signatureBytes).Insert(10, "\n");
        private static readonly byte[] _combinedBytes = Encoding.UTF8.GetBytes(FeedText + FeedUtils.SignatureBlockStart + _signatureBase64 + FeedUtils.SignatureBlockEnd);

        private static readonly byte[] _keyData = Encoding.ASCII.GetBytes("key");

        private const string KeyInfoResponse = @"<?xml version='1.0'?><key-lookup><item vote=""good"">Key information</item></key-lookup>";
        #endregion

        private Mock<IOpenPgp> OpenPgpMock { get { return GetMock<IOpenPgp>(); } }
        private Mock<IFeedCache> FeedCacheMock {  get { return GetMock<IFeedCache>(); } }

        protected override void Register(AutoMockContainer container)
        {
            base.Register(container);

            Config.KeyInfoServer = null;
            Config.AutoApproveKeys = false;
        }

        [Test]
        public void PreviouslyTrusted()
        {
            RegisterKey();
            TrustKey();

            Target.CheckTrust(_combinedBytes, new FeedUri("http://localhost/test.xml"))
                .Should().Be(OpenPgpUtilsTest.TestSignature);
        }

        [Test]
        public void BadSignature()
        {
            OpenPgpMock.Setup(x => x.Verify(_feedBytes, _signatureBytes)).Returns(new OpenPgpSignature[] {new BadSignature(keyID: 123)});

            Target.Invoking(x => x.CheckTrust(_combinedBytes, new FeedUri("http://localhost/test.xml")))
                .ShouldThrow<SignatureException>();
            IsKeyTrusted().Should().BeFalse(because: "Key should not be trusted");
        }

        [Test]
        public void MultipleSignatures()
        {
            OpenPgpMock.Setup(x => x.Verify(_feedBytes, _signatureBytes)).Returns(new OpenPgpSignature[] {new BadSignature(keyID: 123), OpenPgpUtilsTest.TestSignature});
            TrustKey();

            Target.CheckTrust(_combinedBytes, new FeedUri("http://localhost/test.xml"))
                .Should().Be(OpenPgpUtilsTest.TestSignature);
        }

        [Test]
        public void ExistingKeyAndReject()
        {
            RegisterKey();
            Handler.AnswerQuestionWith = false;

            Target.Invoking(x => x.CheckTrust(_combinedBytes, new FeedUri("http://localhost/test.xml")))
                .ShouldThrow<SignatureException>();
            IsKeyTrusted().Should().BeFalse(because: "Key should not be trusted");
        }

        [Test]
        public void ExistingKeyAndApprove()
        {
            RegisterKey();
            Handler.AnswerQuestionWith = true;

            Target.CheckTrust(_combinedBytes, new FeedUri("http://localhost/test.xml"))
                .Should().Be(OpenPgpUtilsTest.TestSignature);
            IsKeyTrusted().Should().BeTrue(because: "Key should be trusted");
        }

        [Test]
        public void ExistingKeyAndNoAutoTrust()
        {
            RegisterKey();
            FeedCacheMock.Setup(x => x.Contains(new FeedUri("http://localhost/test.xml"))).Returns(true);
            Handler.AnswerQuestionWith = false;

            using (var keyInfoServer = new MicroServer("key/" + OpenPgpUtilsTest.TestSignature.FormatFingerprint(), KeyInfoResponse.ToStream()))
            {
                UseKeyInfoServer(keyInfoServer);
                Target.Invoking(x => x.CheckTrust(_combinedBytes, new FeedUri("http://localhost/test.xml")))
                    .ShouldThrow<SignatureException>();
            }
            IsKeyTrusted().Should().BeFalse(because: "Key should not be trusted");
        }

        [Test]
        public void ExistingKeyAndAutoTrust()
        {
            RegisterKey();
            FeedCacheMock.Setup(x => x.Contains(new FeedUri("http://localhost/test.xml"))).Returns(false);

            using (var keyInfoServer = new MicroServer("key/" + OpenPgpUtilsTest.TestSignature.FormatFingerprint(), KeyInfoResponse.ToStream()))
            {
                UseKeyInfoServer(keyInfoServer);
                Target.CheckTrust(_combinedBytes, new FeedUri("http://localhost/test.xml"))
                    .Should().Be(OpenPgpUtilsTest.TestSignature);
            }
            IsKeyTrusted().Should().BeTrue(because: "Key should be trusted");
        }

        [Test]
        public void DownloadKeyAndReject()
        {
            ExpectKeyImport();
            Handler.AnswerQuestionWith = false;

            using (var server = new MicroServer(OpenPgpUtilsTest.TestKeyIDString + ".gpg", new MemoryStream(_keyData)))
            {
                Target.Invoking(x => x.CheckTrust(_combinedBytes, new FeedUri(server.ServerUri + "test.xml")))
                    .ShouldThrow<SignatureException>();
            }
            IsKeyTrusted().Should().BeFalse(because: "Key should not be trusted");
        }

        [Test]
        public void DownloadKeyAndApprove()
        {
            ExpectKeyImport();
            Handler.AnswerQuestionWith = true;

            using (var server = new MicroServer(OpenPgpUtilsTest.TestKeyIDString + ".gpg", new MemoryStream(_keyData)))
            {
                Target.CheckTrust(_combinedBytes, new FeedUri(server.ServerUri + "test.xml"))
                    .Should().Be(OpenPgpUtilsTest.TestSignature);
            }
            IsKeyTrusted().Should().BeTrue(because: "Key should be trusted");
        }

        [Test]
        public void DownloadKeyFromMirrorAndApprove()
        {
            ExpectKeyImport();
            Handler.AnswerQuestionWith = true;

            using (var server = new MicroServer("keys/" + OpenPgpUtilsTest.TestKeyIDString + ".gpg", new MemoryStream(_keyData)))
            {
                Config.FeedMirror = server.ServerUri;
                Target.CheckTrust(_combinedBytes, new FeedUri("http://localhost/test/feed.xml"))
                    .Should().Be(OpenPgpUtilsTest.TestSignature);
            }
            IsKeyTrusted().Should().BeTrue(because: "Key should be trusted");
        }

        private void RegisterKey()
        {
            OpenPgpMock.Setup(x => x.Verify(_feedBytes, _signatureBytes)).Returns(new OpenPgpSignature[] {OpenPgpUtilsTest.TestSignature});
        }

        private void TrustKey()
        {
            TrustDB.TrustKey(OpenPgpUtilsTest.TestSignature.FormatFingerprint(), new Domain("localhost"));
        }

        private bool IsKeyTrusted()
        {
            return TrustDB.IsTrusted(OpenPgpUtilsTest.TestSignature.FormatFingerprint(), new Domain {Value = "localhost"});
        }

        private void ExpectKeyImport()
        {
            OpenPgpMock.SetupSequence(x => x.Verify(_feedBytes, _signatureBytes))
                .Returns(new OpenPgpSignature[] {new MissingKeySignature(OpenPgpUtilsTest.TestKeyID)})
                .Returns(new OpenPgpSignature[] {OpenPgpUtilsTest.TestSignature});
            OpenPgpMock.Setup(x => x.ImportKey(_keyData));
        }

        private void UseKeyInfoServer(MicroServer keyInfoServer)
        {
            Config.AutoApproveKeys = true;
            Config.KeyInfoServer = keyInfoServer.ServerUri;
        }
    }
}
