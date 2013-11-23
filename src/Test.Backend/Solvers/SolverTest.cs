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
using System.Linq;
using Common.Tasks;
using Moq;
using NUnit.Framework;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;
using ZeroInstall.Store;

namespace ZeroInstall.Solvers
{
    /// <summary>
    /// Contains common code for testing specific <see cref="ISolver"/> implementations.
    /// </summary>
    public abstract class SolverTest<T> : TestWithResolver<T> where T : class, ISolver
    {
        [Test]
        public void NoDependency()
        {
            var feed = new Feed
            {
                Uri = new Uri("http://0install.de/feeds/test/app.xml"),
                Elements =
                {
                    new Implementation
                    {
                        ID = "test",
                        Version = new ImplementationVersion("1.0"),
                        Commands = {new Command {Name = Command.NameRun, Path = "test-app"}}
                    }
                },
            };

            var selections = new Selections
            {
                InterfaceID = feed.Uri.ToString(), Command = Command.NameRun,
                Implementations =
                {
                    new ImplementationSelection
                    {
                        ID = "test", InterfaceID = feed.Uri.ToString(),
                        Version = new ImplementationVersion("1.0"),
                        Commands = {new Command {Name = Command.NameRun, Path = "test-app"}}
                    }
                }
            };
            RunAndAssert(
                selections,
                new Requirements {InterfaceID = feed.Uri.ToString()},
                feed);
        }

        [Test]
        public void SimpleDependency()
        {
            var appFeed = new Feed
            {
                Uri = new Uri("http://0install.de/feeds/test/app.xml"),
                Elements =
                {
                    new Implementation
                    {
                        ID = "test",
                        Version = new ImplementationVersion("1.0"),
                        Commands = {new Command {Name = Command.NameRun, Path = "test-app"}},
                        Dependencies = {new Dependency {Interface = "http://0install.de/feeds/test/lib.xml"}}
                    }
                },
            };
            var libFeed = new Feed
            {
                Uri = new Uri("http://0install.de/feeds/test/lib.xml"),
                Elements =
                {
                    new Implementation
                    {
                        ID = "test",
                        Version = new ImplementationVersion("1.0")
                    }
                },
            };

            var selections = new Selections
            {
                InterfaceID = appFeed.Uri.ToString(), Command = Command.NameRun,
                Implementations =
                {
                    new ImplementationSelection
                    {
                        ID = "test", InterfaceID = appFeed.Uri.ToString(),
                        Version = new ImplementationVersion("1.0"),
                        Commands = {new Command {Name = Command.NameRun, Path = "test-app"}},
                        Dependencies = {new Dependency {Interface = "http://0install.de/feeds/test/lib.xml"}}
                    },
                    new ImplementationSelection
                    {
                        ID = "test", InterfaceID = libFeed.Uri.ToString(),
                        Version = new ImplementationVersion("1.0")
                    }
                }
            };
            RunAndAssert(
                selections,
                new Requirements {InterfaceID = appFeed.Uri.ToString()},
                appFeed, libFeed);
        }

        [Test]
        public void CyclicDependency()
        {
            var feed1 = new Feed
            {
                Uri = new Uri("http://0install.de/feeds/test/feed1.xml"),
                Name = "Test App",
                Summaries = {"a test app"},
                Elements =
                {
                    new Implementation
                    {
                        ID = "test",
                        Version = new ImplementationVersion("1.0"),
                        Commands = {new Command {Name = Command.NameRun, Path = "test-app"}},
                        Dependencies = {new Dependency {Interface = "http://0install.de/feeds/test/feed2.xml"}}
                    }
                },
            };
            var feed2 = new Feed
            {
                Uri = new Uri("http://0install.de/feeds/test/feed2.xml"),
                Name = "Test Lib",
                Summaries = {"a test lib"},
                Elements =
                {
                    new Implementation
                    {
                        ID = "test",
                        Version = new ImplementationVersion("1.0"),
                        Dependencies = {new Dependency {Interface = "http://0install.de/feeds/test/feed1.xml"}}
                    }
                },
            };

            var selections = new Selections
            {
                InterfaceID = feed1.Uri.ToString(),
                Command = Command.NameRun,
                Implementations =
                {
                    new ImplementationSelection
                    {
                        ID = "test", InterfaceID = feed1.Uri.ToString(),
                        Version = new ImplementationVersion("1.0"),
                        Commands = {new Command {Name = Command.NameRun, Path = "test-app"}},
                        Dependencies = {new Dependency {Interface = "http://0install.de/feeds/test/feed2.xml"}}
                    },
                    new ImplementationSelection
                    {
                        ID = "test", InterfaceID = feed2.Uri.ToString(),
                        Version = new ImplementationVersion("1.0"),
                        Dependencies = {new Dependency {Interface = "http://0install.de/feeds/test/feed1.xml"}}
                    }
                }
            };
            RunAndAssert(
                selections,
                new Requirements {InterfaceID = feed1.Uri.ToString()},
                feed1, feed2);
        }

        private void RunAndAssert(Selections expectedSelections, Requirements requirements, params Feed[] feeds)
        {
            var handlerMock = Resolver.GetMock<IHandler>();
            handlerMock.SetupGet(x => x.CancellationToken).Returns(new CancellationToken());

            var feedManagerMock = Resolver.GetMock<IFeedManager>();
            foreach (var feed in feeds) feed.Normalize(feed.Uri.ToString());
            var feedDictionary = feeds.ToDictionary(feed => feed.Uri.ToString());
            bool temp = false;
            feedManagerMock.Setup(x => x.GetFeed(It.IsAny<string>(), ref temp)).Returns((string feedID, bool temp1) => feedDictionary[feedID]);

            var selections = Target.Solve(requirements);
            Assert.AreEqual(expectedSelections, selections);
        }
    }
}
