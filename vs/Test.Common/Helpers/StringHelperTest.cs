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

using NUnit.Framework;

namespace Common.Helpers
{
    /// <summary>
    /// Contains test methods for <see cref="StringHelper"/>.
    /// </summary>
    public class StringHelperTest
    {
        /// <summary>
        /// Ensures <see cref="StringHelper.Compare"/> works correctly.
        /// </summary>
        [Test]
        public void TestCompare()
        {
            Assert.IsTrue(StringHelper.Compare("abc", "abc"));
            Assert.IsTrue(StringHelper.Compare("abc", "ABC"));

            Assert.IsFalse(StringHelper.Compare("abc", "123"));
            Assert.IsFalse(StringHelper.Compare("abc", "abc "));
        }

        /// <summary>
        /// Ensures <see cref="StringHelper.Contains"/> works correctly.
        /// </summary>
        [Test]
        public void TestContains()
        {
            Assert.IsTrue(StringHelper.Contains("This is a test.", "TEST"));
            Assert.IsTrue(StringHelper.Contains("This is a test.", "test"));

            Assert.IsFalse(StringHelper.Contains("abc", "123"));
            Assert.IsFalse(StringHelper.Contains("test", "This is a test."));
        }
        
        /// <summary>
        /// Ensures <see cref="StringHelper.GetLastWord"/> works correctly.
        /// </summary>
        [Test]
        public void TestGetLastWord()
        {
            Assert.AreEqual("sentence", StringHelper.GetLastWord("This is a sentence."));
            Assert.AreEqual("words", StringHelper.GetLastWord("some words"));
        }

        /// <summary>
        /// Ensures <see cref="StringHelper.BuildStringFromLines"/> works correctly.
        /// </summary>
        [Test]
        public void TestBuildStringFromLines()
        {
            Assert.AreEqual("line1", StringHelper.BuildStringFromLines(new[] { "line1" }, "\n"));
            Assert.AreEqual("line1\nline2", StringHelper.BuildStringFromLines(new[] { "line1", "line2" }, "\n"));
        }

        /// <summary>
        /// Ensures <see cref="StringHelper.GetLeftPartAtLastOccurrence"/> works correctly.
        /// </summary>
        [Test]
        public void TestGetLeftPartAtLastOccurrence()
        {
            Assert.AreEqual("This is a sentence", StringHelper.GetLeftPartAtLastOccurrence("This is a sentence. And another once", '.'));
            Assert.AreEqual("some words", StringHelper.GetLeftPartAtLastOccurrence("some words", '.'));
        }
    }
}
