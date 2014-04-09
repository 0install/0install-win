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
using NanoByte.Common.Utils;
using NUnit.Framework;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="FeedElementUtils"/>.
    /// </summary>
    [TestFixture]
    public class FeedElementUtilsTest
    {
        [Test]
        public void TestFilterMismatch()
        {
            Assert.IsFalse(FeedElement.FilterMismatch(new EntryPoint()));
            Assert.IsFalse(FeedElement.FilterMismatch(new EntryPoint {IfZeroInstallVersion = new VersionRange("0..")}));
            Assert.IsTrue(FeedElement.FilterMismatch(new EntryPoint {IfZeroInstallVersion = new VersionRange("..!0")}));
        }

        [Test]
        public void TestGetAbsolutePath()
        {
            string result = FeedElementUtils.GetAbsolutePath("subdir/file", WindowsUtils.IsWindows ? @"C:\local\feed.xml" : "/local/feed.xml");
            Assert.IsTrue(Path.IsPathRooted(result));
            Assert.AreEqual(WindowsUtils.IsWindows ? @"C:\local\subdir\file" : "/local/subdir/file", result);
        }

        [Test]
        public void TestGetAbsolutePathException()
        {
            Assert.Throws<IOException>(() => FeedElementUtils.GetAbsolutePath("subdir/file", null));
            Assert.Throws<IOException>(() => FeedElementUtils.GetAbsolutePath("subdir/file", Path.Combine("relative", "path")));
        }

        [Test]
        public void TestGetAbsoluteHref()
        {
            var result = FeedElementUtils.GetAbsoluteHref(new Uri("subdir/file", UriKind.Relative), WindowsUtils.IsWindows ? @"C:\local\feed.xml" : "/local/feed.xml");
            Assert.IsTrue(result.IsAbsoluteUri);
            Assert.AreEqual(WindowsUtils.IsWindows ? new Uri("file:///C:/local/subdir/file") : new Uri("file:///local/subdir/file"), result);
        }

        [Test]
        public void TestGetAbsoluteHrefException()
        {
            Assert.Throws<IOException>(() => FeedElementUtils.GetAbsoluteHref(new Uri("subdir/file", UriKind.Relative), null));
            Assert.Throws<IOException>(() => FeedElementUtils.GetAbsoluteHref(new Uri("subdir/file", UriKind.Relative), Path.Combine("relative", "path")));
        }
    }
}
