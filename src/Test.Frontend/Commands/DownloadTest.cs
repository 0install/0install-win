/*
 * Copyright 2010-2012 Bastian Eicher
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
using Common.Storage;
using NUnit.Framework;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Contains code for testing <see cref="Download"/>.
    /// </summary>
    [TestFixture]
    public class DownloadTest : SelectionTest
    {
        /// <inheritdoc/>
        protected override FrontendCommand GetCommand()
        {
            return new Download(Policy);
        }

        [Test(Description = "Ensures all options are parsed and handled correctly.")]
        public override void TestNormal()
        {
            var requirements = RequirementsTest.CreateTestRequirements();
            var selections = SelectionsTest.CreateTestSelections();

            var testFeed1 = FeedTest.CreateTestFeed();
            testFeed1.Uri = new Uri("http://0install.de/feeds/test/sub1.xml");
            testFeed1.Name = "Sub 1";
            var testImplementation1 = testFeed1[selections.Implementations[0].ID];
            CacheMock.Setup(x => x.GetFeed("http://0install.de/feeds/test/sub1.xml")).Returns(testFeed1);

            var testImplementation2 = new Implementation {ID = "id2", ManifestDigest = new ManifestDigest(sha256: "abc"), Version = new ImplementationVersion("1.0")};
            var testFeed2 = new Feed
            {
                Uri = new Uri("http://0install.de/feeds/test/sub2.xml"),
                Name = "Sub 2",
                Elements = {testImplementation2}
            };
            CacheMock.Setup(x => x.GetFeed("http://0install.de/feeds/test/sub2.xml")).Returns(testFeed2);

            var refreshPolicy = Policy.Clone();
            refreshPolicy.FeedManager.Refresh = true;

            var args = new[] {"http://0install.de/feeds/test/test1.xml", "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0"};
            Command.Parse(args);

            bool stale;
            SolverMock.Setup(x => x.Solve(requirements, Policy, out stale)).Returns(selections).Verifiable(); // First Solve()
            SolverMock.Setup(x => x.Solve(requirements, refreshPolicy, out stale)).Returns(selections).Verifiable(); // Refresh Solve() because there are uncached implementations

            // Download uncached implementations
            FetcherMock.Setup(x => x.FetchImplementations(new[] {testImplementation1, testImplementation2}, Policy.Handler)).Verifiable();

            Assert.AreEqual(0, Command.Execute());
        }

        [Test(Description = "Ensures local Selections XMLs are correctly detected and parsed.")]
        public override void TestImportSelections()
        {
            var testFeed1 = FeedTest.CreateTestFeed();
            testFeed1.Uri = new Uri("http://0install.de/feeds/test/sub1.xml");
            testFeed1.Name = "Sub 1";
            CacheMock.Setup(x => x.GetFeed("http://0install.de/feeds/test/sub1.xml")).Returns(testFeed1);

            var testImplementation2 = new Implementation {ID = "id2", ManifestDigest = new ManifestDigest(sha256: "abc"), Version = new ImplementationVersion("1.0")};
            var testFeed2 = new Feed
            {
                Uri = new Uri("http://0install.de/feeds/test/sub2.xml"),
                Name = "Sub 2",
                Elements = {testImplementation2}
            };
            CacheMock.Setup(x => x.GetFeed("http://0install.de/feeds/test/sub2.xml")).Returns(testFeed2);

            var selections = SelectionsTest.CreateTestSelections();
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.Save(tempFile.Path);
                var args = new[] {tempFile.Path};

                Command.Parse(args);
                Assert.AreEqual(0, Command.Execute());
            }
        }
    }
}
