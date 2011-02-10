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

using NUnit.Framework;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Contains test methods for <see cref="Requirements"/>.
    /// </summary>
    [TestFixture]
    public class RequirementsTest
    {
        /// <summary>
        /// Ensures that setting <see cref="Requirements.InterfaceID"/> produces the correct exceptions.
        /// </summary>
        [Test]
        public void TestInterfaceID()
        {
            var requirements = new Requirements();
            Assert.Throws<InvalidInterfaceIDException>(() => requirements.InterfaceID = "http://0install.de", "Should not accept URIs without slash after hostname");
            Assert.Throws<InvalidInterfaceIDException>(() => requirements.InterfaceID = "ftp://0install.de/feeds/test.xml", "Should not accept protocols other than HTTP(S)");
            Assert.Throws<InvalidInterfaceIDException>(() => requirements.InterfaceID = "test.xml", "Should not accept relative paths");

            Assert.DoesNotThrow(() => requirements.InterfaceID = "http://0install.de/feeds/test.xml", "Should accept HTTP URIs");
            Assert.DoesNotThrow(() => requirements.InterfaceID = "https://0install.de/feeds/test.xml", "Should accept HTTPS URIs");
            Assert.DoesNotThrow(() => requirements.InterfaceID = "/feeds/test.xml", "Should absolute paths");
        }
    }
}
