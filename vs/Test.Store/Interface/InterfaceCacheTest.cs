/*
 * Copyright 2010 Bastian Eicher
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

using System.IO;
using Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Store.Feed
{
    /// <summary>
    /// Contains test methods for <see cref="InterfaceCache"/>.
    /// </summary>
    [TestFixture]
    public class InterfaceCacheTest
    {
        /// <summary>
        /// Ensures <see cref="InterfaceCache.GetFeed"/> correctly gets an interface from the cache or the network.
        /// </summary>
        // Test deactivated because it performs network IO
        //[Test]
        public void TestGetFeed()
        {
            using (var temp = new TemporaryDirectory())
            {
                File.WriteAllText(Path.Combine(temp.Path, "invalid"), "");
                File.WriteAllText(Path.Combine(temp.Path, "http%3a%2f%2f0install.de%2ftest%2finterface.xml"), "");

                var feed = new InterfaceCache(temp.Path).GetFeed("http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml");
                Assert.AreEqual(feed.Uri, "http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml");
            }
        }

        /// <summary>
        /// Ensures that <see cref="InterfaceCache.GetCached"/> correctly identifies cached feed XMLs.
        /// </summary>
        [Test]
        public void TestGetCached()
        {
            using (var temp = new TemporaryDirectory())
            {
                File.WriteAllText(Path.Combine(temp.Path, "invalid"), "");
                File.WriteAllText(Path.Combine(temp.Path, "http%3a%2f%2f0install.de%2ftest%2finterface.xml"), "");

                var cached = new InterfaceCache(temp.Path).GetCached();
                CollectionAssert.AreEqual(new[] {"http://0install.de/test/interface.xml"}, cached);
            }
        }
    }
}
