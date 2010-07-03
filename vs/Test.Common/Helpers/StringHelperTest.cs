/*
 * Copyright 2006-2010 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using NUnit.Framework;

namespace Common.Helpers
{
    /// <summary>
    /// Contains test methods for <see cref="StringHelper"/>.
    /// </summary>
    [TestFixture]
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
        /// Ensures <see cref="StringHelper.Concatenate"/> works correctly.
        /// </summary>
        [Test]
        public void TestBuildStringFromLines()
        {
            Assert.AreEqual("line1", StringHelper.Concatenate(new[] { "line1" }, "\r\n"));
            Assert.AreEqual("line1\r\nline2", StringHelper.Concatenate(new[] { "line1", "line2" }, "\r\n"));
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
