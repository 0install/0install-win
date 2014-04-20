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

using NUnit.Framework;
using ZeroInstall.Store.Model.Preferences;

namespace ZeroInstall.Store.Model.Selection
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
            Assert.IsTrue(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", new FeedPreferences(),
                implementation, new Requirements {InterfaceID = "http://0install.de/feeds/test/test1.xml", Command = Command.NameRun}).IsSuitable);
        }

        [Test]
        public void TestIsSuitableArchitecture()
        {
            var implementation = ImplementationTest.CreateTestImplementation();
            Assert.IsTrue(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", new FeedPreferences(),
                implementation, new Requirements {InterfaceID = "http://0install.de/feeds/test/test1.xml", Command = Command.NameRun, Architecture = implementation.Architecture}).IsSuitable);
            Assert.IsFalse(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", new FeedPreferences(),
                implementation, new Requirements {InterfaceID = "http://0install.de/feeds/test/test1.xml", Command = Command.NameRun, Architecture = new Architecture(OS.FreeBsd, Cpu.Ppc)}).IsSuitable);
        }

        [Test]
        public void TestIsSuitableVersionMismatch()
        {
            var implementation = ImplementationTest.CreateTestImplementation();
            Assert.IsTrue(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", new FeedPreferences(), implementation,
                new Requirements
                {
                    InterfaceID = "http://0install.de/feeds/test/test1.xml",
                    Command = Command.NameRun,
                    ExtraRestrictions = {{"http://0install.de/feeds/test/test1.xml", new VersionRange("..!1.1")}}
                }).IsSuitable);
            Assert.IsFalse(new SelectionCandidate("http://0install.de/feeds/test/test1.xml", new FeedPreferences(), implementation,
                new Requirements
                {
                    InterfaceID = "http://0install.de/feeds/test/test1.xml",
                    Command = Command.NameRun,
                    ExtraRestrictions = {{"http://0install.de/feeds/test/test1.xml", new VersionRange("..!1.0")}}
                }).IsSuitable);
        }

        [Test]
        public void TestIsSuitableBuggyInsecure()
        {
            var implementation = ImplementationTest.CreateTestImplementation();
            Assert.IsFalse(new SelectionCandidate("http://0install.de/feeds/test/test1.xml",
                new FeedPreferences {Implementations = {new ImplementationPreferences {ID = implementation.ID, UserStability = Stability.Buggy}}},
                implementation, new Requirements {InterfaceID = "http://0install.de/feeds/test/test1.xml", Command = Command.NameRun}).IsSuitable);
            Assert.IsFalse(new SelectionCandidate("http://0install.de/feeds/test/test1.xml",
                new FeedPreferences {Implementations = {new ImplementationPreferences {ID = implementation.ID, UserStability = Stability.Insecure}}},
                implementation, new Requirements {InterfaceID = "http://0install.de/feeds/test/test1.xml", Command = Command.NameRun}).IsSuitable);
        }
    }
}
