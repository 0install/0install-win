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
using Xunit;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Contains test methods for <see cref="GenericBinding"/>.
    /// </summary>
    public class GenericBindingTest
    {
        /// <summary>
        /// Creates a fictive test <see cref="GenericBinding"/>.
        /// </summary>
        internal static GenericBinding CreateTestBinding() => new GenericBinding {Path = "path", Command = "command"};

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Fact]
        public void TestClone()
        {
            var binding1 = CreateTestBinding();
            var binding2 = binding1.Clone();

            // Ensure data stayed the same
            binding2.Should().Be(binding1, because: "Cloned objects should be equal.");
            binding2.GetHashCode().Should().Be(binding1.GetHashCode(), because: "Cloned objects' hashes should be equal.");
            binding2.Should().NotBeSameAs(binding1, because: "Cloning should not return the same reference.");
        }
    }
}
