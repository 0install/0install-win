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

using FluentAssertions;
using Moq;
using NanoByte.Common.Storage;
using NUnit.Framework;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services
{
    /// <summary>
    /// Contains test methods for <see cref="SelectionsManager"/>.
    /// </summary>
    [TestFixture]
    public class SelectionsManagerTest : TestWithContainer<SelectionsManager>
    {
        private Mock<IFeedManager> FeedManagereMock { get { return GetMock<IFeedManager>(); } }
        private Mock<IPackageManager> PackageManagerMock { get { return GetMock<IPackageManager>(); } }
        private Mock<IStore> StoreMock { get { return GetMock<IStore>(); } }

        [Test]
        public void TestGetUncachedSelections()
        {
            var selections = SelectionsTest.CreateTestSelections();

            StoreMock.Setup(x => x.Contains(selections.Implementations[0].ManifestDigest)).Returns(false);
            StoreMock.Setup(x => x.Contains(selections.Implementations[1].ManifestDigest)).Returns(true);

            var implementationSelections = Target.GetUncachedSelections(selections);

            implementationSelections.Should().BeEquivalentTo(new[] {selections.Implementations[0]}, because: "Only the first implementation should be listed as uncached");
        }

        [Test]
        public void TestGetUncachedSelectionsPackageManager()
        {
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                var impl1 = new ExternalImplementation("RPM", "firefox", new ImplementationVersion("1.0")) {IsInstalled = false};
                var impl2 = new ExternalImplementation("RPM", "thunderbird", new ImplementationVersion("1.0")) {IsInstalled = true};
                var impl3 = new ExternalImplementation("RPM", "vlc", new ImplementationVersion("1.0")) {IsInstalled = true, QuickTestFile = tempFile};

                var selections = new Selections
                {
                    InterfaceUri = FeedTest.Test1Uri,
                    Command = Command.NameRun,
                    Implementations =
                    {
                        new ImplementationSelection {InterfaceUri = FeedTest.Test1Uri, FromFeed = new FeedUri(FeedUri.FromDistributionPrefix + FeedTest.Test1Uri), ID = impl1.ID, QuickTestFile = impl1.QuickTestFile},
                        new ImplementationSelection {InterfaceUri = FeedTest.Test1Uri, FromFeed = new FeedUri(FeedUri.FromDistributionPrefix + FeedTest.Test1Uri), ID = impl2.ID, QuickTestFile = impl2.QuickTestFile},
                        new ImplementationSelection {InterfaceUri = FeedTest.Test1Uri, FromFeed = new FeedUri(FeedUri.FromDistributionPrefix + FeedTest.Test1Uri), ID = impl3.ID, QuickTestFile = impl3.QuickTestFile}
                    }
                };

                PackageManagerMock.Setup(x => x.Lookup(selections.Implementations[0])).Returns(impl1);
                PackageManagerMock.Setup(x => x.Lookup(selections.Implementations[1])).Returns(impl2);

                var implementationSelections = Target.GetUncachedSelections(selections);

                // Only the first implementation should be listed as uncached
                implementationSelections.Should().BeEquivalentTo(new[] {selections.Implementations[0]}, because: "Only the first implementation should be listed as uncached");
            }
        }

        [Test]
        public void TestGetImplementations()
        {
            var impl1 = new Implementation {ID = "test123"};
            var impl2 = new Implementation {ID = "test456"};
            var impl3 = new ExternalImplementation("RPM", "firefox", new ImplementationVersion("1.0"))
            {
                RetrievalMethods = {new ExternalRetrievalMethod {PackageID = "test"}}
            };
            var implementationSelections = new[]
            {
                new ImplementationSelection {ID = impl1.ID, InterfaceUri = FeedTest.Test1Uri},
                new ImplementationSelection {ID = impl2.ID, InterfaceUri = FeedTest.Test2Uri, FromFeed = FeedTest.Sub2Uri},
                new ImplementationSelection {ID = impl3.ID, InterfaceUri = FeedTest.Test1Uri, FromFeed = new FeedUri(FeedUri.FromDistributionPrefix + FeedTest.Test1Uri)}
            };

            FeedManagereMock.Setup(x => x[FeedTest.Test1Uri]).Returns(new Feed {Elements = {impl1}});
            FeedManagereMock.Setup(x => x[FeedTest.Sub2Uri]).Returns(new Feed {Elements = {impl2}});
            PackageManagerMock.Setup(x => x.Lookup(implementationSelections[2])).Returns(impl3);

            Target.GetImplementations(implementationSelections)
                .Should().BeEquivalentTo(impl1, impl2, impl3);
        }
    }
}
