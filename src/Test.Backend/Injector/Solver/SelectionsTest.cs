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

using NUnit.Framework;
using Moq;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Contains test methods for <see cref="Selections"/>.
    /// </summary>
    [TestFixture]
    public class SelectionsTest
    {
        #region Helpers
        /// <summary>
        /// Creates a <see cref="Selections"/> with two implementations, one using the other as a runner plus a number of bindings.
        /// </summary>
        public static Selections CreateTestSelections()
        {
            return new Selections
            {
                InterfaceID = "http://0install.de/feeds/test/test1.xml", CommandName = Command.NameRun,
                Implementations =
                {
                    ImplementationSelectionTest.CreateTestImplementation1(),
                    ImplementationSelectionTest.CreateTestImplementation2()
                }
            };
        }
        #endregion

        [Test(Description = "Ensures that Selections.GetUncachedImplementations() correctly finds Implementations not cached in a store")]
        public void TestGetUncachedImplementations()
        {
            var feed = FeedTest.CreateTestFeed();
            var selections = CreateTestSelections();
            selections.Implementations.Add(new ImplementationSelection {InterfaceID = "http://0install.de/feeds/test/dummy.xml"});

            // Return the generated feed
            var cacheMock = new Mock<IFeedCache>(MockBehavior.Strict);
            cacheMock.Setup(x => x.GetFeed("http://0install.de/feeds/test/sub1.xml")).Returns(feed).Verifiable();

            // Pretend the first implementation isn't cached, the second is and the third isn't
            var storeMock = new Mock<IStore>(MockBehavior.Strict);
            storeMock.Setup(x => x.Contains(selections.Implementations[0].ManifestDigest)).Returns(false).Verifiable();
            storeMock.Setup(x => x.Contains(selections.Implementations[1].ManifestDigest)).Returns(true).Verifiable();
            storeMock.Setup(x => x.Contains(default(ManifestDigest))).Returns(false).Verifiable();

            var implementations = selections.GetUncachedImplementations(storeMock.Object, cacheMock.Object);

            // Only the first implementation should be listed as uncached
            CollectionAssert.AreEquivalent(new[] {feed.Elements[0]}, implementations);

            cacheMock.Verify();
            storeMock.Verify();
        }
    }
}
