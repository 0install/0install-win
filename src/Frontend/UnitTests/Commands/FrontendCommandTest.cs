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

using System;
using System.IO;
using System.Linq;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NUnit.Framework;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Services;
using ZeroInstall.Services.Fetchers;
using ZeroInstall.Services.Injector;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Contains common code for testing specific <see cref="FrontendCommand"/>s.
    /// </summary>
    /// <typeparam name="TCommand">The specific type of <see cref="FrontendCommand"/> to test.</typeparam>
    public abstract class FrontendCommandTest<TCommand> : TestWithContainer<TCommand>
        where TCommand : FrontendCommand
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Target.Config = Container.Resolve<Config>();
            Target.FeedCache = Container.Resolve<IFeedCache>();
            Target.OpenPgp = Container.Resolve<IOpenPgp>();
            Target.Store = Container.Resolve<IStore>();
            Target.Solver = Container.Resolve<ISolver>();
            Target.Fetcher = Container.Resolve<IFetcher>();
            Target.Executor = Container.Resolve<IExecutor>();
        }

        // Type covariance: TestWithContainer -> FrontendCommandTest, MockTaskHandler -> MockCommandHandler
        protected new MockCommandHandler MockHandler { get; private set; }

        protected override MockTaskHandler CreateMockHandler()
        {
            MockHandler = new MockCommandHandler();
            Container.Register<ICommandHandler>(MockHandler);
            return MockHandler;
        }

        /// <summary>
        /// Verifies that calling <see cref="FrontendCommand.Parse"/> and <see cref="FrontendCommand.Execute"/> causes a specific reuslt.
        /// </summary>
        /// <param name="expectedOutput">The expected string for a <see cref="ITaskHandler.Output"/> call; <see langword="null"/> if none.</param>
        /// <param name="expectedExitStatus">The expected exit status code returned by <see cref="FrontendCommand.Execute"/>.</param>
        /// <param name="args">The arguments to pass to <see cref="FrontendCommand.Parse"/>.</param>
        protected void RunAndAssert(string expectedOutput, int expectedExitStatus, params string[] args)
        {
            Target.Parse(args);
            Assert.AreEqual(expectedExitStatus, Target.Execute());
            Assert.AreEqual(expectedOutput, MockHandler.LastOutput);
        }

        [Test]
        public void TestGetCanonicalUriRemote()
        {
            Assert.AreEqual("http://0install.de/feeds/test/test1.xml", Target.GetCanonicalUri("http://0install.de/feeds/test/test1.xml").ToStringRfc());
        }

        [Test]
        public void TestGetCanonicalUriFile()
        {
            // Absolute paths
            if (WindowsUtils.IsWindows)
            {
                Assert.AreEqual("file:///C:/test/file", Target.GetCanonicalUri(@"C:\test\file").ToStringRfc());
                Assert.AreEqual("file:///C:/test/file", Target.GetCanonicalUri(@"file:///C:\test\file").ToStringRfc());
                Assert.AreEqual("file:///C:/test/file", Target.GetCanonicalUri("file:///C:/test/file").ToStringRfc());
            }
            if (UnixUtils.IsUnix)
            {
                Assert.AreEqual("file:///var/test/file", Target.GetCanonicalUri("/var/test/file").ToStringRfc());
                Assert.AreEqual("file:///var/test/file", Target.GetCanonicalUri("file:///var/test/file").ToStringRfc());
            }

            // Relative paths
            Assert.AreEqual(
                expected: new[] {Environment.CurrentDirectory, "test", "file"}.Aggregate(Path.Combine),
                actual: Target.GetCanonicalUri(Path.Combine("test", "file")).ToString());
            Assert.AreEqual(
                expected: new[] {Environment.CurrentDirectory, "test", "file"}.Aggregate(Path.Combine),
                actual: Target.GetCanonicalUri("file:test/file").ToString());

            // Invalid paths
            Assert.Throws<UriFormatException>(() => Target.GetCanonicalUri("file:/test/file"));
            if (WindowsUtils.IsWindows) Assert.Throws<UriFormatException>(() => Target.GetCanonicalUri(":::"));
        }

        [Test]
        public void TestGetCanonicalUriAliases()
        {
            // Fake an alias
            new AppList
            {
                Entries =
                {
                    new AppEntry
                    {
                        InterfaceUri = FeedTest.Test1Uri,
                        AccessPoints = new AccessPointList {Entries = {new AppAlias {Name = "test"}}}
                    }
                }
            }.SaveXml(AppList.GetDefaultPath());

            Assert.AreEqual(FeedTest.Test1Uri, Target.GetCanonicalUri("alias:test"));
            Assert.Throws<UriFormatException>(() => Target.GetCanonicalUri("alias:invalid"));
        }
    }
}
