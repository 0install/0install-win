/*
 * Copyright 2010-2011 Bastian Eicher
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
using NUnit.Framework;
using NUnit.Mocks;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Contains test methods for <see cref="FeedUtils"/>.
    /// </summary>
    [TestFixture]
    public class FeedUtilsTest
    {
        /// <summary>
        /// Ensures <see cref="FeedUtils.GetFeeds"/> correctly loads <see cref="Feed"/>s from an <see cref="IFeedCache"/>, skipping any exceptions.
        /// </summary>
        [Test]
        public void TestGetFeeds()
        {
            var feed1 = FeedTest.CreateTestFeed();
            var feed3 = FeedTest.CreateTestFeed();
            feed3.Uri = new Uri("http://0install.de/feeds/test/test3.xml");
            
            var cacheMock = new DynamicMock("MockCache", typeof(IFeedCache));
            cacheMock.ExpectAndReturn("ListAll", new[] {"http://0install.de/feeds/test/test1.xml", "http://0install.de/feeds/test/test2.xml", "http://0install.de/feeds/test/test3.xml"});
            cacheMock.ExpectAndReturn("GetFeed", feed1, "http://0install.de/feeds/test/test1.xml");
            cacheMock.ExpectAndThrow("GetFeed", new IOException("Fake IO exception for testing"), "http://0install.de/feeds/test/test2.xml");
            cacheMock.ExpectAndReturn("GetFeed", feed3, "http://0install.de/feeds/test/test3.xml");

            CollectionAssert.AreEqual(new[] {feed1, feed3}, FeedUtils.GetFeeds((IFeedCache)cacheMock.MockInstance));
        }

        /// <summary>
        /// Ensures that <see cref="FeedUtils.GetSignatures" /> throws an <see cref="SignatureException"/> if the signature blockfirst signature is not in a new line.
        /// </summary>
        [Test]
        public void TestGetSignaturesWhenSignatureIsNotInANewLine()
        {
            const string testSignatureBlock = "<!-- Base64 Signature";

            Assert.Throws<SignatureException>(() => FeedUtils.GetSignatures((IOpenPgp)new DynamicMock(typeof(IOpenPgp)).MockInstance, Encoding.UTF8.GetBytes(testSignatureBlock)));
        }

        /// <summary>
        /// Ensures that <see cref="FeedUtils.GetSignatures" /> throws an <see cref="SignatureException"/> if there is no new line after the signature.
        /// </summary>
        [Test]
        public void TestGetSignaturesWhenTheSignatureDoesNotEndWithANewLine()
        {
            const string testSignatureBlock = "\n<!-- Base64 Signature\niF4EABEIAAYFAk51LwAACgkQ79Q45QGfCEYUNgD/aA3U9nvk3uCEobfHtbyuHxv1FDp40a5NBMisyzFlPMIA/1MsCv929pheCamLjEMxFwbxhU+S0EbOLzTPqJmoJ7Uw-->";

            Assert.Throws<SignatureException>(() => FeedUtils.GetSignatures((IOpenPgp)new DynamicMock(typeof(IOpenPgp)).MockInstance, Encoding.UTF8.GetBytes(testSignatureBlock)));
        }

        /// <summary>
        /// Ensures that <see cref="FeedUtils.GetSignatures" /> throws an <see cref="SignatureException"/> if there is no empty line between the signature and the signature comment end.
        /// </summary>
        [Test]
        public void TestGetSignaturesWithoutEmptyLineBetweenSignatureAndSignatureCommentEnd()
        {
            const string testSignatureBlock = "\n<!-- Base64 Signature\niF4EABEIAAYFAk51LwAACgkQ79Q45QGfCEYUNgD/aA3U9nvk3uCEobfHtbyuHxv1FDp40a5NBMisyzFlPMIA/1MsCv929pheCamLjEMxFwbxhU+S0EbOLzTPqJmoJ7Uw\nBLA\n-->";

            Assert.Throws<SignatureException>(() => FeedUtils.GetSignatures((IOpenPgp)new DynamicMock(typeof(IOpenPgp)).MockInstance, Encoding.UTF8.GetBytes(testSignatureBlock)));
        }

        /// <summary>
        /// Ensures that <see cref="FeedUtils.GetSignatures" /> throws an <see cref="SignatureException"/> if the signature comment end is too short.
        /// </summary>
        [Test]
        public void TestGetSignaturesWhenSignatureCommentEndIsTooShort()
        {
            const string testSignatureBlock = "\n<!-- Base64 Signature\niF4EABEIAAYFAk51LwAACgkQ79Q45QGfCEYUNgD/aA3U9nvk3uCEobfHtbyuHxv1FDp40a5NBMisyzFlPMIA/1MsCv929pheCamLjEMxFwbxhU+S0EbOLzTPqJmoJ7Uw\n\n\n--";

            Assert.Throws<SignatureException>(() => FeedUtils.GetSignatures((IOpenPgp)new DynamicMock(typeof(IOpenPgp)).MockInstance, Encoding.UTF8.GetBytes(testSignatureBlock)));
        }

        /// <summary>
        /// Ensures that <see cref="FeedUtils.GetSignatures" /> throws an <see cref="SignatureException"/> if the signature comment end has invalid chars.
        /// </summary>
        [Test]
        public void TestGetSignaturesWhenSignatureCommentEndHasInvalidChars()
        {
            const string testSignatureBlock = "\n<!-- Base64 Signature\niF4EABEIAAYFAk51LwAACgkQ79Q45QGfCEYUNgD/aA3U9nvk3uCEobfHtbyuHxv1FDp40a5NBMisyzFlPMIA/1MsCv929pheCamLjEMxFwbxhU+S0EbOLzTPqJmoJ7Uw\n\n\n-_>";

            Assert.Throws<SignatureException>(() => FeedUtils.GetSignatures((IOpenPgp)new DynamicMock(typeof(IOpenPgp)).MockInstance, Encoding.UTF8.GetBytes(testSignatureBlock)));
        }

        /// <summary>
        /// Ensures that <see cref="FeedUtils.GetSignatures" /> throws an <see cref="SignatureException"/> if the signature comment end has invalid chars.
        /// </summary>
        [Test]
        public void TestGetSignaturesWithExtraDataAfterSignatureBlock()
        {
            const string testSignatureBlock = "\n<!-- Base64 Signature\niF4EABEIAAYFAk51LwAACgkQ79Q45QGfCEYUNgD/aA3U9nvk3uCEobfHtbyuHxv1FDp40a5NBMisyzFlPMIA/1MsCv929pheCamLjEMxFwbxhU+S0EbOLzTPqJmoJ7Uw\n\n\n-->C";

            Assert.Throws<SignatureException>(() => FeedUtils.GetSignatures((IOpenPgp)new DynamicMock(typeof(IOpenPgp)).MockInstance, Encoding.UTF8.GetBytes(testSignatureBlock)));
        }

        [Test]
        public void TestGetSignaturesSeperatingFeedAndSignature()
        {
            string testFeed = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<?xml-stylesheet type='text/xsl' href='interface.xsl'?>\n<interface xmlns=\"http://zero-install.sourceforge.net/2004/injector/interface\" />\n<!-- Base64 Signature\niF4EABEIAAYFAk51LwAACgkQ79Q45QGfCEYUNgD/aA3U9nvk3uCEobfHtbyuHxv1FDp40a5NBMisyzFlPMIA/1MsCv929pheCamLjEMxFwbxhU+S0EbOLzTPqJmoJ7Uw\n\n\n-->";

            var feed = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<?xml-stylesheet type='text/xsl' href='interface.xsl'?>\n<interface xmlns=\"http://zero-install.sourceforge.net/2004/injector/interface\" />\n");
            
            var base64SignatureBytes = Encoding.UTF8.GetBytes("iF4EABEIAAYFAk51LwAACgkQ79Q45QGfCEYUNgD/aA3U9nvk3uCEobfHtbyuHxv1FDp40a5NBMisyzFlPMIA/1MsCv929pheCamLjEMxFwbxhU+S0EbOLzTPqJmoJ7Uw");
            var base64Signature = Encoding.UTF8.GetChars(base64SignatureBytes, 0, base64SignatureBytes.Length);
            var signature = Convert.FromBase64CharArray(base64Signature, 0, base64Signature.Length);

            var openPgpMock = new DynamicMock(typeof(IOpenPgp));
            openPgpMock.ExpectAndReturn("Verify", new OpenPgpSignature[0], feed, signature);

            FeedUtils.GetSignatures((IOpenPgp)openPgpMock.MockInstance, Encoding.UTF8.GetBytes(testFeed));
        }
    }
}