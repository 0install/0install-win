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
using Common.Storage;
using NUnit.Framework;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Interface;

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
        /// Creates a <see cref="Selections"/> with a fictive test <see cref="ImplementationSelection"/>.
        /// </summary>
        private static Selections CreateTestSelections()
        {
            return new Selections
            {
                Interface = new Uri("http://0install.de/feeds/test.xml"),
                Implementations = { ImplementationSelectionTest.CreateTestImplementation() }
            };
        }
        #endregion

        /// <summary>
        /// Ensures that the class is correctly serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoad()
        {
            var selections1 = CreateTestSelections();
            Selections selections2;
            string tempFile = null;

            try
            {
                tempFile = Path.GetTempFileName();

                // Write and read file
                selections1.Save(tempFile);
                selections2 = Selections.Load(tempFile);
            }
            finally
            { // Clean up
                if (tempFile != null) File.Delete(tempFile);
            }

            // Ensure data stayed the same
            Assert.AreEqual(selections1, selections2, "Serialized objects should be equal.");
            Assert.AreEqual(selections1.GetHashCode(), selections2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(selections1, selections2), "Serialized should not return the same reference.");
        }

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var selections1 = CreateTestSelections();
            var selections2 = selections1.CloneSelections();

            // Ensure data stayed the same
            Assert.AreEqual(selections1, selections2, "Cloned objects should be equal.");
            Assert.AreEqual(selections1.GetHashCode(), selections2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(selections1, selections2), "Cloning should not return the same reference.");
        }

        /// <summary>
        /// Ensures that <see cref="Selections.GetUncachedImplementations"/> correctly finds <see cref="Implementation"/>s not cached in a <see cref="IStore"/>.
        /// </summary>
        [Test]
        public void TestGetUncachedImplementations()
        {
            var implementation = new ImplementationSelection
            {
                Interface = "http://afb.users.sourceforge.net/zero-install/interfaces/seamonkey2.xml",
                ManifestDigest = new ManifestDigest("sha1new=3b83356a20a4aae1b5aed7e8e91382895fdddf22")
            };
            var selections = new Selections {Implementations = {implementation}};

            // Look inside a temporary (empty) store
            IEnumerable<Implementation> implementations;
            using (var temp = new TemporaryDirectory())
                implementations = selections.GetUncachedImplementations(new DirectoryStore(temp.Path), new InterfaceProvider());

            // Check the first (and only) entry of the "missing list" is the correct implementation
            var enumerator = implementations.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext(), "An least one Implementation should be missing.");
            Assert.AreEqual(implementation.ManifestDigest, enumerator.Current.ManifestDigest, "The actual Implementation should have the same digest as the selection information.");
        }
    }
}
