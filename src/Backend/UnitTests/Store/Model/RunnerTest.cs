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
    /// Contains test methods for <see cref="Runner"/>.
    /// </summary>
    [TestFixture]
    public class RunnerTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="Runner"/>.
        /// </summary>
        internal static Runner CreateTestRunner()
        {
            return new Runner
            {
                InterfaceID = "http://0install.de/feeds/test/test1.xml", Command = "run2", Bindings = {EnvironmentBindingTest.CreateTestBinding()},
                Constraints = {new Constraint {NotBefore = new ImplementationVersion("1.0"), Before = new ImplementationVersion("2.0")}},
                Arguments = {"--arg"}
            };
        }
        #endregion

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var runner1 = CreateTestRunner();
            var runner2 = runner1.CloneRunner();

            // Ensure data stayed the same
            Assert.AreEqual(runner1, runner2, "Cloned objects should be equal.");
            Assert.AreEqual(runner1.GetHashCode(), runner2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(runner1, runner2), "Cloning should not return the same reference.");
        }
    }
}
