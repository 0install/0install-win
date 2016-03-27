/*
 * Copyright 2010-2016 Bastian Eicher
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

using FluentAssertions;
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
            new SelectionCandidate(FeedTest.Test1Uri, new FeedPreferences(), implementation, new Requirements(FeedTest.Test1Uri, Command.NameRun))
                .IsSuitable.Should().BeTrue();
        }

        [Test]
        public void TestIsSuitableArchitecture()
        {
            var implementation = ImplementationTest.CreateTestImplementation();
            new SelectionCandidate(FeedTest.Test1Uri, new FeedPreferences(), implementation, new Requirements(FeedTest.Test1Uri, Command.NameRun, implementation.Architecture))
                .IsSuitable.Should().BeTrue();
            new SelectionCandidate(FeedTest.Test1Uri, new FeedPreferences(), implementation, new Requirements(FeedTest.Test1Uri, Command.NameRun, new Architecture(OS.FreeBsd, Cpu.Ppc)))
                .IsSuitable.Should().BeFalse();
        }

        [Test]
        public void TestIsSuitableVersionMismatch()
        {
            var implementation = ImplementationTest.CreateTestImplementation();
            new SelectionCandidate(FeedTest.Test1Uri, new FeedPreferences(), implementation, new Requirements(FeedTest.Test1Uri, Command.NameRun)
            {
                ExtraRestrictions = {{FeedTest.Test1Uri, new VersionRange("..!1.1")}}
            }).IsSuitable.Should().BeTrue();
            new SelectionCandidate(FeedTest.Test1Uri, new FeedPreferences(), implementation, new Requirements(FeedTest.Test1Uri, Command.NameRun)
            {
                ExtraRestrictions = {{FeedTest.Test1Uri, new VersionRange("..!1.0")}}
            }).IsSuitable.Should().BeFalse();
        }

        [Test]
        public void TestIsSuitableBuggyInsecure()
        {
            var implementation = ImplementationTest.CreateTestImplementation();
            new SelectionCandidate(FeedTest.Test1Uri, new FeedPreferences {Implementations = {new ImplementationPreferences {ID = implementation.ID, UserStability = Stability.Buggy}}}, implementation, new Requirements(FeedTest.Test1Uri, Command.NameRun))
                .IsSuitable.Should().BeFalse();
            new SelectionCandidate(FeedTest.Test1Uri, new FeedPreferences {Implementations = {new ImplementationPreferences {ID = implementation.ID, UserStability = Stability.Insecure}}}, implementation, new Requirements(FeedTest.Test1Uri, Command.NameRun)).
                IsSuitable.Should().BeFalse();
        }
    }
}
