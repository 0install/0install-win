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

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace ZeroInstall.Model
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
        /// Ensures that <see cref="Catalog.GetFeed"/> correctly finds contained <see cref="Feed"/>s.
        /// </summary>
        [Test]
        public void TestGetFeed()
        {
            var catalog = CreateTestCatalog();

            Assert.AreEqual(FeedTest.CreateTestFeed(), catalog.GetFeed(new Uri("http://somedomain/someapp.xml")));
            Assert.Throws<KeyNotFoundException>(() => catalog.GetFeed(new Uri("http://invalid")));
        }

        /// <summary>
        /// Ensures that the class is correctly serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoad()
        {
            Catalog catalog1, catalog2;
            string tempFile = null;
            try
            {
                tempFile = Path.GetTempFileName();

                // Write and read file
                catalog1 = CreateTestCatalog();
                catalog1.Save(tempFile);
                catalog2 = Catalog.Load(tempFile);
            }
            finally
            { // Clean up
                if (tempFile != null) File.Delete(tempFile);
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
            var catalog2 = catalog1.CloneCatalog();

            // Ensure data stayed the same
            Assert.AreEqual(catalog1, catalog2, "Cloned objects should be equal.");
            Assert.AreEqual(catalog1.GetHashCode(), catalog2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(catalog1, catalog2), "Cloning should not return the same reference.");
        }

        /// <summary>
        /// Ensures that <see cref="Catalog.Simplify"/> correctly removes unneeded information from <see cref="Feed"/>s.
        /// </summary>
        [Test]
        public void TestSimplify()
        {
            // ToDo: Implement
        }
    }
}
