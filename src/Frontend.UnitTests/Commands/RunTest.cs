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
using System.Diagnostics;
using Common.Storage;
using Moq;
using NUnit.Framework;
using ZeroInstall.Services.Fetchers;
using ZeroInstall.Services.Injector;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Contains integration tests for <see cref="Run"/>.
    /// </summary>
    [TestFixture]
    public class RunTest : SelectionTestBase<Run>
    {
        [Test(Description = "Ensures all options are parsed and handled correctly.")]
        public override void TestNormal()
        {
            var requirements = RequirementsTest.CreateTestRequirements();
            var selections = SelectionsManagerTest.CreateTestSelections();

            var testFeed1 = FeedTest.CreateTestFeed();
            testFeed1.Uri = new Uri("http://0install.de/feeds/test/sub1.xml");
            testFeed1.Name = "Sub 1";
            var testImplementation1 = testFeed1[selections.Implementations[0].ID];
            Container.GetMock<IFeedCache>().Setup(x => x.GetFeed("http://0install.de/feeds/test/sub1.xml")).Returns(testFeed1);

            var testImplementation2 = new Implementation {ID = "id2", ManifestDigest = new ManifestDigest(sha256: "abc"), Version = new ImplementationVersion("1.0")};
            var testFeed2 = new Feed
            {
                Uri = new Uri("http://0install.de/feeds/test/sub2.xml"),
                Name = "Sub 2",
                Elements = {testImplementation2}
            };
            Container.GetMock<IFeedCache>().Setup(x => x.GetFeed("http://0install.de/feeds/test/sub2.xml")).Returns(testFeed2);

            Container.GetMock<ISolver>().Setup(x => x.Solve(requirements)).Returns(selections);

            // Download uncached implementations
            Container.GetMock<IStore>().Setup(x => x.Contains(It.IsAny<ManifestDigest>())).Returns(false);
            Container.GetMock<IFetcher>().Setup(x => x.Fetch(new[] {testImplementation1, testImplementation2}));

            var executorMock = Container.GetMock<IExecutor>();
            executorMock.SetupSet(x => x.Main = "Main");
            executorMock.SetupSet(x => x.Wrapper = "Wrapper");
            executorMock.Setup(x => x.Start(selections, "--arg1", "--arg2")).Returns((Process)null);

            RunAndAssert(null, 0, selections,
                "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0",
                "--main=Main", "--wrapper=Wrapper", "http://0install.de/feeds/test/test1.xml", "--arg1", "--arg2");
        }

        [Test(Description = "Ensures local Selections XMLs are correctly detected and parsed.")]
        public override void TestImportSelections()
        {
            var testFeed1 = FeedTest.CreateTestFeed();
            testFeed1.Uri = new Uri("http://0install.de/feeds/test/sub1.xml");
            testFeed1.Name = "Sub 1";
            Container.GetMock<IFeedCache>().Setup(x => x.GetFeed("http://0install.de/feeds/test/sub1.xml")).Returns(testFeed1);
            var testImplementation1 = (Implementation)testFeed1.Elements[0];

            var testImplementation2 = new Implementation {ID = "id2", ManifestDigest = new ManifestDigest(sha256: "abc"), Version = new ImplementationVersion("1.0")};
            var testFeed2 = new Feed
            {
                Uri = new Uri("http://0install.de/feeds/test/sub2.xml"),
                Name = "Sub 2",
                Elements = {testImplementation2}
            };
            Container.GetMock<IFeedCache>().Setup(x => x.GetFeed("http://0install.de/feeds/test/sub2.xml")).Returns(testFeed2);

            var selections = SelectionsManagerTest.CreateTestSelections();

            // Download uncached implementations
            Container.GetMock<IStore>().Setup(x => x.Contains(It.IsAny<ManifestDigest>())).Returns(false);
            Container.GetMock<IFetcher>().Setup(x => x.Fetch(new[] {testImplementation1, testImplementation2}));

            Container.GetMock<IExecutor>().Setup(x => x.Start(It.IsAny<Selections>(), "--arg1", "--arg2")).Returns((Process)null);
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.SaveXml(tempFile);
                RunAndAssert(null, 0, selections, tempFile, "--arg1", "--arg2");
            }
        }

        [Ignore("Not applicable")]
        public override void TestTooManyArgs()
        {}
    }
}
