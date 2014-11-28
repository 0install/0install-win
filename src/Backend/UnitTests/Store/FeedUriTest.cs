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
using NanoByte.Common.Native;
using NUnit.Framework;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store
{
    /// <summary>
    /// Contains test methods for <see cref="FeedUri"/>.
    /// </summary>
    [TestFixture]
    public class FeedUriTest
    {
        /// <summary>
        /// Ensures the <see cref="FeedUri"/> constructor correctly identify invalid interface URIs.
        /// </summary>
        [Test]
        public void TestValid()
        {
            Assert.DoesNotThrow(() => new FeedUri("http://0install.de"));
            Assert.DoesNotThrow(() => new FeedUri("http://0install.de/"));
            Assert.DoesNotThrow(() => new FeedUri("http://0install.de/feeds/test1.xml"));
            Assert.DoesNotThrow(() => new FeedUri("https://0install.de/feeds/test1.xml"));

            Assert.DoesNotThrow(() => new FeedUri("http://0install.de/feeds/my feed.xml"));
            Assert.DoesNotThrow(() => new FeedUri("http://0install.de/feeds/my%20feed.xml"));

            Assert.DoesNotThrow(() => new FeedUri(WindowsUtils.IsWindows ? @"C:\feed.xml" : "/root/feed.xml"));
        }

        /// <summary>
        /// Ensures the <see cref="FeedUri"/> constructor correctly identify valid interface URIs.
        /// </summary>
        [Test]
        public void TestInvalid()
        {
            var invalidIDs = new[]
            {
                "ftp://host/",
                "foo://host/",
                "relative"
            };

            foreach (var id in invalidIDs)
                Assert.Throws<UriFormatException>(() => new FeedUri(id), "Should reject " + id);
        }

        /// <summary>
        /// Ensures the <see cref="FeedUri.ToString"/> and <see cref="FeedUri.ToStringRfc"/> work correctly.
        /// </summary>
        [Test]
        public void TestToString()
        {
            Assert.AreEqual("http://0install.de/", new FeedUri("http://0install.de").ToStringRfc());
            Assert.AreEqual("http://0install.de/", new FeedUri("http://0install.de/").ToStringRfc());
            Assert.AreEqual("http://0install.de/feeds/test1.xml", new FeedUri("http://0install.de/feeds/test1.xml").ToStringRfc());
            Assert.AreEqual("https://0install.de/feeds/test1.xml", new FeedUri("https://0install.de/feeds/test1.xml").ToStringRfc());

            Assert.AreEqual("http://0install.de/feeds/my feed.xml", new FeedUri("http://0install.de/feeds/my feed.xml").ToString());
            Assert.AreEqual("http://0install.de/feeds/my feed.xml", new FeedUri("http://0install.de/feeds/my%20feed.xml").ToString());
            Assert.AreEqual("http://0install.de/feeds/my%20feed.xml", new FeedUri("http://0install.de/feeds/my feed.xml").ToStringRfc());
            Assert.AreEqual("http://0install.de/feeds/my%20feed.xml", new FeedUri("http://0install.de/feeds/my%20feed.xml").ToStringRfc());

            var absoluteUri = new FeedUri(WindowsUtils.IsWindows ? @"C:\my feed.xml" : "/root/my feed.xml");
            Assert.AreEqual(
                expected: WindowsUtils.IsWindows ? @"C:\my feed.xml" : "/root/my feed.xml",
                actual: absoluteUri.LocalPath);
            Assert.AreEqual(
                expected: WindowsUtils.IsWindows ? @"C:\my feed.xml" : "/root/my feed.xml",
                actual: absoluteUri.ToString());
            Assert.AreEqual(
                expected: WindowsUtils.IsWindows ? "file:///C:/my%20feed.xml" : "file:///root/my%20feed.xml",
                actual: absoluteUri.ToStringRfc());
        }

        [Test]
        public void TestEscape()
        {
            Assert.AreEqual("http%3a%2f%2f0install.de%2ffeeds%2ftest%2ftest1.xml", FeedTest.Test1Uri.Escape());
        }

        [Test]
        public void TestUnescape()
        {
            Assert.AreEqual(
                FeedTest.Test1Uri,
                FeedUri.Unescape("http%3A%2F%2F0install.de%2Ffeeds%2Ftest%2Ftest1.xml"));
        }

        [Test]
        public void TestPrettyEscape()
        {
            Assert.AreEqual(
                // Colon is preserved on POSIX systems but not on other OSes
                UnixUtils.IsUnix ? "http:##0install.de#feeds#test#test1.xml" : "http%3a##0install.de#feeds#test#test1.xml",
                FeedTest.Test1Uri.PrettyEscape());
        }

        [Test]
        public void TestPrettyUnescape()
        {
            Assert.AreEqual(
                FeedTest.Test1Uri,
                // Colon is preserved on POSIX systems but not on other OSes
                FeedUri.PrettyUnescape(UnixUtils.IsUnix ? "http:##0install.de#feeds#test#test1.xml" : "http%3a##0install.de#feeds#test#test1.xml"));
        }
    }
}
