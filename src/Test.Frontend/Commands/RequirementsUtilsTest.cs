/*
 * Copyright 2010-2013 Bastian Eicher
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

using NDesk.Options;
using NUnit.Framework;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Contains code for testing <see cref="RequirementsExtensions"/>.
    /// </summary>
    [TestFixture]
    public class RequirementsUtilsTest
    {
        /// <summary>
        /// Ensures command-line parsing works correctly.
        /// </summary>
        [Test]
        public void TestFromCommandLine()
        {
            var requirements = new Requirements();
            var options = new OptionSet();

            requirements.FromCommandLine(options);
            options.Parse(new[] {"--command=command", "--os=Windows", "--cpu=i586", "--version=1.0..!2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0"});
            requirements.InterfaceID = "http://0install.de/feeds/test/test1.xml";

            Assert.AreEqual(RequirementsTest.CreateTestRequirements(), requirements);
        }

        /// <summary>
        /// Ensures command-line parsing works correctly with legacy options.
        /// </summary>
        [Test]
        public void TestFromCommandLineLegacy()
        {
            var requirements = new Requirements();
            var options = new OptionSet();

            requirements.FromCommandLine(options);
            options.Parse(new[] {"--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0"});
            requirements.InterfaceID = "http://0install.de/feeds/test/test1.xml";

            Assert.AreEqual(RequirementsTest.CreateTestRequirements(), requirements);
        }
    }
}
