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

using System;
using Common.Storage;
using NUnit.Framework;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ImplementationSelection = ZeroInstall.Injector.Solver.ImplementationSelection;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Contains code for testing <see cref="Update"/>.
    /// </summary>
    [TestFixture]
    public class UpdateTest : SelectionTest
    {
        /// <inheritdoc/>
        protected override CommandBase GetCommand()
        {
            return new Update(Policy);
        }

        [Test(Description = "Ensures local Selections XMLs are correctly detected and parsed.")]
        public override void TestNormal()
        {
            var requirements = RequirementsTest.CreateTestRequirements();
            var selectionsOld = SelectionsTest.CreateTestSelections();
            var selectionsNew = SelectionsTest.CreateTestSelections();
            selectionsNew.Implementations[1].Version = new ImplementationVersion("2.0");
            selectionsNew.Implementations.Add(new ImplementationSelection {InterfaceID = "http://0install.de/feeds/test/sub3.xml", Version = new ImplementationVersion("0.1")});

            var noRefreshPolicy = Policy.ClonePolicy();
            noRefreshPolicy.FeedManager.Refresh = false;

            Policy.FeedManager.Refresh = true;

            SolverMock.ExpectAndReturn("Solve", selectionsOld, requirements, noRefreshPolicy, false); // No refresh Solve()
            SolverMock.ExpectAndReturn("Solve", selectionsNew, requirements, Policy, false); // Refresh Solve()
            CacheMock.ExpectAndReturn("GetFeed", FeedTest.CreateTestFeed(), new Uri("http://0install.de/feeds/test/sub1.xml")); // Get feeds from cache to determine uncached implementations
            CacheMock.ExpectAndReturn("GetFeed", FeedTest.CreateTestFeed(), new Uri("http://0install.de/feeds/test/sub2.xml"));
            CacheMock.ExpectAndReturn("GetFeed", FeedTest.CreateTestFeed(), new Uri("http://0install.de/feeds/test/sub3.xml"));

            // Download uncached implementations
            FetcherMock.Expect("Start");
            FetcherMock.Expect("Join");

            var args = new[] {"http://0install.de/feeds/test/test1.xml", "--command=command name", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0"};
            AssertParseExecuteResult(args, selectionsNew, "http://0install.de/feeds/test/test2.xml: 1.0 -> 2.0" + Environment.NewLine + "http://0install.de/feeds/test/sub3.xml: new -> 0.1", 0);
        }

        [Test(Description = "Ensures local Selections XMLs are rejected.")]
        public override void TestImportSelections()
        {
            var selections = SelectionsTest.CreateTestSelections();
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.Save(tempFile.Path);
                Command.Parse(new[] {tempFile.Path});
                Assert.Throws<NotSupportedException>(() => Command.Execute());
            }
        }
    }
}
