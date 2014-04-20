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
    /// Contains test methods for <see cref="EnvironmentBinding"/>.
    /// </summary>
    [TestFixture]
    public class EnvironmentBindingTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="EnvironmentBinding"/>.
        /// </summary>
        internal static EnvironmentBinding CreateTestBinding()
        {
            return new EnvironmentBinding {Name = "name", Value = "value", Default = "default", Mode = EnvironmentMode.Replace, Separator = ","};
        }
        #endregion

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var binding1 = CreateTestBinding();
            var binding2 = binding1.Clone();

            // Ensure data stayed the same
            Assert.AreEqual(binding1, binding2, "Cloned objects should be equal.");
            Assert.AreEqual(binding1.GetHashCode(), binding2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(binding1, binding2), "Cloning should not return the same reference.");
        }
    }
}
