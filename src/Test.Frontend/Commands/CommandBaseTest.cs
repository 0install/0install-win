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

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Contains common code for testing specific <see cref="CommandBase"/>s.
    /// </summary>
    public abstract class CommandBaseTest
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
        private IHandler _handler;

        /// <summary>The content of the last <see cref="MockHandler.ShowSelections"/> call.</summary>
        private Selections _selections;

        /// <summary>The content of the last <see cref="MockHandler.Output"/> call.</summary>
        private string _output;

        private MockRepository _mockRepository;

        protected Mock<IFeedCache> CacheMock { get; private set; }
        protected Mock<IOpenPgp> OpenPgpMock { get; private set; }
        protected Mock<ISolver> SolverMock { get; private set; }
        protected Mock<IFetcher> FetcherMock { get; private set; }
        protected Policy Policy { get; private set; }

        /// <summary>The command to be tested.</summary>
        protected CommandBase Command { get; private set; }
        #endregion

        /// <summary>Creates an instance of the command type to be tested using <see cref="_handler"/> and <see cref="Policy"/>.</summary>
        protected abstract CommandBase GetCommand();

        [SetUp]
        public void SetUp()
        {
            _selections = null;
            _output = null;

            // Store values passed to callback methods in fields
            _handler = new MockHandler(selections => _selections = selections, information => _output = information);

            _mockRepository = new MockRepository(MockBehavior.Strict);
            CacheMock = _mockRepository.Create<IFeedCache>();
            OpenPgpMock = _mockRepository.Create<IOpenPgp>();
            SolverMock = _mockRepository.Create<ISolver>();
            FetcherMock = _mockRepository.Create<IFetcher>(MockBehavior.Loose);
            FetcherMock.Setup(x => x.Store).Returns(new Mock<IStore>().Object);
            Policy = new Policy(
                new Config(), new FeedManagerMock(CacheMock.Object),
                FetcherMock.Object, OpenPgpMock.Object, SolverMock.Object, _handler);

            Command = GetCommand();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.Verify();
        }

        /// <summary>
        /// Verifies that calling <see cref="CommandBase.Parse"/> and <see cref="CommandBase.Execute"/> causes a specific reuslt.
        /// </summary>
        /// <param name="args">The arguments to pass to <see cref="CommandBase.Parse"/>.</param>
        /// <param name="selections">The expected value for a <see cref="IHandler.ShowSelections"/> call; <see langword="null"/> if none.</param>
        /// <param name="output">The expected string for a <see cref="IHandler.Output"/> call; <see langword="null"/> if none.</param>
        /// <param name="exitStatus">The expected exit status code returned by <see cref="CommandBase.Execute"/>.</param>
        protected void AssertParseExecuteResult(IEnumerable<string> args, Selections selections, string output, int exitStatus)
        {
            Command.Parse(args);
            Assert.AreEqual(exitStatus, Command.Execute());
            Assert.AreEqual(selections, _selections);
            Assert.AreEqual(output, _output);
        }

        [Test(Description = "Ensures an exception is thrown if Execute() is called before Parse().")]
        public void TestExecuteBeforeParse()
        {
            Assert.Throws<InvalidOperationException>(() => Command.Execute(), "Execute should not allow calls before Parse");
        }
    }
}
