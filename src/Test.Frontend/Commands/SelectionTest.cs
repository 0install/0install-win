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

using Common.Storage;
using NDesk.Options;
using NUnit.Framework;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Contains integration tests for <see cref="Selection"/>.
    /// </summary>
    [TestFixture]
    public class SelectionTest : FrontendCommandTest
    {
        /// <inheritdoc/>
        protected override FrontendCommand GetCommand()
        {
            return new Selection(Policy);
        }

        [Test(Description = "Ensures all options are parsed and handled correctly.")]
        public virtual void TestNormal()
        {
            var requirements = RequirementsTest.CreateTestRequirements();
            var selections = SelectionsUtilsTest.CreateTestSelections();

            bool stale;
            SolverMock.Setup(x => x.Solve(requirements, Policy, out stale)).Returns(selections).Verifiable();
            AssertParseExecuteResult(selections, selections.ToXmlString(), 0,
                "--xml", "http://0install.de/feeds/test/test1.xml", "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0");
        }

        [Test(Description = "Ensures local Selections XMLs are correctly detected and parsed.")]
        public virtual void TestImportSelections()
        {
            var selections = SelectionsUtilsTest.CreateTestSelections();
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.SaveXml(tempFile);
                AssertParseExecuteResult(selections, selections.ToXmlString(), 0,
                    "--xml", tempFile);
            }
        }

        [Test(Description = "Ensures calling with no arguments raises an exception.")]
        public void TestNoArgs()
        {
            Assert.Throws<InvalidInterfaceIDException>(() => Command.Parse(new string[0]), "Should reject empty argument list");
        }

        [Test(Description = "Ensures calling with too many arguments raises an exception.")]
        public void TestTooManyArgs()
        {
            Command.Parse(new[] {"http://0install.de/feeds/test/test1.xml", "arg1"});
            Assert.Throws<OptionException>(() => Command.Execute(), "Should reject more than one argument");
        }
    }
}
