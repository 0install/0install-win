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
using NUnit.Framework;
using ZeroInstall.Commands.Utils;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Fetchers;
using ZeroInstall.Services.Injector;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store;
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

        protected override void Register(AutoMockContainer container)
        {
            container.Register<ICommandHandler>(Handler = new MockCommandHandler());
        }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Target.Config = Config;
            Target.FeedCache = Resolve<IFeedCache>();
            Target.CatalogManager = Resolve<ICatalogManager>();
            Target.OpenPgp = Resolve<IOpenPgp>();
            Target.TrustDB = TrustDB;
            Target.Store = Resolve<IStore>();
            Target.PackageManager = Resolve<IPackageManager>();
            Target.Solver = Resolve<ISolver>();
            Target.Fetcher = Resolve<IFetcher>();
            Target.Executor = Resolve<IExecutor>();
            Target.SelectionsManager = Resolve<ISelectionsManager>();
            Target.Exporter = Resolve<IExporter>();

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
            Target.Parse(args);
            Target.Execute().Should().Be(expectedExitCode);
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
            Target.Parse(args);
            Target.Execute().Should().Be(expectedExitCode);
            Handler.LastOutputObjects.Should().Equal(expectedOutput);
        }
    }
}
