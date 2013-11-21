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
using ZeroInstall.Backend;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;

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
            return new Selection(Resolver);
        }

        [Test(Description = "Ensures all options are parsed and handled correctly.")]
        public virtual void TestNormal()
        {
            var requirements = RequirementsTest.CreateTestRequirements();
            var selections = SelectionsManagerTest.CreateTestSelections();

            bool stale;
            SolverMock.Setup(x => x.Solve(requirements, out stale)).Returns(selections).Verifiable();
            AssertParseExecuteResult(selections.ToXmlString(), 0,
                "--xml", "http://0install.de/feeds/test/test1.xml", "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0");
            AssertSelections(selections);
        }

        [Test(Description = "Ensures local Selections XMLs are correctly detected and parsed.")]
        public virtual void TestImportSelections()
        {
            var selections = SelectionsManagerTest.CreateTestSelections();
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.SaveXml(tempFile);
                AssertParseExecuteResult(selections.ToXmlString(), 0, "--xml", tempFile);
                AssertSelections(selections);
            }
        }

        [Test(Description = "Ensures calling with too many arguments raises an exception.")]
        public void TestTooManyArgs()
        {
            Assert.Throws<OptionException>(
                () => Command.Parse(new[] {"http://0install.de/feeds/test/test1.xml", "arg1"}),
                "Should reject more than one argument");
        }

        /// <summary>
        /// Checks that <see cref="IBackendHandler.ShowSelections"/> was called with a specific set of <see cref="Selections"/>.
        /// </summary>
        protected void AssertSelections(Selections selections)
        {
            Assert.AreEqual(selections.InterfaceID, Selections.InterfaceID);
            Assert.AreEqual(selections.Command, Selections.Command);
            CollectionAssert.AreEqual(selections.Implementations, Selections.Implementations);
        }
    }
}
