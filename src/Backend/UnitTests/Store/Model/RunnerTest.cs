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
    /// Contains test methods for <see cref="Runner"/>.
    /// </summary>
    [TestFixture]
    public class RunnerTest
    {
        /// <summary>
        /// Creates a fictive test <see cref="Runner"/>.
        /// </summary>
        private static Runner CreateTestRunner() => new Runner
        {
            InterfaceUri = FeedTest.Test1Uri, Command = "run2", Bindings = {EnvironmentBindingTest.CreateTestBinding()},
            Versions = new VersionRange("1.0..!2.0"),
            Arguments = {"--arg"}
        };

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var runner1 = CreateTestRunner();
            var runner2 = runner1.CloneRunner();

            // Ensure data stayed the same
            runner2.Should().Be(runner1, because: "Cloned objects should be equal.");
            runner2.GetHashCode().Should().Be(runner1.GetHashCode(), because: "Cloned objects' hashes should be equal.");
            runner2.Should().NotBeSameAs(runner1, because: "Cloning should not return the same reference.");
        }
    }
}
