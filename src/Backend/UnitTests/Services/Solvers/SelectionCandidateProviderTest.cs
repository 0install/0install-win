/*
 * Copyright 2010-2015 Bastian Eicher
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

using System.Linq;
using Moq;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using NUnit.Framework;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Runs test methods for <see cref="SelectionCandidateProvider"/>.
    /// </summary>
    [TestFixture]
    public class SelectionCandidateProviderTest : TestWithContainer<SelectionCandidateProvider>
    {
        private Mock<IFeedManager> _feedManagerMock;
        private Mock<IPackageManager> _packageManagerMock;

        protected override void Register(AutoMockContainer container)
        {
            _feedManagerMock = container.GetMock<IFeedManager>();
            _packageManagerMock = container.GetMock<IPackageManager>();

            base.Register(container);
        }

        [Test]
        public void TestGetSortedCandidates()
        {
            var mainFeed = FeedTest.CreateTestFeed();
            mainFeed.Feeds.Clear();
            _feedManagerMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(mainFeed);
            _packageManagerMock.Setup(x => x.Query((PackageImplementation)mainFeed.Elements[1])).Returns(Enumerable.Empty<ExternalImplementation>());

            var requirements = new Requirements(FeedTest.Test1Uri, Command.NameRun);
            CollectionAssert.AreEqual(
                expected: new[]
                {
                    new SelectionCandidate(FeedTest.Test1Uri, new FeedPreferences(), (Implementation)mainFeed.Elements[0], requirements)
                },
                actual: Target.GetSortedCandidates(requirements));
        }

        [Test]
        public void TestFeedReferences()
        {
            var mainFeed = FeedTest.CreateTestFeed();
            _feedManagerMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(mainFeed);
            _packageManagerMock.Setup(x => x.Query((PackageImplementation)mainFeed.Elements[1])).Returns(Enumerable.Empty<ExternalImplementation>());

            var subFeed = mainFeed.Clone();
            subFeed.Uri = FeedTest.Sub1Uri;
            subFeed.Elements[0].Version = new ImplementationVersion("2.0");
            _feedManagerMock.Setup(x => x.GetFeed(FeedTest.Sub1Uri)).Returns(subFeed);

            var requirements = new Requirements(FeedTest.Test1Uri, Command.NameRun);
            CollectionAssert.AreEqual(
                expected: new[]
                {
                    new SelectionCandidate(FeedTest.Sub1Uri, new FeedPreferences(), (Implementation)subFeed.Elements[0], requirements),
                    new SelectionCandidate(FeedTest.Test1Uri, new FeedPreferences(), (Implementation)mainFeed.Elements[0], requirements)
                },
                actual: Target.GetSortedCandidates(requirements));
        }

        [Test]
        public void TestInterfacePreferences()
        {
            var mainFeed = FeedTest.CreateTestFeed();
            mainFeed.Elements.RemoveAt(1);
            mainFeed.Feeds.Clear();
            _feedManagerMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(mainFeed);

            new InterfacePreferences {Feeds = {new FeedReference {Source = FeedTest.Sub1Uri}}}.SaveFor(mainFeed.Uri);

            var subFeed = mainFeed.Clone();
            subFeed.Uri = FeedTest.Sub1Uri;
            subFeed.Elements[0].Version = new ImplementationVersion("2.0");
            _feedManagerMock.Setup(x => x.GetFeed(FeedTest.Sub1Uri)).Returns(subFeed);

            var requirements = new Requirements(FeedTest.Test1Uri, Command.NameRun);
            CollectionAssert.AreEqual(
                expected: new[]
                {
                    new SelectionCandidate(FeedTest.Sub1Uri, new FeedPreferences(), (Implementation)subFeed.Elements[0], requirements),
                    new SelectionCandidate(FeedTest.Test1Uri, new FeedPreferences(), (Implementation)mainFeed.Elements[0], requirements)
                },
                actual: Target.GetSortedCandidates(requirements));
        }

        [Test]
        public void TestNativeFeed()
        {
            var mainFeed = FeedTest.CreateTestFeed();
            mainFeed.Elements.RemoveAt(1);
            mainFeed.Feeds.Clear();
            _feedManagerMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(mainFeed);

            using (new LocationsRedirect("0install-unit-tests"))
            {
                var localUri = new FeedUri(Locations.GetSaveDataPath("0install.net", true, "native_feeds", mainFeed.Uri.PrettyEscape()));

                var subFeed = mainFeed.Clone();
                subFeed.Uri = FeedTest.Sub1Uri;
                subFeed.Elements[0].Version = new ImplementationVersion("2.0");
                subFeed.SaveXml(localUri.LocalPath);
                _feedManagerMock.Setup(x => x.GetFeed(localUri)).Returns(subFeed);

                var requirements = new Requirements(FeedTest.Test1Uri, Command.NameRun);
                CollectionAssert.AreEqual(
                    expected: new[]
                    {
                        new SelectionCandidate(localUri, new FeedPreferences(), (Implementation)subFeed.Elements[0], requirements),
                        new SelectionCandidate(FeedTest.Test1Uri, new FeedPreferences(), (Implementation)mainFeed.Elements[0], requirements)
                    },
                    actual: Target.GetSortedCandidates(requirements));
            }
        }

        [Test]
        public void TestSitePackages()
        {
            var mainFeed = FeedTest.CreateTestFeed();
            mainFeed.Feeds.Clear();
            _feedManagerMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(mainFeed);

            using (new LocationsRedirect("0install-unit-tests"))
            {
                var pathComponents = mainFeed.Uri.EscapeComponent()
                    .Prepend("site-packages")
                    .Concat(new[] {"xyz", "0install", "feed.xml"});
                var localUri = new FeedUri(Locations.GetSaveDataPath("0install.net", isFile: true, resource: pathComponents.ToArray()));

                var subFeed = mainFeed.Clone();
                subFeed.Uri = FeedTest.Sub1Uri;
                subFeed.Elements[0].Version = new ImplementationVersion("2.0");
                subFeed.SaveXml(localUri.LocalPath);
                _feedManagerMock.Setup(x => x.GetFeed(localUri)).Returns(subFeed);
                _packageManagerMock.Setup(x => x.Query((PackageImplementation)mainFeed.Elements[1])).Returns(Enumerable.Empty<ExternalImplementation>());

                var requirements = new Requirements(FeedTest.Test1Uri, Command.NameRun);
                CollectionAssert.AreEqual(
                    expected: new[]
                    {
                        new SelectionCandidate(localUri, new FeedPreferences(), (Implementation)subFeed.Elements[0], requirements),
                        new SelectionCandidate(FeedTest.Test1Uri, new FeedPreferences(), (Implementation)mainFeed.Elements[0], requirements)
                    },
                    actual: Target.GetSortedCandidates(requirements));
            }
        }

        [Test]
        public void TestPackageManager()
        {
            var mainFeed = FeedTest.CreateTestFeed();
            mainFeed.Feeds.Clear();
            _feedManagerMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(mainFeed);

            var nativeImplementation = new ExternalImplementation("rpm", "firefox", new ImplementationVersion("1.0")) {Languages = {"en-US"}};
            _packageManagerMock.Setup(x => x.Query((PackageImplementation)mainFeed.Elements[1])).Returns(new[] {nativeImplementation});

            var requirements = new Requirements(FeedTest.Test1Uri, Command.NameRun);

            CollectionAssert.AreEqual(
                expected: new[]
                {
                    new SelectionCandidate(new FeedUri(FeedUri.FromDistributionPrefix + FeedTest.Test1Uri), new FeedPreferences(), nativeImplementation, requirements),
                    new SelectionCandidate(FeedTest.Test1Uri, new FeedPreferences(), (Implementation)mainFeed.Elements[0], requirements),
                },
                actual: Target.GetSortedCandidates(requirements));
        }

        [Test]
        public void TestLookupOriginalImplementation()
        {
            var mainFeed = FeedTest.CreateTestFeed();
            mainFeed.Feeds.Clear();
            _feedManagerMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(mainFeed);
            _packageManagerMock.Setup(x => x.Query((PackageImplementation)mainFeed.Elements[1])).Returns(Enumerable.Empty<ExternalImplementation>());

            var requirements = new Requirements(FeedTest.Test1Uri, Command.NameRun);
            var candidates = Target.GetSortedCandidates(requirements);
            var candidate = candidates.Single().ToSelection(candidates, requirements);

            Assert.AreEqual(
                expected: mainFeed.Elements[0],
                actual: Target.LookupOriginalImplementation(candidate));
        }
    }
}
