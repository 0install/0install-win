/*
 * Copyright 2010 Bastian Eicher
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
                InterfaceID = "http://0install.de/feeds/test/test1.xml", Command = Command.NameRun,
                Implementations =
                    {
                        ImplementationSelectionTest.CreateTestImplementation1(),
                        ImplementationSelectionTest.CreateTestImplementation2()
                    }
            };
        }
        #endregion

        [Test(Description = "Ensures that the class is correctly serialized and deserialized.")]
        public void TestSaveLoad()
        {
            var selections1 = CreateTestSelections();

            // Serialize and deserialize data
            string data = selections1.WriteToString();
            var selections2 = Selections.LoadFromString(data);

            // Ensure data stayed the same
            Assert.AreEqual(selections1, selections2, "Serialized objects should be equal.");
            Assert.AreEqual(selections1.GetHashCode(), selections2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(selections1, selections2), "Serialized objects should not return the same reference.");
        }

        [Test(Description = "Ensures that the class can be correctly cloned.")]
        public void TestClone()
        {
            var selections1 = CreateTestSelections();
            var selections2 = selections1.CloneSelections();

            // Ensure data stayed the same
            Assert.AreEqual(selections1, selections2, "Cloned objects should be equal.");
            Assert.AreEqual(selections1.GetHashCode(), selections2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(selections1, selections2), "Cloning should not return the same reference.");
        }

        [Test(Description = "Ensures that Selections.ListUncachedImplementations() correctly finds Implementations not cached in a store")]
        public void TestListUncachedImplementations()
        {
            var feed = FeedTest.CreateTestFeed();
            var selections = CreateTestSelections();
            selections.Implementations.Add(new ImplementationSelection {InterfaceID = "http://0install.de/feeds/test/dummy.xml"});

            // Return the generated feed and an empty one (close enough to a point to PackageImplementations)
            var cacheMock = new Mock<IFeedCache>(MockBehavior.Strict);
            cacheMock.Setup(x => x.GetFeed("http://0install.de/feeds/test/sub1.xml")).Returns(feed);
            cacheMock.Setup(x => x.GetFeed("http://0install.de/feeds/test/dummy.xml")).Returns(new Feed());

            // Pretend the first implementation isn't cached, the second is and the third isn't
            var storeMock = new Mock<IStore>(MockBehavior.Strict);
            storeMock.Setup(x => x.Contains(selections.Implementations[0].ManifestDigest)).Returns(false);
            storeMock.Setup(x => x.Contains(selections.Implementations[1].ManifestDigest)).Returns(true);
            storeMock.Setup(x => x.Contains(default(ManifestDigest))).Returns(false);

            var implementations = selections.ListUncachedImplementations(storeMock.Object, cacheMock.Object);

            // Only the first implementation should be listed as uncached
            CollectionAssert.AreEquivalent(new[] {feed.Elements[0]}, implementations);

            cacheMock.Verify();
            storeMock.Verify();
        }
    }
}
