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
using System.IO;
using FluentAssertions;
using Moq;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using Xunit;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Contains test methods for <see cref="FeedUtils"/>.
    /// </summary>
    public class FeedUtilsTest : TestWithMocks
    {
        /// <summary>
        /// Ensures <see cref="FeedUtils.SignFeed"/> produces valid feed files.
        /// </summary>
        [Fact]
        public void TestSignFeed()
        {
            using (var stream = new MemoryStream())
            {
                var feed = FeedTest.CreateTestFeed();
                const string passphrase = "passphrase123";
                var signature = new byte[] {1, 2, 3};
                var secretKey = new OpenPgpSecretKey(keyID: 123, fingerprint: new byte[] {1, 2, 3}, userID: "user");

                var openPgpMock = CreateMock<IOpenPgp>();
                openPgpMock.Setup(x => x.Sign(It.IsAny<byte[]>(), secretKey, passphrase))
                    .Returns(signature);
                feed.SaveXml(stream);
                FeedUtils.SignFeed(stream, secretKey, passphrase, openPgpMock.Object);

                string signedFeed = stream.ReadToString();
                string expectedFeed = feed.ToXmlString() + Store.Feeds.FeedUtils.SignatureBlockStart +
                                      Convert.ToBase64String(signature) + "\n" + Store.Feeds.FeedUtils.SignatureBlockEnd;
                signedFeed.Should().Be(expectedFeed, because: "Feed should remain unchanged except for appended XML signatre");
            }
        }
    }
}
