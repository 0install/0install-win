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
using NUnit.Mocks;
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
            private readonly Action<string> _outputCallback;

            /// <summary>
            /// Creates a new mock handler.
            /// </summary>
            /// <param name="outputCallback">Callback to be raised every time <see cref="Output"/> is called.</param>
            public MockHandler(Action<string> outputCallback)
            {
                _outputCallback = outputCallback;
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

        /// <summary>The content of the last <see cref="MockHandler.Output"/> call.</summary>
        private string _result;

        protected DynamicMock CacheMock { get; private set; }
        protected DynamicMock SolverMock { get; private set; }
        protected DynamicMock StoreMock { get; private set; }
        protected DynamicMock FetcherMock { get; private set; }
        protected Policy Policy { get; private set; }

        /// <summary>The command to be tested.</summary>
        protected CommandBase Command { get; private set; }
        #endregion

        /// <summary>Creates an instance of the command type to be tested using <see cref="_handler"/> and <see cref="Policy"/>.</summary>
        protected abstract CommandBase GetCommand();

        [SetUp]
        public virtual void SetUp()
        {
            _result = null;
            _handler = new MockHandler(message => _result = message);

            CacheMock = new DynamicMock("MockCache", typeof(IFeedCache));
            SolverMock = new DynamicMock("SolverMock", typeof(ISolver));
            StoreMock = new DynamicMock("MockStore", typeof(IStore));
            FetcherMock = new DynamicMock("MockFetcher", typeof(IFetcher));
            FetcherMock.SetReturnValue("get_Store", StoreMock.MockInstance);

            Policy = new Policy(new Preferences(), new FeedManager((IFeedCache)CacheMock.MockInstance), (ISolver)SolverMock.MockInstance, (IFetcher)FetcherMock.MockInstance, _handler);

            Command = GetCommand();
        }

        [TearDown]
        public virtual void TearDown()
        {
            CacheMock.Verify();
            SolverMock.Verify();
            StoreMock.Verify();
            FetcherMock.Verify();
        }

        /// <summary>
        /// Verifies that calling <see cref="CommandBase.Parse"/> and <see cref="CommandBase.Execute"/> causes a specific reuslt.
        /// </summary>
        /// <param name="args">The arguments to pass to <see cref="CommandBase.Parse"/>.</param>
        /// <param name="output">The expected string for a <see cref="IHandler.Output"/> call; <see langword="null"/> if none.</param>
        /// <param name="exitStatus">The expected exit status code returned by <see cref="CommandBase.Execute"/>.</param>
        protected void AssertParseExecuteResult(IEnumerable<string> args, string output, int exitStatus)
        {
            Command.Parse(args);
            Assert.AreEqual(exitStatus, Command.Execute());
            Assert.AreEqual(output, _result);
        }

        [Test(Description = "Ensures an exception is thrown if Execute() is called before Parse().")]
        public void TestExecuteBeforeParse()
        {
            Assert.Throws<InvalidOperationException>(() => Command.Execute(), "Execute should not allow calls before Parse");
        }
    }
}
