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
    /// Contains test methods for <see cref="FeedReference"/>.
    /// </summary>
    [TestFixture]
    public class FeedReferenceTest
    {
        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
        public void TestClone()
        {
            var reference1 = new FeedReference
            {
                Source = FeedTest.Test1Uri,
                Architecture = new Architecture(OS.Windows, Cpu.I586), Languages = {"en-US"}
            };
            var reference2 = reference1.Clone();

            // Ensure data stayed the same
            Assert.AreEqual(reference1, reference2, "Cloned objects should be equal.");
            Assert.AreEqual(reference1.GetHashCode(), reference2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(reference1, reference2), "Cloning should not return the same reference.");
        }

        /// <summary>
        /// Ensures that equal and unequal instances can be correctly differentiated.
        /// </summary>
        [Test]
        public void TestEquals()
        {
            var reference1 = new FeedReference
            {
                Source = FeedTest.Test1Uri,
                Architecture = new Architecture(OS.Windows, Cpu.I586),
                Languages = {"en-US"}
            };
            var reference2 = new FeedReference
            {
                Source = FeedTest.Test1Uri,
                Architecture = new Architecture(OS.Windows, Cpu.I586),
                Languages = {"en-US"}
            };
            Assert.AreEqual(reference1, reference2);

            reference2 = new FeedReference
            {
                Source = FeedTest.Test1Uri,
                Architecture = new Architecture(OS.Windows, Cpu.I586),
                Languages = {"de-DE"}
            };
            Assert.AreNotEqual(reference1, reference2);

            reference2 = new FeedReference
            {
                Source = FeedTest.Test1Uri,
                Languages = {"en-US"}
            };
            Assert.AreNotEqual(reference1, reference2);
        }
    }
}
