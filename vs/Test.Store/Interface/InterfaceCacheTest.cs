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

using NUnit.Framework;

namespace ZeroInstall.Store.Interface
{
    /// <summary>
    /// Contains test methods for <see cref="InterfaceCache"/>.
    /// </summary>
    [TestFixture]
    public class InterfaceCacheTest
    {
        /// <summary>
        /// Ensures <see cref="InterfaceCache.GetInterface"/> correctly gets an interface from the cache or the network.
        /// </summary>
        // Test deactivated because it performs network IO and launches an external application
        //[Test]
        public void TestGetInterface()
        {
            var cache = new InterfaceCache();

            var interfaceInfo = cache.GetInterface("http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml");
            Assert.AreEqual(interfaceInfo.Uri, "http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml");
        }
    }
}
