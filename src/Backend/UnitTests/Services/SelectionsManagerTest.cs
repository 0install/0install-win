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

using Moq;
using NUnit.Framework;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
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
        private Mock<IFeedCache> _feedCacheMock;
        private Mock<IStore> _storeMock;

        protected override void Register(AutoMockContainer container)
        {
            _feedCacheMock = container.GetMock<IFeedCache>();
            _storeMock = container.GetMock<IStore>();

            base.Register(container);
        }

        [Test]
        public void TestGetUncachedImplementationSelections()
        {
            var selections = SelectionsTest.CreateTestSelections();
            selections.Implementations.Add(new ImplementationSelection {InterfaceUri = new FeedUri("http://0install.de/feeds/test/dummy.xml")});

            // Pretend the first implementation isn't cached, the second is and the third isn't
            _storeMock.Setup(x => x.Contains(selections.Implementations[0].ManifestDigest)).Returns(false);
            _storeMock.Setup(x => x.Contains(selections.Implementations[1].ManifestDigest)).Returns(true);
            _storeMock.Setup(x => x.Contains(default(ManifestDigest))).Returns(false);

            var implementationSelections = Target.GetUncachedImplementationSelections(selections);

            // Only the first implementation should be listed as uncached
            CollectionAssert.AreEquivalent(new[] {selections.Implementations[0]}, implementationSelections);
        }

        [Test]
        public void TestGetOriginalImplementations()
        {
            var impl1 = new Implementation {ID = "test123"};
            var impl2 = new Implementation {ID = "test456"};
            var implementationSelections = new[]
            {
                new ImplementationSelection {ID = impl1.ID, InterfaceUri = FeedTest.Test1Uri},
                new ImplementationSelection {ID = impl2.ID, InterfaceUri = FeedTest.Test2Uri, FromFeed = FeedTest.Sub2Uri}
            };

            _feedCacheMock.Setup(x => x.GetFeed(FeedTest.Test1Uri)).Returns(new Feed {Elements = {impl1}});
            _feedCacheMock.Setup(x => x.GetFeed(FeedTest.Sub2Uri)).Returns(new Feed {Elements = {impl2}});

            var implementations = Target.GetOriginalImplementations(implementationSelections);

            CollectionAssert.AreEquivalent(new[] {impl1, impl2}, implementations);
        }
    }
}
