/*
 * Copyright 2010 Simon E. Silva Lauinger
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
using NUnit.Framework;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms
{

    /// <summary>
    /// Contains test methods for <see cref="ControlHelpers"/>.
    /// </summary>
    [TestFixture]
    class ControlHelpersTest
    {
        /// <summary>
        /// Tests all methods IsValidFeedUrl(...) with a set of valid an invalid feed URLs.
        /// </summary>
        [Test]
        public void TestIsValidFeedUrl()
        {
            // Because "public static bool IsValidFeedUrl(string url, out Uri uri)" is used directly by "public static bool IsValidFeedUrl(string url)"
            // it is sufficient only to test this methode.

            // Test invalid URLs
            //TODO find more invalid URLs
            var invalidUrls = new[]
                              {
                                  @"foo://",
                                  @"ftp://",
                                  @"www://",
                                  @"http://.de/",
                                  @"http://abc..de/",
                                  @"http://abc§.de/",
                                  @"ggo;\\"
                              };
            foreach (var url in invalidUrls)
                Assert.IsFalse(ControlHelpers.IsValidFeedUrl(url));

            // Test valid URLs
            //TODO find more valid URLs
            var validUrls = new[]
                            {
                                @"http://0install.de/",
                                @"https://0install.de/"
                            };
            foreach (var url in validUrls)
                Assert.IsTrue(ControlHelpers.IsValidFeedUrl(url));
        }

        /// <summary>
        /// Test methode ControlHelpers.IsEmpty(Archive toCheck).
        /// </summary>
        [Test]
        public void TestIsEmpty()
        {
            // Test with empty archive
            Assert.IsTrue(ControlHelpers.IsEmpty(new Archive()));

            // Test with archives with setted values
            Archive[] toTest = {
                                         new Archive() { Extract = "/home/0install" },
                                         new Archive() { Location = new Uri(@"http://0install.de") },
                                         new Archive() { MimeType = @"image/png" },
                                         new Archive() { Size = 1024 },
                                         new Archive() { StartOffset = 512 }
                                     };
            foreach (var archive in toTest)
                Assert.IsFalse(ControlHelpers.IsEmpty(archive));
        }
    }
}
