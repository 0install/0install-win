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

using System.Globalization;
using NUnit.Framework;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Contains test methods for <see cref="Implementation"/>.
    /// </summary>
    [TestFixture]
    public class ImplementationTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="Implementation"/>.
        /// </summary>
        public static Implementation CreateTestImplementation()
        {
            return new Implementation
            {
                ID = "id", ManifestDigest = new ManifestDigest("sha256=invalid"), Version = new ImplementationVersion("1.0"),
                Architecture = new Architecture(OS.Windows, Cpu.I586), Languages = {new CultureInfo("en-US")},
                Main = "executable", DocDir = "doc", Stability = Stability.Developer,
                Bindings = {EnvironmentBindingTest.CreateTestBinding()},
                RetrievalMethods = {ArchiveTest.CreateTestArchive()}
            };
        }
        #endregion

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var implementation1 = CreateTestImplementation();
            var implementation2 = implementation1.CloneImplementation();

            // Ensure data stayed the same
            Assert.AreEqual(implementation1, implementation2, "Cloned objects should be equal.");
            Assert.AreEqual(implementation1.GetHashCode(), implementation2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(implementation1, implementation2), "Cloning should not return the same reference.");
        }

        /// <summary>
        /// Ensures that <see cref="Implementation.Simplify"/> correctly identifies manifest digests in the ID tag.
        /// </summary>
        [Test]
        public void TestSimplify()
        {
            var implementation = new Implementation { ID = "sha256=invalid" };
            implementation.Simplify();
            Assert.AreEqual("invalid", implementation.ManifestDigest.Sha256);
        }
    }
}
