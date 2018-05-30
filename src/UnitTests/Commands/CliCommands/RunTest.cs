// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Diagnostics;
using System.IO;
using FluentAssertions;
using Moq;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using Xunit;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Services.Executors;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Contains integration tests for <see cref="Run"/>.
    /// </summary>
    public class RunTest : SelectionTestBase<Run>
    {
        private Mock<ICatalogManager> CatalogManagerMock => GetMock<ICatalogManager>();

        [Fact] // Ensures all options are parsed and handled correctly.
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
            envBuilderMock.Setup(x => x.Start()).Returns((Process)null);

            RunAndAssert(null, 0, selections,
                "--command=command", "--os=Windows", "--cpu=i586", "--not-before=1.0", "--before=2.0", "--version-for=http://0install.de/feeds/test/test2.xml", "2.0..!3.0",
                "--main=Main", "--wrapper=Wrapper", "http://0install.de/feeds/test/test1.xml", "--arg1", "--arg2");
        }

        [Fact] // Ensures local Selections XMLs are correctly detected and parsed.
        public void TestImportSelections()
        {
            var selections = Fake.Selections;

            ExpectFetchUncached(selections,
                new Implementation {ID = "id1", ManifestDigest = new ManifestDigest(sha256: "abc"), Version = new ImplementationVersion("1.0")},
                new Implementation {ID = "id2", ManifestDigest = new ManifestDigest(sha256: "xyz"), Version = new ImplementationVersion("1.0")});

            var envBuilderMock = GetMock<IEnvironmentBuilder>();
            GetMock<IExecutor>().Setup(x => x.Inject(selections, null)).Returns(envBuilderMock.Object);
            envBuilderMock.Setup(x => x.AddWrapper(null)).Returns(envBuilderMock.Object);
            envBuilderMock.Setup(x => x.AddArguments("--arg1", "--arg2")).Returns(envBuilderMock.Object);
            envBuilderMock.Setup(x => x.Start()).Returns((Process)null);

            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                selections.SaveXml(tempFile);

                selections.Normalize();
                RunAndAssert(null, 0, selections, tempFile, "--arg1", "--arg2");
            }
        }

        public override void ShouldRejectTooManyArgs()
        {
            // Not applicable
        }

        [Fact]
        public void TestGetCanonicalUriRemote()
            => Sut.GetCanonicalUri("http://0install.de/feeds/test/test1.xml")
                  .ToStringRfc()
                  .Should().Be("http://0install.de/feeds/test/test1.xml");

        [Fact]
        public void TestGetCanonicalUriFile()
        {
            CatalogManagerMock.Setup(x => x.GetCached()).Returns(new Catalog());
            CatalogManagerMock.Setup(x => x.GetOnline()).Returns(new Catalog());

            // Absolute paths
            if (WindowsUtils.IsWindows)
            {
                Sut.GetCanonicalUri(@"C:\test\file").ToStringRfc().Should().Be(@"C:\test\file");
                Sut.GetCanonicalUri(@"file:///C:\test\file").ToStringRfc().Should().Be(@"C:\test\file");
                Sut.GetCanonicalUri("file:///C:/test/file").ToStringRfc().Should().Be(@"C:\test\file");
            }
            if (UnixUtils.IsUnix)
            {
                Sut.GetCanonicalUri("/test/file").ToStringRfc().Should().Be("/test/file");
                Sut.GetCanonicalUri("file:///test/file").ToStringRfc().Should().Be("/test/file");
            }

            // Relative paths
            Sut.GetCanonicalUri(Path.Combine("test", "file")).ToString().Should().Be(
                Path.Combine(Environment.CurrentDirectory, "test", "file"));
            Sut.GetCanonicalUri("file:test/file").ToString().Should().Be(
                Path.Combine(Environment.CurrentDirectory, "test", "file"));

            // Invalid paths
            Assert.Throws<UriFormatException>(() => Sut.GetCanonicalUri("file:/test/file"));
            if (WindowsUtils.IsWindows) Assert.Throws<UriFormatException>(() => Sut.GetCanonicalUri(":::"));
        }

        [Fact]
        public void TestGetCanonicalUriAliases()
        {
            // Fake an alias
            new AppList
            {
                Entries =
                {
                    new AppEntry
                    {
                        InterfaceUri = Fake.Feed1Uri,
                        AccessPoints = new AccessPointList {Entries = {new AppAlias {Name = "test"}}}
                    }
                }
            }.SaveXml(AppList.GetDefaultPath());

            Sut.GetCanonicalUri("alias:test").Should().Be(Fake.Feed1Uri);
            Assert.Throws<UriFormatException>(() => Sut.GetCanonicalUri("alias:invalid"));
        }

        [Fact]
        public void TestGetCanonicalUriCatalogCached()
        {
            CatalogManagerMock.Setup(x => x.GetCached()).Returns(new Catalog {Feeds = {new Feed {Uri = Fake.Feed1Uri, Name = "MyApp"}}});
            Sut.GetCanonicalUri("MyApp").Should().Be(Fake.Feed1Uri);
        }

        [Fact]
        public void TestGetCanonicalUriCatalogOnline()
        {
            CatalogManagerMock.Setup(x => x.GetCached()).Returns(new Catalog());
            CatalogManagerMock.Setup(x => x.GetOnline()).Returns(new Catalog {Feeds = {new Feed {Uri = Fake.Feed1Uri, Name = "MyApp"}}});
            Sut.GetCanonicalUri("MyApp").Should().Be(Fake.Feed1Uri);
        }
    }
}
