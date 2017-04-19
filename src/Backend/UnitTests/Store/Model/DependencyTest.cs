/*
 * Copyright 2010-2016 Bastian Eicher
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

using FluentAssertions;
using NUnit.Framework;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="Dependency"/>.
    /// </summary>
    [TestFixture]
    public class DependencyTest
    {
        /// <summary>
        /// Creates a fictive test <see cref="Dependency"/>.
        /// </summary>
        public static Dependency CreateTestDependency() => new Dependency
        {
            Versions = new VersionRange("1.0..!2.0"),
            OS = OS.Windows,
            Distributions = {Restriction.DistributionZeroInstall},
            Constraints = {new Constraint {NotBefore = new ImplementationVersion("1.0"), Before = new ImplementationVersion("2.0")}},
            Importance = Importance.Recommended
        };

        /// <summary>
        /// Ensures that the class can be correctly cloned and compared.
        /// </summary>
        [Test]
        public void TestCloneEquals()
        {
            var dependency1 = CreateTestDependency();
            dependency1.Should().Be(dependency1, because: "Equals() should be reflexive.");
            dependency1.GetHashCode().Should().Be(dependency1.GetHashCode(), because: "GetHashCode() should be reflexive.");

            var dependency2 = dependency1.CloneDependency();
            dependency2.Should().Be(dependency1, because: "Cloned objects should be equal.");
            dependency2.GetHashCode().Should().Be(dependency1.GetHashCode(), because: "Cloned objects' hashes should be equal.");
            ReferenceEquals(dependency1, dependency2).Should().BeFalse(because: "Cloning should not return the same reference.");

            dependency2.Bindings.Add(new EnvironmentBinding());
            dependency2.Should().NotBe(dependency1, because: "Modified objects should no longer be equal");
        }
    }
}
