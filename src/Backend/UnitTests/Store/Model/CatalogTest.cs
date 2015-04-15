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
        /// Ensures that <see cref="Catalog.GetFeed"/> and <see cref="Catalog.this"/> correctly find contained <see cref="Feed"/>s.
        /// </summary>
        [Test]
        public void TestGetFeed()
        {
            var catalog = CreateTestCatalog();

            Assert.AreEqual(FeedTest.CreateTestFeed(), catalog.GetFeed(FeedTest.Test1Uri));
            Assert.AreEqual(FeedTest.CreateTestFeed(), catalog[FeedTest.Test1Uri]);

            Assert.IsNull(catalog.GetFeed(new FeedUri("http://invalid/")));
            // ReSharper disable once UnusedVariable
            Assert.Throws<KeyNotFoundException>(() => { var dummy = catalog[new FeedUri("http://invalid/")]; });
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

        /// <summary>
        /// Ensures that <see cref="Catalog.FindByShortName"/> works correctly.
        /// </summary>
        [Test]
        public void TestFindByShortName()
        {
            var appA = new Feed
            {
                Uri = FeedTest.Test1Uri, Name = "AppA",
                EntryPoints = {new EntryPoint {Command = Command.NameRun, BinaryName = "BinaryA"}}
            };
            var appB = new Feed
            {
                Uri = FeedTest.Test2Uri, Name = "AppB",
                EntryPoints = {new EntryPoint {Command = Command.NameRun, BinaryName = "BinaryB"}}
            };
            var catalog = new Catalog {Feeds = {appA, appA.Clone(), appB, appB.Clone()}};

            Assert.IsNull(catalog.FindByShortName(""));
            Assert.AreSame(expected: appA, actual: catalog.FindByShortName("AppA"));
            Assert.AreSame(expected: appA, actual: catalog.FindByShortName("BinaryA"));
            Assert.AreSame(expected: appB, actual: catalog.FindByShortName("AppB"));
            Assert.AreSame(expected: appB, actual: catalog.FindByShortName("BinaryB"));
        }

        /// <summary>
        /// Ensures that <see cref="Catalog.Search"/> works correctly.
        /// </summary>
        [Test]
        public void TestSearch()
        {
            var appA = new Feed {Uri = FeedTest.Test1Uri, Name = "AppA"};
            var appB = new Feed {Uri = FeedTest.Test2Uri, Name = "AppB"};
            var lib = new Feed {Uri = FeedTest.Test3Uri, Name = "Lib"};
            var catalog = new Catalog {Feeds = {appA, appB, lib}};

            CollectionAssert.AreEqual(expected: new[] {appA, appB, lib}, actual: catalog.Search(""));
            CollectionAssert.AreEqual(expected: new[] {appA, appB}, actual: catalog.Search("App"));
            CollectionAssert.AreEqual(expected: new[] {appA}, actual: catalog.Search("AppA"));
            CollectionAssert.AreEqual(expected: new[] {appB}, actual: catalog.Search("AppB"));
        }
    }
}
