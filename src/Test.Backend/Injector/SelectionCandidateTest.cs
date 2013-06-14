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

using NUnit.Framework;
using ZeroInstall.Model;
using ZeroInstall.Model.Preferences;
using ZeroInstall.Model.Selection;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Contains test methods for <see cref="SelectionCandidate"/>.
    /// </summary>
    [TestFixture]
    public class SelectionCandidateTest
    {
        [Test]
        public void TestIsSuitable()
        {
            var implementation = ImplementationTest.CreateTestImplementation();
            Assert.IsTrue(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences(), new Requirements()).IsSuitable);
        }

        [Test]
        public void TestIsSuitableArchitecture()
        {
            var implementation = ImplementationTest.CreateTestImplementation();
            Assert.IsTrue(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences(), new Requirements {Architecture = implementation.Architecture}).IsSuitable);
            Assert.IsFalse(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences(), new Requirements {Architecture = new Architecture(OS.FreeBSD, Cpu.PPC)}).IsSuitable);
        }

        [Test]
        public void TestIsSuitableVersionMismatch()
        {
            var implementation = ImplementationTest.CreateTestImplementation();
            Assert.IsTrue(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences(), new Requirements {Versions = new VersionRange("..!1.1")}).IsSuitable);
            Assert.IsFalse(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences(), new Requirements {Versions = new VersionRange("..!1.0")}).IsSuitable);
        }

        [Test]
        public void TestIsSuitableBuggyInsecure()
        {
            var implementation = ImplementationTest.CreateTestImplementation();
            Assert.IsFalse(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences {UserStability = Stability.Buggy}, new Requirements()).IsSuitable);
            Assert.IsFalse(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences {UserStability = Stability.Insecure}, new Requirements()).IsSuitable);
        }
    }
}
