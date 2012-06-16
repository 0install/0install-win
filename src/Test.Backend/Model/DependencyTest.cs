/*
 * Copyright 2010-2012 Bastian Eicher
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

namespace ZeroInstall.Model
{
    /// <summary>
    /// Contains test methods for <see cref="Dependency"/>.
    /// </summary>
    [TestFixture]
    public class DependencyTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="Dependency"/>.
        /// </summary>
        public static Dependency CreateTestDependency()
        {
            return new Dependency
            {
                Interface = "",
                Importance = Importance.Recommended,
                Use = "testing",
                Constraints = {new Constraint {NotBeforeVersion = new ImplementationVersion("1.0"), BeforeVersion = new ImplementationVersion("2.0")}}
            };
        }
        #endregion

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var dependency1 = CreateTestDependency();
            var dependency2 = dependency1.Clone();

            // Ensure data stayed the same
            Assert.AreEqual(dependency1, dependency2, "Cloned objects should be equal.");
            Assert.AreEqual(dependency1.GetHashCode(), dependency2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(dependency1, dependency2), "Cloning should not return the same reference.");
        }

        /// <summary>
        /// Ensures that the class can be correctly compared.
        /// </summary>
        [Test]
        public void TestEquals()
        {
            var dependency1 = CreateTestDependency();
            var dependency2 = dependency1.Clone();
            dependency2.Bindings.Add(new EnvironmentBinding());

            // Ensure data stayed the same
            Assert.AreEqual(dependency1, dependency1, "Equals() should be reflexive.");
            Assert.AreEqual(dependency1.GetHashCode(), dependency1.GetHashCode(), "GetHashCode() should be reflexive.");
            Assert.AreNotEqual(dependency1, dependency2);
            Assert.AreNotEqual(dependency1.GetHashCode(), dependency2.GetHashCode());
        }

        /// <summary>
        /// Ensures <see cref="Dependency.NotBeforeVersion"/> and <see cref="Dependency.BeforeVersion"/> deduce correct values from <see cref="Dependency.Constraints"/>.
        /// </summary>
        [Test]
        public void TestConstraintCollapsing()
        {
            var dependency = new Dependency
            {
                Constraints =
                    {
                        new Constraint {NotBeforeVersion = new ImplementationVersion("1.0"), BeforeVersion = new ImplementationVersion("2.0")},
                        new Constraint {NotBeforeVersion = new ImplementationVersion("0.9"), BeforeVersion = new ImplementationVersion("1.9")},
                    }
            };
            Assert.AreEqual(new ImplementationVersion("1.0"), dependency.NotBeforeVersion);
            Assert.AreEqual(new ImplementationVersion("1.9"), dependency.BeforeVersion);
        }
    }
}
