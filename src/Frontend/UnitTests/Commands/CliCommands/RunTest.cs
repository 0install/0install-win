/*
 * Copyright 2010-2016 Bastian Eicher
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
using FluentAssertions;
using Moq;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NUnit.Framework;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Services.Executors;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Contains integration tests for <see cref="Run"/>.
    /// </summary>
    [TestFixture]
    public class RunTest : SelectionTestBase<Run>
    {
        private Mock<ICatalogManager> CatalogManagerMock => GetMock<ICatalogManager>();

        [Test(Description = "Ensures all options are parsed and handled correctly.")]
        public void TestNormal()
        {
            var selections = ExpectSolve();

            ExpectFetchUncached(selections,
                new Implementation {ID = "id1", ManifestDigest = new ManifestDigest(sha256: "abc"), Version = new ImplementationVersion("1.0")},
                new Implementation {ID = "id2", ManifestDigest = new ManifestDigest(sha256: "xyz"), Version = new ImplementationVersion("1.0")});

            var envBuilderMock = GetMock<IEnvironmentBuilder>();
            GetMock<IExecutor>().Setup(x => x.Inject(selections, "Main")).Returns(envBuilderMock.Object);
            envBuilderMock.Setup(x => x.AddWrapper("Wrapper")).Returns(envBuilderMock.Object);
            envBuilderMock.Setup(x => x.AddArguments("--arg1", "--arg2")).Returns(envBuilderMock.Object);
            envBuilderMock.Setup(x => x.Start()).Returns<Process>(null);

            RunAndAssert(null, 0, selections,
                "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0",
                "--main=Main", "--wrapper=Wrapper", "http://0install.de/feeds/test/test1.xml", "--arg1", "--arg2");
        }

        [Test(Description = "Ensures local Selections XMLs are correctly detected and parsed.")]
        public void TestImportSelections()
        {
            var selections = SelectionsTest.CreateTestSelections();

            ExpectFetchUncached(selections,
                new Implementation {ID = "id1", ManifestDigest = new ManifestDigest(sha256: "abc"), Version = new ImplementationVersion("1.0")},
                new Implementation {ID = "id2", ManifestDigest = new ManifestDigest(sha256: "xyz"), Version = new ImplementationVersion("1.0")});

            var envBuilderMock = GetMock<IEnvironmentBuilder>();
            GetMock<IExecutor>().Setup(x => x.Inject(selections, null)).Returns(envBuilderMock.Object);
            envBuilderMock.Setup(x => x.AddWrapper(null)).Returns(envBuilderMock.Object);
            envBuilderMock.Setup(x => x.AddArguments("--arg1", "--arg2")).Returns(envBuilderMock.Object);
            envBuilderMock.Setup(x => x.Start()).Returns<Process>(null);

            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.SaveXml(tempFile);

                selections.Normalize();
                RunAndAssert(null, 0, selections, tempFile, "--arg1", "--arg2");
            }
        }

        [Ignore("Not applicable")]
        public override void TestTooManyArgs()
        {}

        [Test]
        public void TestGetCanonicalUriRemote()
        {
            Target.GetCanonicalUri("http://0install.de/feeds/test/test1.xml").ToStringRfc()
                .Should().Be("http://0install.de/feeds/test/test1.xml");
        }

        [Test]
        public void TestGetCanonicalUriFile()
        {
            CatalogManagerMock.Setup(x => x.GetCached()).Returns(new Catalog());
            CatalogManagerMock.Setup(x => x.GetOnline()).Returns(new Catalog());

            // Absolute paths
            if (WindowsUtils.IsWindows)
            {
                Target.GetCanonicalUri(@"C:\test\file").ToStringRfc().Should().Be(@"C:\test\file");
                Target.GetCanonicalUri(@"file:///C:\test\file").ToStringRfc().Should().Be(@"C:\test\file");
                Target.GetCanonicalUri("file:///C:/test/file").ToStringRfc().Should().Be(@"C:\test\file");
            }
            if (UnixUtils.IsUnix)
            {
                Target.GetCanonicalUri("/test/file").ToStringRfc().Should().Be("/test/file");
                Target.GetCanonicalUri("file:///test/file").ToStringRfc().Should().Be("/test/file");
            }

            // Relative paths
            Target.GetCanonicalUri(Path.Combine("test", "file")).ToString().Should().Be(
                Path.Combine(Environment.CurrentDirectory, "test", "file"));
            Target.GetCanonicalUri("file:test/file").ToString().Should().Be(
                Path.Combine(Environment.CurrentDirectory, "test", "file"));

            // Invalid paths
            Target.Invoking(x => x.GetCanonicalUri("file:/test/file")).ShouldThrow<UriFormatException>();
            if (WindowsUtils.IsWindows) Target.Invoking(x => x.GetCanonicalUri(":::")).ShouldThrow<UriFormatException>();
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

            Target.GetCanonicalUri("alias:test").Should().Be(FeedTest.Test1Uri);
            Target.Invoking(x => x.GetCanonicalUri("alias:invalid")).ShouldThrow<UriFormatException>();
        }

        [Test]
        public void TestGetCanonicalUriCatalogCached()
        {
            CatalogManagerMock.Setup(x => x.GetCached()).Returns(new Catalog {Feeds = {new Feed {Uri = FeedTest.Test1Uri, Name = "MyApp"}}});
            Target.GetCanonicalUri("MyApp").Should().Be(FeedTest.Test1Uri);
        }

        [Test]
        public void TestGetCanonicalUriCatalogOnline()
        {
            CatalogManagerMock.Setup(x => x.GetCached()).Returns(new Catalog());
            CatalogManagerMock.Setup(x => x.GetOnline()).Returns(new Catalog {Feeds = {new Feed {Uri = FeedTest.Test1Uri, Name = "MyApp"}}});
            Target.GetCanonicalUri("MyApp").Should().Be(FeedTest.Test1Uri);
        }
    }
}
