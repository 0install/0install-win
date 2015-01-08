/*
 * Copyright 2010-2014 Bastian Eicher
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
using Moq;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NUnit.Framework;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Contains test methods for <see cref="FeedUtils"/>.
    /// </summary>
    [TestFixture]
    public class FeedUtilsTest : TestWithMocks
    {
        /// <summary>
        /// Ensures <see cref="FeedUtils.SignFeed"/> produces valid feed files.
        /// </summary>
        [Test]
        public void TestSignFeed()
        {
            using (var stream = new MemoryStream())
            {
                var feed = FeedTest.CreateTestFeed();
                var secretKey = new OpenPgpSecretKey("fingerprint", "key", "user@0install.de", new DateTime(2000, 1, 1), OpenPgpAlgorithm.Rsa, 128);
                var openPgpMock = MockRepository.Create<IOpenPgp>();
                const string passphrase = "passphrase123";
                const string signature = "iQEcB";

                openPgpMock.Setup(x => x.DetachSign(It.IsAny<Stream>(), secretKey.Fingerprint, passphrase))
                    .Returns(signature);
                feed.SaveXml(stream);
                FeedUtils.SignFeed(stream, secretKey, passphrase, openPgpMock.Object);

                string signedFeed = stream.ReadToString();
                string expectedFeed = feed.ToXmlString() + Store.Feeds.FeedUtils.SignatureBlockStart +
                                      signature + "\n" + Store.Feeds.FeedUtils.SignatureBlockEnd;
                Assert.AreEqual(expectedFeed, signedFeed,
                    "Feed should remain unchanged except for appended XML signatre");
            }
        }

        /// <summary>
        /// Ensures <see cref="FeedUtils.DeployPublicKey"/> produces key files properly.
        /// </summary>
        [Test]
        public void TestDeployPublicKey()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                var secretKey = new OpenPgpSecretKey("fingerprint", "key", "user@0install.de", new DateTime(2000, 1, 1), OpenPgpAlgorithm.Rsa, 128);
                const string publicKey = "public";
                var openPgpMock = MockRepository.Create<IOpenPgp>();

                openPgpMock.Setup(x => x.GetPublicKey(secretKey.Fingerprint)).Returns(publicKey);
                FeedUtils.DeployPublicKey(tempDir.Path, secretKey, openPgpMock.Object);

                Assert.AreEqual(publicKey, File.ReadAllText(tempDir + Path.DirectorySeparatorChar + secretKey.KeyID + ".gpg"),
                    "Public key should be written to parallel file in directory");
            }
        }
    }
}
