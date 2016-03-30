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
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Contains test methods for <see cref="FeedCacheExtentsions"/>.
    /// </summary>
    [TestFixture]
    public class FeedCacheExtentsionsTest : TestWithMocks
    {
        /// <summary>
        /// Ensures <see cref="FeedCacheExtentsions.GetAll"/> correctly loads <see cref="Feed"/>s from an <see cref="IFeedCache"/>, skipping any exceptions.
        /// </summary>
        [Test]
        public void TestGetFeeds()
        {
            var feed1 = FeedTest.CreateTestFeed();
            var feed3 = FeedTest.CreateTestFeed();
            feed3.Uri = FeedTest.Test3Uri;

            var cacheMock = CreateMock<IFeedCache>();
            cacheMock.Setup(x => x.ListAll()).Returns(new[] {FeedTest.Test1Uri, FeedTest.Test2Uri, FeedTest.Test3Uri});
            cacheMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(feed1);
            cacheMock.Setup(x => x.GetFeed(FeedTest.Test2Uri)).Throws(new InvalidDataException("Fake exception for testing"));
            cacheMock.Setup(x => x.GetFeed(FeedTest.Test3Uri)).Returns(feed3);

            cacheMock.Object.GetAll().Should().Equal(feed1, feed3);
        }

        private const string FeedText = "Feed data\n";
        private readonly byte[] _feedBytes = Encoding.UTF8.GetBytes(FeedText);
        private static readonly byte[] _signatureBytes = Encoding.UTF8.GetBytes("Signature data");
        private static readonly string _signatureBase64 = Convert.ToBase64String(_signatureBytes).Insert(10, "\n");

        /// <summary>
        /// Ensures that <see cref="FeedUtils.GetSignatures"/> correctly separates an XML signature block from a signed feed.
        /// </summary>
        [Test]
        public void TestGetSignatures()
        {
            var openPgpMock = CreateMock<IOpenPgp>();
            var result = new OpenPgpSignature[] {OpenPgpUtilsTest.TestSignature};
            openPgpMock.Setup(x => x.Verify(_feedBytes, _signatureBytes)).Returns(result);

            string input = FeedText + FeedUtils.SignatureBlockStart + _signatureBase64 + FeedUtils.SignatureBlockEnd;
            FeedUtils.GetSignatures(openPgpMock.Object, Encoding.UTF8.GetBytes(input)).Should().Equal(result);
        }

        /// <summary>
        /// Ensures that <see cref="FeedUtils.GetSignatures"/> throws a <see cref="SignatureException"/> if the signature block does not start in a new line.
        /// </summary>
        [Test]
        public void TestGetSignaturesMissingNewLine()
        {
            string input = "Feed without newline" + FeedUtils.SignatureBlockStart + _signatureBase64 + FeedUtils.SignatureBlockEnd;
            Assert.Throws<SignatureException>(() => FeedUtils.GetSignatures(new Mock<IOpenPgp>().Object, Encoding.UTF8.GetBytes(input)));
        }

        /// <summary>
        /// Ensures that <see cref="FeedUtils.GetSignatures" /> throws a <see cref="SignatureException"/> if the signature contains non-base 64 characters.
        /// </summary>
        [Test]
        public void TestGetSignaturesInvalidChars()
        {
            const string input = FeedText + FeedUtils.SignatureBlockStart + "*!?#" + FeedUtils.SignatureBlockEnd;
            Assert.Throws<SignatureException>(() => FeedUtils.GetSignatures(new Mock<IOpenPgp>().Object, Encoding.UTF8.GetBytes(input)));
        }

        /// <summary>
        /// Ensures that <see cref="FeedUtils.GetSignatures" /> throws a <see cref="SignatureException"/> if the correct signature end is missing.
        /// </summary>
        [Test]
        public void TestGetSignaturesMissingEnd()
        {
            string input = FeedText + FeedUtils.SignatureBlockStart + _signatureBase64;
            Assert.Throws<SignatureException>(() => FeedUtils.GetSignatures(new Mock<IOpenPgp>().Object, Encoding.UTF8.GetBytes(input)));
        }

        /// <summary>
        /// Ensures that <see cref="FeedUtils.GetSignatures" /> throws a <see cref="SignatureException"/> if there is additional data after the signature block.
        /// </summary>
        [Test]
        public void TestGetSignaturesDataAfterSignature()
        {
            string input = FeedText + FeedUtils.SignatureBlockStart + _signatureBase64 + FeedUtils.SignatureBlockEnd + "more data";
            Assert.Throws<SignatureException>(() => FeedUtils.GetSignatures(new Mock<IOpenPgp>().Object, Encoding.UTF8.GetBytes(input)));
        }
    }
}
