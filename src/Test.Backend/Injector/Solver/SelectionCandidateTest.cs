/*
 * Copyright 2010-2012 Bastian Eicher
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
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Model;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Contains test methods for <see cref="SelectionCandidate"/>.
    /// </summary>
    [TestFixture]
    public class SelectionCandidateTest
    {
        [Test]
        public void TestIsUsable()
        {
            var implementation = ImplementationTest.CreateTestImplementation();
            Assert.IsTrue(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences(), new Requirements()).IsUsable);
        }

        [Test]
        public void TestIsUsableArchitecture()
        {
            var implementation = ImplementationTest.CreateTestImplementation();
            Assert.IsTrue(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences(), new Requirements {Architecture = implementation.Architecture}).IsUsable);
            Assert.IsFalse(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences(), new Requirements {Architecture = new Architecture(OS.Solaris, Cpu.Ppc)}).IsUsable);
        }

        [Test]
        public void TestIsUsableVersionTooOld()
        {
            var implementation = ImplementationTest.CreateTestImplementation();
            Assert.IsTrue(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences(), new Requirements {NotBeforeVersion = new ImplementationVersion("1.0")}).IsUsable);
            Assert.IsFalse(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences(), new Requirements {NotBeforeVersion = new ImplementationVersion("1.1")}).IsUsable);
        }

        [Test]
        public void TestIsUsableVersionTooNew()
        {
            var implementation = ImplementationTest.CreateTestImplementation();
            Assert.IsTrue(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences(), new Requirements {BeforeVersion = new ImplementationVersion("1.1")}).IsUsable);
            Assert.IsFalse(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences(), new Requirements {BeforeVersion = new ImplementationVersion("1.0")}).IsUsable);
        }

        [Test]
        public void TestIsUsableBuggyInsecure()
        {
            var implementation = ImplementationTest.CreateTestImplementation();
            Assert.IsFalse(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences {UserStability = Stability.Buggy}, new Requirements()).IsUsable);
            Assert.IsFalse(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", implementation,
                new ImplementationPreferences {UserStability = Stability.Insecure}, new Requirements()).IsUsable);
        }
    }
}
