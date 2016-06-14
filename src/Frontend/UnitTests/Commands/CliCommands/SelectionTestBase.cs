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
using JetBrains.Annotations;
using NanoByte.Common.Tasks;
using NDesk.Options;
using NUnit.Framework;
using ZeroInstall.Services;
using ZeroInstall.Services.Fetchers;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Feeds;
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
        [Test(Description = "Ensures calling with too many arguments raises an exception.")]
        public virtual void TestTooManyArgs()
        {
            Target.Invoking(x => x.Parse(new[] {"http://0install.de/feeds/test/test1.xml", "arg1"}))
                .ShouldThrow<OptionException>(because: "Should reject more than one argument");
        }

        /// <summary>
        /// Configures the <see cref="ISolver"/> mock to expect a call with <see cref="RequirementsTest.CreateTestRequirements"/>.
        /// </summary>
        /// <returns>The selections returned by the mock; <see cref="SelectionsTest.CreateTestSelections"/>.</returns>
        protected Selections ExpectSolve()
        {
            var requirements = RequirementsTest.CreateTestRequirements();
            var selections = SelectionsTest.CreateTestSelections();

            GetMock<ISolver>().Setup(x => x.Solve(requirements)).Returns(selections);

            var feed = FeedTest.CreateTestFeed();
            GetMock<IFeedCache>().Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(feed);

            selections.Name = feed.Name;
            return selections;
        }

        /// <summary>
        /// Configures the <see cref="ISelectionsManager"/> mock to expect the listing of uncached implementations.
        /// </summary>
        /// <param name="selections">The selections to check for uncached implementations.</param>
        /// <param name="implementations">The implementations to treat as uncached.</param>
        protected void ExpectListUncached(Selections selections, params Implementation[] implementations)
        {
            GetMock<ISelectionsManager>().Setup(x => x.GetUncachedSelections(selections)).Returns(selections.Implementations);
            GetMock<ISelectionsManager>().Setup(x => x.GetImplementations(selections.Implementations)).Returns(implementations);
        }

        /// <summary>
        /// Configures the <see cref="ISelectionsManager"/> and <see cref="IFetcher"/> mocks to expect the download of uncached implementations.
        /// </summary>
        /// <param name="selections">The selections to check for uncached implementations.</param>
        /// <param name="implementations">The implementations to treat as uncached.</param>
        protected void ExpectFetchUncached(Selections selections, params Implementation[] implementations)
        {
            ExpectListUncached(selections, implementations);
            GetMock<IFetcher>().Setup(x => x.Fetch(implementations));
        }

        /// <summary>
        /// Verifies that calling <see cref="CliCommand.Parse"/> and <see cref="CliCommand.Execute"/> causes a specific reuslt.
        /// </summary>
        /// <param name="expectedOutput">The expected string for a <see cref="ITaskHandler.Output"/> call; <c>null</c> if none.</param>
        /// <param name="expectedExitCode">The expected exit status code returned by <see cref="CliCommand.Execute"/>.</param>
        /// <param name="expectedSelections">The expected value passed to <see cref="ICommandHandler.ShowSelections"/>.</param>
        /// <param name="args">The arguments to pass to <see cref="CliCommand.Parse"/>.</param>
        protected void RunAndAssert([CanBeNull] string expectedOutput, ExitCode expectedExitCode, Selections expectedSelections, params string[] args)
        {
            RunAndAssert(expectedOutput, expectedExitCode, args);

            var selections = Handler.LastSelections;
            if (expectedSelections == null) selections.Should().BeNull();
            else
            {
                selections.InterfaceUri.Should().Be(expectedSelections.InterfaceUri);
                selections.Command.Should().Be(expectedSelections.Command);
                selections.Implementations.Should().Equal(expectedSelections.Implementations);
            }
        }
    }
}
