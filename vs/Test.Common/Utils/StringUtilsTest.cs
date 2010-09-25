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

namespace Common.Utils
{
    /// <summary>
    /// Contains test methods for <see cref="StringUtils"/>.
    /// </summary>
    [TestFixture]
    public class StringUtilsTest
    {
        /// <summary>
        /// Ensures <see cref="StringUtils.Compare"/> works correctly.
        /// </summary>
        [Test]
        public void TestCompare()
        {
            Assert.IsTrue(StringUtils.Compare("abc", "abc"));
            Assert.IsTrue(StringUtils.Compare("abc", "ABC"));

            Assert.IsFalse(StringUtils.Compare("abc", "123"));
            Assert.IsFalse(StringUtils.Compare("abc", "abc "));
        }

        /// <summary>
        /// Ensures <see cref="StringUtils.Contains"/> works correctly.
        /// </summary>
        [Test]
        public void TestContains()
        {
            Assert.IsTrue(StringUtils.Contains("This is a test.", "TEST"));
            Assert.IsTrue(StringUtils.Contains("This is a test.", "test"));

            Assert.IsFalse(StringUtils.Contains("abc", "123"));
            Assert.IsFalse(StringUtils.Contains("test", "This is a test."));
        }
        
        /// <summary>
        /// Ensures <see cref="StringUtils.GetLastWord"/> works correctly.
        /// </summary>
        [Test]
        public void TestGetLastWord()
        {
            Assert.AreEqual("sentence", StringUtils.GetLastWord("This is a sentence."));
            Assert.AreEqual("words", StringUtils.GetLastWord("some words"));
        }

        /// <summary>
        /// Ensures <see cref="StringUtils.Concatenate"/> works correctly.
        /// </summary>
        [Test]
        public void TestBuildStringFromLines()
        {
            Assert.AreEqual("line1", StringUtils.Concatenate(new[] { "line1" }, "\r\n"));
            Assert.AreEqual("line1\r\nline2", StringUtils.Concatenate(new[] { "line1", "line2" }, "\r\n"));
        }

        /// <summary>
        /// Ensures <see cref="StringUtils.GetLeftPartAtLastOccurrence"/> works correctly.
        /// </summary>
        [Test]
        public void TestGetLeftPartAtLastOccurrence()
        {
            Assert.AreEqual("This is a sentence", StringUtils.GetLeftPartAtLastOccurrence("This is a sentence. And another once", '.'));
            Assert.AreEqual("some words", StringUtils.GetLeftPartAtLastOccurrence("some words", '.'));
        }
    }
}
