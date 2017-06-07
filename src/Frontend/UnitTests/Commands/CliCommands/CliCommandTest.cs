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

using System.Collections.Generic;
using FluentAssertions;
using JetBrains.Annotations;
using Moq;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Utils;
using ZeroInstall.Services;
using ZeroInstall.Services.Executors;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Fetchers;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Contains common code for testing specific <see cref="CliCommand"/>s.
    /// </summary>
    /// <typeparam name="TCommand">The specific type of <see cref="CliCommand"/> to test.</typeparam>
    public abstract class CliCommandTest<TCommand> : TestWithContainer<TCommand>
        where TCommand : CliCommand
    {
        // Type covariance: TestWithContainer -> FrontendCommandTest, MockTaskHandler -> MockCommandHandler
        protected new MockCommandHandler Handler { get; private set; }


        protected CliCommandTest()
        {
            Container.Register<ICommandHandler>(Handler = new MockCommandHandler());

            Sut.Config = Config;
            Sut.FeedCache = Resolve<IFeedCache>();
            Sut.CatalogManager = Resolve<ICatalogManager>();
            Sut.OpenPgp = Resolve<IOpenPgp>();
            Sut.TrustDB = TrustDB;
            Sut.Store = Resolve<IStore>();
            Sut.PackageManager = Resolve<IPackageManager>();
            Sut.Solver = Resolve<ISolver>();
            Sut.Fetcher = Resolve<IFetcher>();
            Sut.Executor = Resolve<IExecutor>();
            Sut.SelectionsManager = Resolve<ISelectionsManager>();

            SelfUpdateUtils.NoAutoCheck = true;
        }

        /// <summary>
        /// Verifies that calling <see cref="CliCommand.Parse"/> and <see cref="CliCommand.Execute"/> causes a specific reuslt.
        /// </summary>
        /// <param name="expectedOutput">The expected string for a <see cref="ITaskHandler.Output"/> call; <c>null</c> if none.</param>
        /// <param name="expectedExitCode">The expected exit status code returned by <see cref="CliCommand.Execute"/>.</param>
        /// <param name="args">The arguments to pass to <see cref="CliCommand.Parse"/>.</param>
        protected void RunAndAssert([CanBeNull] string expectedOutput, ExitCode expectedExitCode, params string[] args)
        {
            Sut.Parse(args);
            Sut.Execute().Should().Be(expectedExitCode);
            Handler.LastOutput.Should().Be(expectedOutput);
        }

        /// <summary>
        /// Verifies that calling <see cref="CliCommand.Parse"/> and <see cref="CliCommand.Execute"/> causes a specific reuslt.
        /// </summary>
        /// <param name="expectedOutput">The expected tabular data for a <see cref="ITaskHandler.Output{T}"/> call.</param>
        /// <param name="expectedExitCode">The expected exit status code returned by <see cref="CliCommand.Execute"/>.</param>
        /// <param name="args">The arguments to pass to <see cref="CliCommand.Parse"/>.</param>
        protected void RunAndAssert<T>(IEnumerable<T> expectedOutput, ExitCode expectedExitCode, params string[] args)
        {
            Sut.Parse(args);
            Sut.Execute().Should().Be(expectedExitCode);
            Handler.LastOutputObjects.Should().Equal(expectedOutput);
        }
    }
}
