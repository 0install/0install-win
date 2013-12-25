﻿/*
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
using System.IO;
using System.Text;
using Common;
using Common.Utils;
using Moq;
using NUnit.Framework;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Injector
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

        private static readonly ValidSignature _signature = new ValidSignature("123abc", new DateTime(2000, 1, 1));
        private static readonly byte[] _keyData = {16, 32, 64};

        private const string KeyInfoResponse = @"<?xml version='1.0'?><key-lookup><item vote=""good"">Key information</item></key-lookup>";
        #endregion

        #region Shared
        private Mock<IOpenPgp> _openPgpMock;
        private Mock<IHandler> _handlerMock;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _openPgpMock = Container.GetMock<IOpenPgp>();
            _handlerMock = Container.GetMock<IHandler>();

            Config.KeyInfoServer = null;
            Config.AutoApproveKeys = false;
        }
        #endregion

        [Test]
        public void PreviouslyTrusted()
        {
            RegisterKey();
            TrustKey();

            Assert.AreEqual(_signature, Target.CheckTrust(new Uri("http://localhost/test.xml"), _combinedBytes));
        }

        [Test]
        public void BadSignature()
        {
            _openPgpMock.Setup(x => x.Verify(_feedBytes, _signatureBytes)).Returns(new OpenPgpSignature[] {new BadSignature(_signature.Fingerprint)});

            Assert.Throws<SignatureException>(() => Target.CheckTrust(new Uri("http://localhost/test.xml"), _combinedBytes));
            Assert.IsFalse(IsKeyTrusted, "Key should not be trusted");
        }

        [Test]
        public void MultipleSignatures()
        {
            _openPgpMock.Setup(x => x.Verify(_feedBytes, _signatureBytes)).Returns(new OpenPgpSignature[] {new BadSignature("xyz"), _signature}).Verifiable();
            TrustKey();

            Assert.AreEqual(_signature, Target.CheckTrust(new Uri("http://localhost/test.xml"), _combinedBytes));
        }

        [Test]
        public void ExistingKeyAndReject()
        {
            RegisterKey();
            AnswerQuestionWith(false);

            Assert.Throws<SignatureException>(() => Target.CheckTrust(new Uri("http://localhost/test.xml"), _combinedBytes));
            Assert.IsFalse(IsKeyTrusted, "Key should not be trusted");
        }

        [Test]
        public void ExistingKeyAndApprove()
        {
            RegisterKey();
            AnswerQuestionWith(true);

            Assert.AreEqual(_signature, Target.CheckTrust(new Uri("http://localhost/test.xml"), _combinedBytes));
            Assert.IsTrue(IsKeyTrusted, "Key should be trusted");
        }

        [Test]
        public void ExistingKeyAndNoAutoTrust()
        {
            RegisterKey();
            Container.GetMock<IFeedCache>().Setup(x => x.Contains("http://localhost/test.xml")).Returns(true);
            AnswerQuestionWith(false);

            using (var keyInfoServer = new MicroServer("key/" + _signature.Fingerprint, KeyInfoResponse.ToStream()))
            {
                UseKeyInfoServer(keyInfoServer);
                Assert.Throws<SignatureException>(() => Target.CheckTrust(new Uri("http://localhost/test.xml"), _combinedBytes));
            }
            Assert.IsFalse(IsKeyTrusted, "Key should not be trusted");
        }

        [Test]
        public void ExistingKeyAndAutoTrust()
        {
            RegisterKey();
            Container.GetMock<IFeedCache>().Setup(x => x.Contains("http://localhost/test.xml")).Returns(false);

            using (var keyInfoServer = new MicroServer("key/" + _signature.Fingerprint, KeyInfoResponse.ToStream()))
            {
                UseKeyInfoServer(keyInfoServer);
                Assert.AreEqual(_signature, Target.CheckTrust(new Uri("http://localhost/test.xml"), _combinedBytes));
            }
            Assert.IsTrue(IsKeyTrusted, "Key should be trusted");
        }

        [Test]
        public void DownloadKeyAndReject()
        {
            ExpectKeyImport();
            AnswerQuestionWith(false);

            using (var server = new MicroServer(_signature.Fingerprint + ".gpg", new MemoryStream(_keyData)))
                Assert.Throws<SignatureException>(() => Target.CheckTrust(new Uri(server.ServerUri, "test.xml"), _combinedBytes));
            Assert.IsFalse(IsKeyTrusted, "Key should not be trusted");
        }

        [Test]
        public void DownloadKeyAndApprove()
        {
            ExpectKeyImport();
            AnswerQuestionWith(true);

            using (var server = new MicroServer(_signature.Fingerprint + ".gpg", new MemoryStream(_keyData)))
                Assert.AreEqual(_signature, Target.CheckTrust(new Uri(server.ServerUri, "test.xml"), _combinedBytes));
            Assert.IsTrue(IsKeyTrusted, "Key should be trusted");
        }

        [Test]
        public void DownloadKeyFromMirrorAndApprove()
        {
            ExpectKeyImport();
            AnswerQuestionWith(true);

            using (var server = new MicroServer(_signature.Fingerprint + ".gpg", new MemoryStream(_keyData)))
                Assert.AreEqual(_signature, Target.CheckTrust(new Uri("http://localhost/test.xml"), _combinedBytes, new Uri(server.ServerUri, "test.xml")));
            Assert.IsTrue(IsKeyTrusted, "Key should be trusted");
        }

        private void RegisterKey()
        {
            _openPgpMock.Setup(x => x.Verify(_feedBytes, _signatureBytes)).Returns(new OpenPgpSignature[] {_signature}).Verifiable();
        }

        private static void TrustKey()
        {
            new TrustDB {Keys = {new Key {Domains = {new Domain("localhost")}, Fingerprint = _signature.Fingerprint}}}.Save();
        }

        private static bool IsKeyTrusted { get { return TrustDB.LoadSafe().IsTrusted(_signature.Fingerprint, new Domain {Value = "localhost"}); } }

        private void AnswerQuestionWith(bool answer)
        {
            _handlerMock.Setup(x => x.AskQuestion(It.IsAny<string>(), It.IsAny<string>())).Returns(answer).Verifiable();
        }

        private void ExpectKeyImport()
        {
            _openPgpMock.SetupSequence(x => x.Verify(_feedBytes, _signatureBytes))
                .Returns(new OpenPgpSignature[] {new MissingKeySignature(_signature.Fingerprint)})
                .Returns(new OpenPgpSignature[] {_signature});
            _openPgpMock.Setup(x => x.ImportKey(_keyData)).Verifiable();

            ProvideCancellationToken();
        }

        private void UseKeyInfoServer(MicroServer keyInfoServer)
        {
            Config.AutoApproveKeys = true;
            Config.KeyInfoServer = keyInfoServer.ServerUri;

            ProvideCancellationToken();
        }
    }
}
