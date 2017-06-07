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
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NanoByte.Common.Native;
using Xunit;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store
{
    /// <summary>
    /// Contains test methods for <see cref="FeedUri"/>.
    /// </summary>
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    public class FeedUriTest
    {
        /// <summary>
        /// Ensures the <see cref="FeedUri"/> constructor correctly identify invalid interface URIs.
        /// </summary>
        [Fact]
        public void TestValid()
        {
            new FeedUri("http://0install.de");
            new FeedUri("http://0install.de/");
            new FeedUri("http://0install.de/feeds/test1.xml");
            new FeedUri("https://0install.de/feeds/test1.xml");

            new FeedUri("http://0install.de/feeds/my feed.xml");
            new FeedUri("http://0install.de/feeds/my%20feed.xml");

            new FeedUri(WindowsUtils.IsWindows ? @"C:\my feed.xml" : "/root/my feed.xml");
            new FeedUri(WindowsUtils.IsWindows ? "file:///C:/my%20feed.xml" : "file:///root/my%20feed.xml");
            if (WindowsUtils.IsWindows)
            {
                new FeedUri(@"\\SERVER\C$\my feed.xml");
                new FeedUri("file://SERVER/C$/my%20feed.xml");
            }
        }

        /// <summary>
        /// Ensures the <see cref="FeedUri"/> constructor correctly identify valid interface URIs.
        /// </summary>
        [Theory, InlineData("ftp://host/"), InlineData("foo://host/"), InlineData("relative")]
        public void TestInvalid(string id)
            => Assert.Throws<UriFormatException>(() => new FeedUri(id));

        /// <summary>
        /// Ensures the <see cref="FeedUri.ToString"/> and <see cref="FeedUri.ToStringRfc"/> work correctly.
        /// </summary>
        [Fact]
        public void TestToString()
        {
            new FeedUri("http://0install.de").ToStringRfc().Should().Be("http://0install.de/");
            new FeedUri("http://0install.de/").ToStringRfc().Should().Be("http://0install.de/");
            new FeedUri("http://0install.de/feeds/test1.xml").ToStringRfc().Should().Be("http://0install.de/feeds/test1.xml");
            new FeedUri("https://0install.de/feeds/test1.xml").ToStringRfc().Should().Be("https://0install.de/feeds/test1.xml");

            new FeedUri("http://0install.de/feeds/my feed.xml").ToString().Should().Be("http://0install.de/feeds/my feed.xml");
            new FeedUri("http://0install.de/feeds/my%20feed.xml").ToString().Should().Be("http://0install.de/feeds/my feed.xml");
            new FeedUri("http://0install.de/feeds/my feed.xml").ToStringRfc().Should().Be("http://0install.de/feeds/my%20feed.xml");
            new FeedUri("http://0install.de/feeds/my%20feed.xml").ToStringRfc().Should().Be("http://0install.de/feeds/my%20feed.xml");

            var absoluteUri = new FeedUri(WindowsUtils.IsWindows ? @"C:\my feed.xml" : "/root/my feed.xml");
            absoluteUri.LocalPath.Should().Be(
                WindowsUtils.IsWindows ? @"C:\my feed.xml" : "/root/my feed.xml");
            absoluteUri.ToString().Should().Be(
                WindowsUtils.IsWindows ? @"C:\my feed.xml" : "/root/my feed.xml");
            absoluteUri.ToStringRfc().Should().Be(
                WindowsUtils.IsWindows ? @"C:\my feed.xml" : "/root/my feed.xml");

            absoluteUri = new FeedUri(WindowsUtils.IsWindows ? "file:///C:/my%20feed.xml" : "file:///root/my%20feed.xml");
            absoluteUri.LocalPath.Should().Be(
                WindowsUtils.IsWindows ? @"C:\my feed.xml" : "/root/my feed.xml");
            absoluteUri.ToString().Should().Be(
                WindowsUtils.IsWindows ? @"C:\my feed.xml" : "/root/my feed.xml");
            absoluteUri.ToStringRfc().Should().Be(
                WindowsUtils.IsWindows ? @"C:\my feed.xml" : "/root/my feed.xml");

            if (WindowsUtils.IsWindows)
            {
                absoluteUri = new FeedUri(@"\\SERVER\C$\my feed.xml");
                absoluteUri.ToString().Should().Be(
                    @"\\server\C$\my feed.xml");
                absoluteUri.ToStringRfc().Should().Be(
                    @"\\server\C$\my feed.xml");

                absoluteUri = new FeedUri("file://SERVER/C$/my%20feed.xml");
                absoluteUri.ToString().Should().Be(
                    @"\\server\C$\my feed.xml");
                absoluteUri.ToStringRfc().Should().Be(
                    @"\\server\C$\my feed.xml");
            }
        }

        [Fact]
        public void TestEscape()
            => FeedTest.Test1Uri.Escape().Should().Be("http%3a%2f%2f0install.de%2ffeeds%2ftest%2ftest1.xml");

        [Fact]
        public void TestUnescape()
            => FeedUri.Unescape("http%3A%2F%2F0install.de%2Ffeeds%2Ftest%2Ftest1.xml").Should().Be(FeedTest.Test1Uri);

        [Fact]
        public void TestPrettyEscape()
            => FeedTest.Test1Uri.PrettyEscape().Should().Be(
                // Colon is preserved on POSIX systems but not on other OSes
                UnixUtils.IsUnix ? "http:##0install.de#feeds#test#test1.xml" : "http%3a##0install.de#feeds#test#test1.xml");

        [Fact]
        public void TestPrettyUnescape()
            => FeedUri.PrettyUnescape(UnixUtils.IsUnix ? "http:##0install.de#feeds#test#test1.xml" : "http%3a##0install.de#feeds#test#test1.xml").Should().Be(FeedTest.Test1Uri);

        [Fact]
        public void TestEscapeComponent()
        {
            new FeedUri("http://example.com/foo/bar.xml").EscapeComponent().Should().Equal("http", "example.com", "foo__bar.xml");
            new FeedUri("http://example.com/").EscapeComponent().Should().Equal("http", "example.com", "");
            new FeedUri(WindowsUtils.IsWindows ? @"C:\my feed.xml" : "/root/my feed.xml").EscapeComponent().Should().Equal("file", WindowsUtils.IsWindows ? "C_3a___my_20_feed.xml" : "root__my_20_feed.xml");
            if (WindowsUtils.IsWindows)
                new FeedUri(@"\\SERVER\C$\my feed.xml").EscapeComponent().Should().Equal("file", "____server__C_24___my_20_feed.xml");
        }

        [Fact]
        public void TestPrefixes()
        {
            var fakeUri = new FeedUri("fake:http://example.com/");
            fakeUri.IsFake.Should().BeTrue();
            fakeUri.ToString().Should().Be("fake:http://example.com/");
            fakeUri.ToStringRfc().Should().Be("fake:http://example.com/");

            var fromDistributionUri = new FeedUri("distribution:http://example.com/");
            fromDistributionUri.IsFromDistribution.Should().BeTrue();
            fromDistributionUri.ToString().Should().Be("distribution:http://example.com/");
            fromDistributionUri.ToStringRfc().Should().Be("distribution:http://example.com/");
        }
    }
}
