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

        [Test]
        public void TestGetSignatures()
        {
            using (var stream = File.OpenRead(@"G:\Documents\Internet\Anthea\srv\www\0install\feeds\Blender.xml"))
                FeedUtils.GetSignatures(OpenPgpProvider.Default, stream);
        }
    }
}