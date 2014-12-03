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
using System.IO;
using System.Linq;
using Moq;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NUnit.Framework;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Store;
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
            var selections = SelectionsTest.CreateTestSelections();

            var testFeed1 = FeedTest.CreateTestFeed();
            testFeed1.Uri = new FeedUri(FeedTest.Sub1Uri);
            testFeed1.Name = "Sub 1";
            var testImplementation1 = testFeed1[selections.Implementations[0].ID];
            FeedCacheMock.Setup(x => x.GetFeed(FeedTest.Sub1Uri)).Returns(testFeed1);

            var testImplementation2 = new Implementation {ID = "id2", ManifestDigest = new ManifestDigest(sha256: "abc"), Version = new ImplementationVersion("1.0")};
            var testFeed2 = new Feed
            {
                Uri = new FeedUri(FeedTest.Sub2Uri),
                Name = "Sub 2",
                Elements = {testImplementation2}
            };
            FeedCacheMock.Setup(x => x.GetFeed(FeedTest.Sub2Uri)).Returns(testFeed2);

            SolverMock.Setup(x => x.Solve(requirements)).Returns(selections);

            // Download uncached implementations
            StoreMock.Setup(x => x.Contains(It.IsAny<ManifestDigest>())).Returns(false);
            FetcherMock.Setup(x => x.Fetch(new[] {testImplementation1, testImplementation2}));

            ExecutorMock.SetupSet(x => x.Main = "Main");
            ExecutorMock.SetupSet(x => x.Wrapper = "Wrapper");
            ExecutorMock.Setup(x => x.Start(selections, "--arg1", "--arg2")).Returns((Process)null);

            RunAndAssert(null, 0, selections,
                "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0",
                "--main=Main", "--wrapper=Wrapper", "http://0install.de/feeds/test/test1.xml", "--arg1", "--arg2");
        }

        [Test(Description = "Ensures local Selections XMLs are correctly detected and parsed.")]
        public override void TestImportSelections()
        {
            var testFeed1 = FeedTest.CreateTestFeed();
            testFeed1.Uri = new FeedUri(FeedTest.Sub1Uri);
            testFeed1.Name = "Sub 1";
            FeedCacheMock.Setup(x => x.GetFeed(FeedTest.Sub1Uri)).Returns(testFeed1);
            var testImplementation1 = (Implementation)testFeed1.Elements[0];

            var testImplementation2 = new Implementation {ID = "id2", ManifestDigest = new ManifestDigest(sha256: "abc"), Version = new ImplementationVersion("1.0")};
            var testFeed2 = new Feed
            {
                Uri = new FeedUri(FeedTest.Sub2Uri),
                Name = "Sub 2",
                Elements = {testImplementation2}
            };
            FeedCacheMock.Setup(x => x.GetFeed(FeedTest.Sub2Uri)).Returns(testFeed2);

            var selections = SelectionsTest.CreateTestSelections();

            // Download uncached implementations
            StoreMock.Setup(x => x.Contains(It.IsAny<ManifestDigest>())).Returns(false);
            FetcherMock.Setup(x => x.Fetch(new[] {testImplementation1, testImplementation2}));

            ExecutorMock.Setup(x => x.Start(It.IsAny<Selections>(), "--arg1", "--arg2")).Returns((Process)null);
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.SaveXml(tempFile);
                RunAndAssert(null, 0, selections, tempFile, "--arg1", "--arg2");
            }
        }

        [Ignore("Not applicable")]
        public override void TestTooManyArgs()
        {}

        [Test]
        public void TestGetCanonicalUriRemote()
        {
            Assert.AreEqual("http://0install.de/feeds/test/test1.xml", Target.GetCanonicalUri("http://0install.de/feeds/test/test1.xml").ToStringRfc());
        }

        [Test]
        public void TestGetCanonicalUriFile()
        {
            CatalogManagerMock.Setup(x => x.GetCached()).Returns(new Catalog());
            CatalogManagerMock.Setup(x => x.GetOnline()).Returns(new Catalog());

            // Absolute paths
            if (WindowsUtils.IsWindows)
            {
                Assert.AreEqual("file:///C:/test/file", Target.GetCanonicalUri(@"C:\test\file").ToStringRfc());
                Assert.AreEqual("file:///C:/test/file", Target.GetCanonicalUri(@"file:///C:\test\file").ToStringRfc());
                Assert.AreEqual("file:///C:/test/file", Target.GetCanonicalUri("file:///C:/test/file").ToStringRfc());
            }
            if (UnixUtils.IsUnix)
            {
                Assert.AreEqual("file:///test/file", Target.GetCanonicalUri("/test/file").ToStringRfc());
                Assert.AreEqual("file:///test/file", Target.GetCanonicalUri("file:///test/file").ToStringRfc());
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

        [Test]
        public void TestGetCanonicalUriCatalogCached()
        {
            CatalogManagerMock.Setup(x => x.GetCached()).Returns(new Catalog {Feeds = {new Feed {Uri = FeedTest.Test1Uri, Name = "MyApp"}}});
            Assert.AreEqual(
                expected: FeedTest.Test1Uri,
                actual: Target.GetCanonicalUri("MyApp"));
        }

        [Test]
        public void TestGetCanonicalUriCatalogOnline()
        {
            CatalogManagerMock.Setup(x => x.GetCached()).Returns(new Catalog());
            CatalogManagerMock.Setup(x => x.GetOnline()).Returns(new Catalog {Feeds = {new Feed {Uri = FeedTest.Test1Uri, Name = "MyApp"}}});
            Assert.AreEqual(
                expected: FeedTest.Test1Uri,
                actual: Target.GetCanonicalUri("MyApp"));
        }
    }
}
