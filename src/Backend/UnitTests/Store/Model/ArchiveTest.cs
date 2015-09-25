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
using FluentAssertions;
using NanoByte.Common.Native;
using NUnit.Framework;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="Archive"/>.
    /// </summary>
    [TestFixture]
    public class ArchiveTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="Archive"/>.
        /// </summary>
        internal static Archive CreateTestArchive()
        {
            return new Archive {Href = new Uri("http://0install.de/files/test/test.exe"), MimeType = Archive.MimeTypeZip, Size = 128, StartOffset = 16, Extract = "extract", Destination = "dest"};
        }
        #endregion

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var archive1 = CreateTestArchive();
            var archive2 = archive1.CloneRecipeStep();

            // Ensure data stayed the same
            archive2.Should().Be(archive1, because: "Cloned objects should be equal.");
            archive2.GetHashCode().Should().Be(archive1.GetHashCode(), because: "Cloned objects' hashes should be equal.");
            archive2.Should().NotBeSameAs(archive1, because: "Cloning should not return the same reference.");
        }

        [Test]
        public void TestNormalizeGuessMimeType()
        {
            var archive = new Archive {Href = new Uri("http://0install.de/files/test/test.tar.gz"), Size = 128};
            archive.Normalize(new FeedUri("http://0install.de/feeds/test/"));
            archive.MimeType.Should().Be(Archive.MimeTypeTarGzip, because: "Normalize() should guess missing MIME type");
        }

        [Test]
        public void TestNormalizeLocalPath()
        {
            var archive = new Archive {Href = new Uri("test.zip", UriKind.Relative), MimeType = Archive.MimeTypeZip, Size = 128};
            archive.Normalize(new FeedUri(Path.Combine(WindowsUtils.IsWindows ? @"C:\some\dir" : "/some/dir", "feed.xml")));
            archive.Href.Should().Be(
                new Uri(WindowsUtils.IsWindows ? "file:///C:/some/dir/test.zip" : "file:///some/dir/test.zip"),
                because: "Normalize() should make relative local paths absolute");
        }
    }
}
