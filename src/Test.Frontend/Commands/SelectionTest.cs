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

using Common.Storage;
using Moq;
using NDesk.Options;
using NUnit.Framework;
using ZeroInstall.Backend;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;

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
            return new Selection(HandlerMock.Object);
        }

        [Test(Description = "Ensures all options are parsed and handled correctly.")]
        public virtual void TestNormal()
        {
            var requirements = RequirementsTest.CreateTestRequirements();
            var selections = SelectionsManagerTest.CreateTestSelections();

            SolverMock.Setup(x => x.Solve(requirements)).Returns(selections).Verifiable();
            RunAndAssert(selections.ToXmlString(), 0, selections,
                "--xml", "http://0install.de/feeds/test/test1.xml", "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0");
        }

        [Test(Description = "Ensures local Selections XMLs are correctly detected and parsed.")]
        public virtual void TestImportSelections()
        {
            var selections = SelectionsManagerTest.CreateTestSelections();
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.SaveXml(tempFile);
                RunAndAssert(selections.ToXmlString(), 0, selections,
                    "--xml", tempFile);
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
        /// Verifies that calling <see cref="FrontendCommand.Parse"/> and <see cref="FrontendCommand.Execute"/> causes a specific reuslt.
        /// </summary>
        /// <param name="expectedOutput">The expected string for a <see cref="IHandler.Output"/> call; <see langword="null"/> if none.</param>
        /// <param name="expectedExitStatus">The expected exit status code returned by <see cref="FrontendCommand.Execute"/>.</param>
        /// <param name="expectedSelections">The expected value passed to <see cref="IBackendHandler.ShowSelections"/>.</param>
        /// <param name="args">The arguments to pass to <see cref="FrontendCommand.Parse"/>.</param>
        protected void RunAndAssert(string expectedOutput, int expectedExitStatus, Selections expectedSelections, params string[] args)
        {
            Selections selections = null;
            HandlerMock.Setup(x => x.ShowSelections(It.IsAny<Selections>(), It.IsAny<IFeedCache>()))
                .Callback((Selections sel, IFeedCache cache) => selections = sel);

            RunAndAssert(expectedOutput, expectedExitStatus, args);

            Assert.AreEqual(selections.InterfaceID, expectedSelections.InterfaceID);
            Assert.AreEqual(selections.Command, expectedSelections.Command);
            CollectionAssert.AreEqual(selections.Implementations, expectedSelections.Implementations);
        }
    }
}
