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

using NUnit.Framework;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="Restriction"/>.
    /// </summary>
    [TestFixture]
    public class RestrictionTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="Restriction"/>.
        /// </summary>
        public static Restriction CreateTestRestriction()
        {
            return new Restriction
            {
                Versions = new VersionRange("1.0..!2.0"),
                OS = OS.Windows,
                Distributions = {"0install"},
                Constraints = {new Constraint {NotBefore = new ImplementationVersion("1.0"), Before = new ImplementationVersion("2.0")}}
            };
        }
        #endregion

        /// <summary>
        /// Ensures that the class can be correctly cloned and compared.
        /// </summary>
        [Test]
        public void TestCloneEquals()
        {
            var restriction1 = CreateTestRestriction();
            Assert.AreEqual(restriction1, restriction1, "Equals() should be reflexive.");
            Assert.AreEqual(restriction1.GetHashCode(), restriction1.GetHashCode(), "GetHashCode() should be reflexive.");

            var restriction2 = restriction1.Clone();
            Assert.AreEqual(restriction1, restriction2, "Cloned objects should be equal.");
            Assert.AreEqual(restriction1.GetHashCode(), restriction2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(restriction1, restriction2), "Cloning should not return the same reference.");

            restriction2.Constraints.Add(new Constraint());
            Assert.AreNotEqual(restriction1, restriction2, "Modified objects should no longer be equal");
            //Assert.AreNotEqual(Restriction1.GetHashCode(), Restriction2.GetHashCode(), "Modified objects' hashes should no longer be equal");
        }

        /// <summary>
        /// Ensures <see cref="Restriction.Normalize"/> correctly converts <see cref="Constraint.NotBefore"/> to <see cref="Restriction.Versions"/>.
        /// </summary>
        [Test]
        public void TestNormalizeNotBefore()
        {
            var restriction = new Restriction {Constraints = {new Constraint {NotBefore = new ImplementationVersion("1.0")}}};
            restriction.Normalize();
            Assert.AreEqual(new VersionRange("1.0.."), restriction.Versions);
        }

        /// <summary>
        /// Ensures <see cref="Restriction.Normalize"/> correctly converts <see cref="Constraint.Before"/> to <see cref="Restriction.Versions"/>.
        /// </summary>
        [Test]
        public void TestNormalizeBefore()
        {
            var restriction = new Restriction {Constraints = {new Constraint {Before = new ImplementationVersion("2.0")}}};
            restriction.Normalize();
            Assert.AreEqual(new VersionRange("..!2.0"), restriction.Versions);
        }

        /// <summary>
        /// Ensures <see cref="Restriction.Normalize"/> correctly converts <see cref="Constraint.NotBefore"/> and <see cref="Constraint.Before"/> to <see cref="Restriction.Versions"/>.
        /// </summary>
        [Test]
        public void TestNormalizeRange()
        {
            var restriction = new Restriction {Constraints = {new Constraint {NotBefore = new ImplementationVersion("1.0"), Before = new ImplementationVersion("2.0")}}};
            restriction.Normalize();
            Assert.AreEqual(new VersionRange("1.0..!2.0"), restriction.Versions);
        }

        /// <summary>
        /// Ensures <see cref="Restriction.Normalize"/> deduces correct <see cref="Restriction.Versions"/> values from overlapping <see cref="Restriction.Constraints"/>.
        /// </summary>
        [Test]
        public void TestNormalizeOverlap()
        {
            var restriction = new Restriction
            {
                Constraints =
                {
                    new Constraint {NotBefore = new ImplementationVersion("1.0"), Before = new ImplementationVersion("2.0")},
                    new Constraint {NotBefore = new ImplementationVersion("0.9"), Before = new ImplementationVersion("1.9")},
                }
            };
            restriction.Normalize();
            Assert.AreEqual(new VersionRange("1.0..!1.9"), restriction.Versions);
        }
    }
}
