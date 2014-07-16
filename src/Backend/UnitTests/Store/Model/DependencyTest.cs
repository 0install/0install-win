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

using NUnit.Framework;

namespace ZeroInstall.Store.Model
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
                InterfaceID = "",
                Versions = new VersionRange("1.0..!2.0"),
                OS = OS.Windows,
                Distribution = "0install",
                Constraints = {new Constraint {NotBefore = new ImplementationVersion("1.0"), Before = new ImplementationVersion("2.0")}},
                Importance = Importance.Recommended,
                Use = Dependency.UseTesting
            };
        }
        #endregion

        /// <summary>
        /// Ensures that the class can be correctly cloned and compared.
        /// </summary>
        [Test]
        public void TestCloneEquals()
        {
            var dependency1 = CreateTestDependency();
            Assert.AreEqual(dependency1, dependency1, "Equals() should be reflexive.");
            Assert.AreEqual(dependency1.GetHashCode(), dependency1.GetHashCode(), "GetHashCode() should be reflexive.");

            var dependency2 = dependency1.CloneDependency();
            Assert.AreEqual(dependency1, dependency2, "Cloned objects should be equal.");
            Assert.AreEqual(dependency1.GetHashCode(), dependency2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(dependency1, dependency2), "Cloning should not return the same reference.");

            dependency2.Bindings.Add(new EnvironmentBinding());
            Assert.AreNotEqual(dependency1, dependency2, "Modified objects should no longer be equal");
            //Assert.AreNotEqual(dependency1.GetHashCode(), dependency2.GetHashCode(), "Modified objects' hashes should no longer be equal");
        }
    }
}
