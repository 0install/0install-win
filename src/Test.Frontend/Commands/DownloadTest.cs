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
using NUnit.Framework;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Contains code for testing <see cref="Download"/>.
    /// </summary>
    [TestFixture]
    public class DownloadTest : SelectionTest
    {
        /// <inheritdoc/>
        protected override CommandBase GetCommand()
        {
            return new Download(Handler, Policy, Solver);
        }

        [Test(Description = "Ensures all options are parsed and handled correctly.")]
        public override void TestNormal()
        {
            var requirements = RequirementsTest.CreateTestRequirements();
            var selections = SelectionsTest.CreateTestSelections();
            var testFeed1 = FeedTest.CreateTestFeed();
            var testFeed2 = FeedTest.CreateTestFeed();
            var testImplementation1 = testFeed1.GetImplementation(selections.Implementations[0].ID);
            var testImplementation2 = testFeed1.GetImplementation(selections.Implementations[1].ID);

            var refreshPolicy = Policy.ClonePolicy();
            refreshPolicy.FeedManager.Refresh = true;

            var args = new[] {"http://0install.de/feeds/test/test1.xml", "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0"};
            Command.Parse(args);

            SolverMock.ExpectAndReturn("Solve", selections, requirements, Policy, Handler, false);
            SolverMock.ExpectAndReturn("Solve", selections, requirements, refreshPolicy, Handler, false);
            CacheMock.ExpectAndReturn("GetFeed", testFeed1, new Uri("http://0install.de/feeds/test/sub1.xml"));
            CacheMock.ExpectAndReturn("GetFeed", testFeed2, new Uri("http://0install.de/feeds/test/sub2.xml"));
            FetcherMock.Expect("RunSync", new FetchRequest(new[] {testImplementation1, testImplementation2}), Handler);

            Assert.AreEqual(0, Command.Execute());
        }
    }
}
