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

using System;
using System.IO;
using System.Linq;
using Common.Storage;
using Common.Utils;
using Moq;
using NUnit.Framework;
using ZeroInstall.Backend;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Contains common code for testing specific <see cref="FrontendCommand"/>s.
    /// </summary>
    public abstract class FrontendCommandTest
    {
        #region Mock classes
        /// <summary>
        /// Works like <see cref="SilentHandler"/> but reports <see cref="Output"/> calls via a delegate.
        /// </summary>
        private class MockHandler : SilentHandler
        {
            private readonly Action<Selections> _showSelectionsCallback;
            private readonly Action<string> _outputCallback;

            /// <summary>
            /// Creates a new mock handler.
            /// </summary>
            /// <param name="showSelectionsCallback">Callback to be raised every time <see cref="ShowSelections"/> is called.</param>
            /// <param name="outputCallback">Callback to be raised every time <see cref="Output"/> is called.</param>
            public MockHandler(Action<Selections> showSelectionsCallback, Action<string> outputCallback)
            {
                _outputCallback = outputCallback;
                _showSelectionsCallback = showSelectionsCallback;
            }

            /// <inheritdoc/>
            public override void ShowSelections(Selections selections, IFeedCache feedCache)
            {
                _showSelectionsCallback(selections);
            }

            /// <inheritdoc/>
            public override void Output(string title, string information)
            {
                _outputCallback(information);
            }
        }
        #endregion

        #region Properties
        protected IBackendHandler Handler { get; private set; }

        /// <summary>The content of the last <see cref="MockHandler.ShowSelections"/> call.</summary>
        protected Selections Selections { get; private set; }

        /// <summary>The content of the last <see cref="MockHandler.Output"/> call.</summary>
        private string _output;

        private MockRepository _mockRepository;

        protected Mock<IFeedCache> CacheMock { get; private set; }
        protected Mock<IOpenPgp> OpenPgpMock { get; private set; }
        protected Mock<IStore> StoreMock { get; private set; }
        protected Mock<ISolver> SolverMock { get; private set; }
        protected Mock<IFetcher> FetcherMock { get; private set; }

        /// <summary>The command to be tested.</summary>
        protected FrontendCommand Command { get; private set; }
        #endregion

        /// <summary>Creates an instance of the command type to be tested using <see cref="Handler"/> and <see cref="Resolver"/>.</summary>
        protected abstract FrontendCommand GetCommand();

        private LocationsRedirect _redirect;

        [SetUp]
        public void SetUp()
        {
            // Don't store generated executables settings in real user profile
            _redirect = new LocationsRedirect("0install-unit-tests");

            Selections = null;
            _output = null;

            // Store values passed to callback methods in fields
            Handler = new MockHandler(selections => Selections = selections, information => _output = information);

            _mockRepository = new MockRepository(MockBehavior.Strict);
            CacheMock = _mockRepository.Create<IFeedCache>();
            OpenPgpMock = _mockRepository.Create<IOpenPgp>();
            StoreMock = _mockRepository.Create<IStore>();
            SolverMock = _mockRepository.Create<ISolver>();
            FetcherMock = _mockRepository.Create<IFetcher>(MockBehavior.Loose);

            Command = GetCommand();
            Command.Config = new Config();
            Command.FeedCache = CacheMock.Object;
            Command.OpenPgp = OpenPgpMock.Object;
            Command.Store = StoreMock.Object;
            Command.Solver = SolverMock.Object;
            Command.Fetcher = FetcherMock.Object;
        }

        [TearDown]
        public void TearDown()
        {
            _redirect.Dispose();
            _mockRepository.Verify();
        }

        /// <summary>
        /// Verifies that calling <see cref="FrontendCommand.Parse"/> and <see cref="FrontendCommand.Execute"/> causes a specific reuslt.
        /// </summary>
        /// <param name="output">The expected string for a <see cref="IHandler.Output"/> call; <see langword="null"/> if none.</param>
        /// <param name="exitStatus">The expected exit status code returned by <see cref="FrontendCommand.Execute"/>.</param>
        /// <param name="args">The arguments to pass to <see cref="FrontendCommand.Parse"/>.</param>
        protected void AssertParseExecuteResult(string output, int exitStatus, params string[] args)
        {
            Command.Parse(args);
            Assert.AreEqual(exitStatus, Command.Execute());
            Assert.AreEqual(output, _output);
        }

        [Test]
        public void TestGetCanonicalID()
        {
            // Absolute paths
            if (WindowsUtils.IsWindows)
            {
                Assert.AreEqual(@"C:\test\file", Command.GetCanonicalID("file:///C:/test/file"));
                Assert.AreEqual(@"C:\test\file", Command.GetCanonicalID(@"C:\test\file"));
            }
            if (MonoUtils.IsUnix)
            {
                Assert.AreEqual("/var/test/file", Command.GetCanonicalID("file:///var/test/file"));
                Assert.AreEqual("/var/test/file", Command.GetCanonicalID("/var/test/file"));
            }

            // Relative paths
            Assert.AreEqual(
                new[] {Environment.CurrentDirectory, "test", "file"}.Aggregate(Path.Combine),
                Command.GetCanonicalID("file:test/file"));
            Assert.AreEqual(
                new[] {Environment.CurrentDirectory, "test", "file"}.Aggregate(Path.Combine),
                Command.GetCanonicalID(Path.Combine("test", "file")));

            // Invalid paths
            Assert.Throws<InvalidInterfaceIDException>(() => Command.GetCanonicalID("file:/test/file"));
            if (WindowsUtils.IsWindows) Assert.Throws<InvalidInterfaceIDException>(() => Command.GetCanonicalID(":::"));

            // URIs
            Assert.AreEqual("http://0install.de/feeds/test/test1.xml", Command.GetCanonicalID("http://0install.de/feeds/test/test1.xml"));
        }

        [Test]
        public void TestGetCanonicalIDAliases()
        {
            // Fake an alias
            new AppList
            {
                Entries =
                {
                    new AppEntry
                    {
                        InterfaceID = "http://0install.de/feeds/test/test1.xml",
                        AccessPoints = new AccessPointList {Entries = {new AppAlias {Name = "test"}}}
                    }
                }
            }.SaveXml(AppList.GetDefaultPath());

            Assert.AreEqual("http://0install.de/feeds/test/test1.xml", Command.GetCanonicalID("alias:test"));
            Assert.Throws<InvalidInterfaceIDException>(() => Command.GetCanonicalID("alias:invalid"));
        }
    }
}
