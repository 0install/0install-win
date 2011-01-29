/*
 * Copyright 2006-2011 Bastian Eicher
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

using System.Collections.Specialized;
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
        /// Ensures <see cref="StringUtils.CountOccurences"/> works correctly.
        /// </summary>
        [Test]
        public void TestCountOccurences()
        {
            Assert.AreEqual(0, StringUtils.CountOccurences(null, '/'));
            Assert.AreEqual(0, StringUtils.CountOccurences("abc", '/'));
            Assert.AreEqual(1, StringUtils.CountOccurences("ab/c", '/'));
            Assert.AreEqual(2, StringUtils.CountOccurences("ab/c/", '/'));
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

        [Test]
        public void TestConcatenate()
        {
            Assert.AreEqual("part1", StringUtils.Concatenate(new[] { "part1" }, " "));
            Assert.AreEqual("part1 part2", StringUtils.Concatenate(new[] { "part1", "part2" }, " "));
            Assert.AreEqual("\"part1 part2\" part3", StringUtils.Concatenate(new[] { "part1 part2", "part3" }, " ", '"'));
        }

        [Test]
        public void TestGetLeftRightPartChar()
        {
            const string testString = "text1 text2 text3";
            Assert.AreEqual("text1", StringUtils.GetLeftPartAtFirstOccurrence(testString, ' '));
            Assert.AreEqual("text2 text3", StringUtils.GetRightPartAtFirstOccurrence(testString, ' '));
            Assert.AreEqual("text1 text2", StringUtils.GetLeftPartAtLastOccurrence(testString, ' '));
            Assert.AreEqual("text3", StringUtils.GetRightPartAtLastOccurrence(testString, ' '));
        }

        [Test]
        public void TestGetLeftRightPartString()
        {
            const string testString = "text1 - text2 - text3";
            Assert.AreEqual("text1", StringUtils.GetLeftPartAtFirstOccurrence(testString, " - "));
            Assert.AreEqual("text2 - text3", StringUtils.GetRightPartAtFirstOccurrence(testString, " - "));
            Assert.AreEqual("text1 - text2", StringUtils.GetLeftPartAtLastOccurrence(testString, " - "));
            Assert.AreEqual("text3", StringUtils.GetRightPartAtLastOccurrence(testString, " - "));
        }

        [Test]
        public void TestExpandUnixVariables()
        {
            var variables = new StringDictionary
            {
                {"key1", "value1"},
                {"key2", "value2"}
            };

            Assert.AreEqual("value1value2/value1 value2", StringUtils.ExpandUnixVariables("$KEY1$KEY2/$KEY1 $KEY2", variables));
        }
    }
}
