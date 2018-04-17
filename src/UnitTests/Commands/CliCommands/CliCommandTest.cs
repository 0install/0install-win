// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Collections.Generic;
using FluentAssertions;
using JetBrains.Annotations;
using Moq;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using Xunit;
using ZeroInstall.Commands.Utils;
using ZeroInstall.Services;
using ZeroInstall.Services.Executors;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Fetchers;
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
    [Collection("LocationsRedirect")]
    public abstract class CliCommandTest<TCommand> : TestWithMocks
        where TCommand : CliCommand
    {
        private readonly LocationsRedirect _redirect = new LocationsRedirect("0install-unit-tests");

        public override void Dispose()
        {
            _redirect.Dispose();
            base.Dispose();
        }

        private readonly AutoMockContainer _container;

        protected readonly MockCommandHandler Handler = new MockCommandHandler();

        /// <summary>
        /// The object to be tested (system under test).
        /// </summary>
        protected readonly TCommand Sut;

        /// <summary>
        /// Creates or retrieves a <see cref="Mock"/> for a specific type. Multiple requests for the same type return the same mock instance.
        /// These are the same mocks that are injected into the <see cref="Sut"/>.
        /// </summary>
        /// <remarks>All created <see cref="Mock"/>s are automatically <see cref="Mock.Verify"/>d after the test completes.</remarks>
        protected Mock<T> GetMock<T>() where T : class => _container.GetMock<T>();

        /// <summary>
        /// Provides an instance of a specific type.
        /// Will usually be a <see cref="Mock"/> as provided by <see cref="GetMock{T}"/> unless a custom instance has been registered in the constructor.
        /// </summary>
        protected T Resolve<T>() where T : class => _container.Resolve<T>();

        protected CliCommandTest()
        {
            TrustDB trustDB;
            Config config;
            _container = new AutoMockContainer(MockRepository);
            _container.Register<ITaskHandler>(Handler);
            _container.Register<ICommandHandler>(Handler);
            _container.Register(config = new Config());
            _container.Register(trustDB = new TrustDB());

            Sut = _container.Create<TCommand>();
            Sut.Config = config;
            Sut.FeedCache = Resolve<IFeedCache>();
            Sut.CatalogManager = Resolve<ICatalogManager>();
            Sut.OpenPgp = Resolve<IOpenPgp>();
            Sut.TrustDB = trustDB;
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
