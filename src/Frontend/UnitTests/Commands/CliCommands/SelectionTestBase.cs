/*
 * Copyright 2010-2015 Bastian Eicher
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

using JetBrains.Annotations;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NDesk.Options;
using NUnit.Framework;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Contains integration tests for commands derived from <see cref="Selection"/>.
    /// </summary>
    /// <typeparam name="TCommand">The specific type of <see cref="CliCommand"/> to test.</typeparam>
    public abstract class SelectionTestBase<TCommand> : CliCommandTest<TCommand>
        where TCommand : Selection
    {
        [Test(Description = "Ensures all options are parsed and handled correctly.")]
        public virtual void TestNormal()
        {
            var requirements = RequirementsTest.CreateTestRequirements();
            var selections = SelectionsTest.CreateTestSelections();

            SolverMock.Setup(x => x.Solve(requirements)).Returns(selections);
            RunAndAssert(selections.ToXmlString(), 0, selections,
                "--xml", "http://0install.de/feeds/test/test1.xml", "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0");
        }

        [Test(Description = "Ensures local Selections XMLs are correctly detected and parsed.")]
        public virtual void TestImportSelections()
        {
            var selections = SelectionsTest.CreateTestSelections();
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.SaveXml(tempFile);
                RunAndAssert(selections.ToXmlString(), 0, selections,
                    "--xml", tempFile);
            }
        }

        [Test(Description = "Ensures calling with too many arguments raises an exception.")]
        public virtual void TestTooManyArgs()
        {
            Assert.Throws<OptionException>(
                () => Target.Parse(new[] {"http://0install.de/feeds/test/test1.xml", "arg1"}),
                "Should reject more than one argument");
        }

        /// <summary>
        /// Verifies that calling <see cref="CliCommand.Parse"/> and <see cref="CliCommand.Execute"/> causes a specific reuslt.
        /// </summary>
        /// <param name="expectedOutput">The expected string for a <see cref="ITaskHandler.Output"/> call; <see langword="null"/> if none.</param>
        /// <param name="expectedExitCode">The expected exit status code returned by <see cref="CliCommand.Execute"/>.</param>
        /// <param name="expectedSelections">The expected value passed to <see cref="ICommandHandler.ShowSelections"/>.</param>
        /// <param name="args">The arguments to pass to <see cref="CliCommand.Parse"/>.</param>
        protected void RunAndAssert([CanBeNull] string expectedOutput, ExitCode expectedExitCode, Selections expectedSelections, params string[] args)
        {
            RunAndAssert(expectedOutput, expectedExitCode, args);

            var selections = Handler.LastSelections;
            Assert.AreEqual(expectedSelections.InterfaceUri, selections.InterfaceUri);
            Assert.AreEqual(expectedSelections.Command, selections.Command);
            CollectionAssert.AreEqual(expectedSelections.Implementations, selections.Implementations);
        }
    }
}
