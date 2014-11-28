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

using System.Collections.Generic;
using NanoByte.Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="Catalog"/>.
    /// </summary>
    [TestFixture]
    public class CatalogTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="Catalog"/>.
        /// </summary>
        public static Catalog CreateTestCatalog()
        {
            return new Catalog {Feeds = {FeedTest.CreateTestFeed()}};
        }
        #endregion

        /// <summary>
        /// Ensures that <see cref="Catalog.Merge"/> correctly combines <see cref="Feed"/>s from multiple <see cref="Catalog"/>s and filters out duplicates.
        /// </summary>
        [Test]
        public void TestMerge()
        {
            var feed1 = new Feed {Uri = new Uri("http://0install.de/feeds/test/test1.xml")};
            var feed2 = new Feed {Uri = new Uri("http://0install.de/feeds/test/test2.xml")};
            var feed3 = new Feed {Uri = new Uri("http://0install.de/feeds/test/test3.xml")};
            var catalog1 = new Catalog {Feeds = {feed1, feed2}};
            var catalog2 = new Catalog {Feeds = {feed2, feed3}};

            var mergedCatalog = Catalog.Merge(new[] {catalog1, catalog2});
            CollectionAssert.IsSubsetOf(catalog1.Feeds, mergedCatalog.Feeds);
            CollectionAssert.IsSubsetOf(catalog2.Feeds, mergedCatalog.Feeds);
            CollectionAssert.AreEqual(new[] {feed1, feed2, feed3}, mergedCatalog.Feeds);
        }

        /// <summary>
        /// Ensures that <see cref="Catalog.GetFeed"/> and <see cref="Catalog.this"/> correctly find contained <see cref="Feed"/>s.
        /// </summary>
        [Test]
        public void TestGetFeed()
        {
            var catalog = CreateTestCatalog();

            Assert.AreEqual(FeedTest.CreateTestFeed(), catalog.GetFeed(new Uri("http://0install.de/feeds/test/test1.xml")));
            Assert.AreEqual(FeedTest.CreateTestFeed(), catalog[new Uri("http://0install.de/feeds/test/test1.xml")]);

            Assert.IsNull(catalog.GetFeed(new Uri("http://invalid")));
            // ReSharper disable UnusedVariable
            Assert.Throws<KeyNotFoundException>(() => { var dummy = catalog[new Uri("http://invalid")]; });
            // ReSharper restore UnusedVariable
        }

        /// <summary>
        /// Ensures that the class is correctly serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoad()
        {
            Catalog catalog1 = CreateTestCatalog(), catalog2;
            Assert.That(catalog1, Is.XmlSerializable);
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // Write and read file
                catalog1.SaveXml(tempFile);
                catalog2 = XmlStorage.LoadXml<Catalog>(tempFile);
            }

            // Ensure data stayed the same
            Assert.AreEqual(catalog1, catalog2, "Serialized objects should be equal.");
            Assert.AreEqual(catalog1.GetHashCode(), catalog2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(catalog1, catalog2), "Serialized objects should not return the same reference.");
        }

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var catalog1 = CreateTestCatalog();
            var catalog2 = catalog1.Clone();

            // Ensure data stayed the same
            Assert.AreEqual(catalog1, catalog2, "Cloned objects should be equal.");
            Assert.AreEqual(catalog1.GetHashCode(), catalog2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(catalog1, catalog2), "Cloning should not return the same reference.");
        }
    }
}
