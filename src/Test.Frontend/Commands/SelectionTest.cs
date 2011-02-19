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

using NDesk.Options;
using NUnit.Framework;
using NUnit.Mocks;
using ZeroInstall.Injector.Solver;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Contains code for testing <see cref="Selection"/>.
    /// </summary>
    [TestFixture]
    public class SelectionTest : CommandBaseTest
    {
        protected DynamicMock SolverMock;
        protected ISolver Solver;

        /// <inheritdoc/>
        protected override CommandBase GetCommand()
        {
            return new Selection(Handler, Policy, Solver);
        }

        [SetUp]
        public override void SetUp()
        {
            SolverMock = new DynamicMock("MockSolver", typeof(ISolver));
            Solver = (ISolver)SolverMock.MockInstance;
            base.SetUp();
        }

        [TearDown]
        public override void TearDown()
        {
            SolverMock.Verify();
            base.TearDown();
        }

        /// <summary>
        /// Ensures all options are parsed and handled correctly.
        /// </summary>
        [Test]
        public virtual void TestNormal()
        {
            var requirements = RequirementsTest.CreateTestRequirements();
            var selections = SelectionsTest.CreateTestSelections();

            SolverMock.ExpectAndReturn("Solve", selections, requirements, Policy, Handler);
            var args = new[] { "--xml", "http://0install.de/feeds/test/test1.xml", "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0"};
            AssertParseExecuteResult(args, selections.WriteToString(), 0);
        }

        /// <summary>
        /// Ensures calling with no arguments raises an exception.
        /// </summary>
        [Test]
        public void TestNoArgs()
        {
            Assert.Throws<InvalidInterfaceIDException>(() => Command.Parse(new string[0]), "Should reject empty argument list");
        }

        /// <summary>
        /// Ensures calling with too many arguments raises an exception.
        /// </summary>
        [Test]
        public void TestTooManyArgs()
        {
            Command.Parse(new[] { "http://0install.de/feeds/test/test1.xml", "arg1" });
            Assert.Throws<OptionException>(() => Command.Execute(), "Should reject more than one argument");
        }
    }
}
