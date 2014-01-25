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
using Common.Storage;
using Common.Utils;
using NUnit.Framework;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Fetchers;
using ZeroInstall.Model;
using ZeroInstall.Solvers;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;
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
        }

        /// <summary>
        /// Verifies that calling <see cref="FrontendCommand.Parse"/> and <see cref="FrontendCommand.Execute"/> causes a specific reuslt.
        /// </summary>
        /// <param name="expectedOutput">The expected string for a <see cref="IHandler.Output"/> call; <see langword="null"/> if none.</param>
        /// <param name="expectedExitStatus">The expected exit status code returned by <see cref="FrontendCommand.Execute"/>.</param>
        /// <param name="args">The arguments to pass to <see cref="FrontendCommand.Parse"/>.</param>
        protected void RunAndAssert(string expectedOutput, int expectedExitStatus, params string[] args)
        {
            Target.Parse(args);
            Assert.AreEqual(expectedExitStatus, Target.Execute());
            Assert.AreEqual(expectedOutput, MockHandler.LastOutput);
        }

        [Test]
        public void TestGetCanonicalID()
        {
            // Absolute paths
            if (WindowsUtils.IsWindows)
            {
                Assert.AreEqual(@"C:\test\file", Target.GetCanonicalID("file:///C:/test/file"));
                Assert.AreEqual(@"C:\test\file", Target.GetCanonicalID(@"C:\test\file"));
            }
            if (MonoUtils.IsUnix)
            {
                Assert.AreEqual("/var/test/file", Target.GetCanonicalID("file:///var/test/file"));
                Assert.AreEqual("/var/test/file", Target.GetCanonicalID("/var/test/file"));
            }

            // Relative paths
            Assert.AreEqual(
                new[] {Environment.CurrentDirectory, "test", "file"}.Aggregate(Path.Combine),
                Target.GetCanonicalID("file:test/file"));
            Assert.AreEqual(
                new[] {Environment.CurrentDirectory, "test", "file"}.Aggregate(Path.Combine),
                Target.GetCanonicalID(Path.Combine("test", "file")));

            // Invalid paths
            Assert.Throws<InvalidInterfaceIDException>(() => Target.GetCanonicalID("file:/test/file"));
            if (WindowsUtils.IsWindows) Assert.Throws<InvalidInterfaceIDException>(() => Target.GetCanonicalID(":::"));

            // URIs
            Assert.AreEqual("http://0install.de/feeds/test/test1.xml", Target.GetCanonicalID("http://0install.de/feeds/test/test1.xml"));
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

            Assert.AreEqual("http://0install.de/feeds/test/test1.xml", Target.GetCanonicalID("alias:test"));
            Assert.Throws<InvalidInterfaceIDException>(() => Target.GetCanonicalID("alias:invalid"));
        }
    }
}
