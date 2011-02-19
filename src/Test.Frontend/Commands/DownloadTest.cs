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

        /// <inheritdoc/>
        [Test]
        public override void TestNormal()
        {
            var requirements = RequirementsTest.CreateTestRequirements();
            var selections = SelectionsTest.CreateTestSelections();

            var args = new[] {"http://0install.de/feeds/test/test1.xml", "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0"};
            Command.Parse(args);

            SolverMock.ExpectAndReturn("Solve", selections, requirements, Policy, Handler);
            CacheMock.ExpectAndReturn("GetFeed", FeedTest.CreateTestFeed(), new Uri("http://0install.de/feeds/test/sub1.xml"));
            CacheMock.ExpectAndReturn("GetFeed", FeedTest.CreateTestFeed(), new Uri("http://0install.de/feeds/test/sub2.xml"));
            FetcherMock.Expect("RunSync");
            Assert.AreEqual(0, Command.Execute());
        }
    }
}
