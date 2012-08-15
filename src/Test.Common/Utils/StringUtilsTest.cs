/*
 * Copyright 2006-2012 Bastian Eicher
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
using System.Security.Cryptography;
using NUnit.Framework;

namespace Common.Utils
{
    /// <summary>
    /// Contains test methods for <see cref="StringUtils"/>.
    /// </summary>
    [TestFixture]
    public class StringUtilsTest
    {
        [Test]
        public void TestCompare()
        {
            Assert.IsTrue(StringUtils.Compare("abc", "abc"));
            Assert.IsTrue(StringUtils.Compare("abc", "ABC"));

            Assert.IsFalse(StringUtils.Compare("abc", "123"));
            Assert.IsFalse(StringUtils.Compare("abc", "abc "));
        }

        [Test]
        public void TestContains()
        {
            Assert.IsTrue(StringUtils.Contains("This is a test.", "TEST"));
            Assert.IsTrue(StringUtils.Contains("This is a test.", "test"));

            Assert.IsFalse(StringUtils.Contains("abc", "123"));
            Assert.IsFalse(StringUtils.Contains("test", "This is a test."));
        }

        [Test]
        public void TestCountOccurences()
        {
            Assert.AreEqual(0, StringUtils.CountOccurences(null, '/'));
            Assert.AreEqual(0, StringUtils.CountOccurences("abc", '/'));
            Assert.AreEqual(1, StringUtils.CountOccurences("ab/c", '/'));
            Assert.AreEqual(2, StringUtils.CountOccurences("ab/c/", '/'));
        }

        [Test]
        public void TestGetLastWord()
        {
            Assert.AreEqual("sentence", StringUtils.GetLastWord("This is a sentence."));
            Assert.AreEqual("words", StringUtils.GetLastWord("some words"));
        }

        [Test]
        public void TestSplitMultilineText()
        {
            CollectionAssert.AreEqual(new[] {"123", "abc"}, StringUtils.SplitMultilineText("123\nabc"), "Should split Linux-stlye linebreaks");
            CollectionAssert.AreEqual(new[] {"123", "abc"}, StringUtils.SplitMultilineText("123\rabc"), "Should split old Mac-stlye linebreaks");
            CollectionAssert.AreEqual(new[] {"123", "abc"}, StringUtils.SplitMultilineText("123\r\nabc"), "Should split Windows-stlye linebreaks");
        }

        [Test]
        public void TestJoin()
        {
            Assert.AreEqual("part1", StringUtils.Join(" ", new[] {"part1"}));
            Assert.AreEqual("part1 part2", StringUtils.Join(" ", new[] {"part1", "part2"}));
            Assert.AreEqual("\"part1 part2\" part3", StringUtils.JoinEscapeArguments(new[] {"part1 part2", "part3"}));
        }

        [Test]
        public void TestJoinEscapeArguments()
        {
            Assert.AreEqual("part1", StringUtils.JoinEscapeArguments(new[] {"part1"}));
            Assert.AreEqual("part1 part2", StringUtils.JoinEscapeArguments(new[] {"part1", "part2"}));
            Assert.AreEqual("\"part1 \\\" part2\" part3", StringUtils.JoinEscapeArguments(new[] {"part1 \" part2", "part3"}));
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
        public void TestEscapeArgument()
        {
            Assert.AreEqual("test", StringUtils.EscapeArgument("test"), "Simple strings shouldn't be modified");
            Assert.AreEqual("\"test1 test2\"", StringUtils.EscapeArgument("test1 test2"), "Strings with whitespaces should be encapsulated");
            Assert.AreEqual("\"test1 test2\\\\\"", StringUtils.EscapeArgument("test1 test2\\"), "Trailing backslashes should be escaped");
            Assert.AreEqual("test1\\\"test2", StringUtils.EscapeArgument("test1\"test2"), "Quotation marks should be escaped");
            Assert.AreEqual("test1\\\\test2", StringUtils.EscapeArgument("test1\\\\test2"), "Consecutive slashes without quotation marks should not be escaped");
            Assert.AreEqual("test1\\\\\\\"test2", StringUtils.EscapeArgument("test1\\\"test2"), "Slashes with quotation marks should be escaped");
        }

        [Test]
        public void TestBase64Utf8Encode()
        {
            Assert.AreEqual(null, StringUtils.Base64Utf8Encode(null));
            Assert.AreEqual("", StringUtils.Base64Utf8Encode(""));
            Assert.AreEqual("dGVzdA==", StringUtils.Base64Utf8Encode("test"));
        }

        [Test]
        public void TestBase64Utf8Decode()
        {
            Assert.AreEqual(null, StringUtils.Base64Utf8Decode(null));
            Assert.AreEqual("", StringUtils.Base64Utf8Decode(""));
            Assert.AreEqual("test", StringUtils.Base64Utf8Decode("dGVzdA=="));
        }

        [Test]
        public void TestBase32Encode()
        {
            Assert.AreEqual("IFBA", StringUtils.Base32Encode(new byte[] {65, 66}));
        }

        [Test]
        public void TestBase16Encode()
        {
            Assert.AreEqual("4142", StringUtils.Base16Encode(new byte[] {65, 66}));
        }

        [Test]
        public void TestBase16Decode()
        {
            Assert.AreEqual(new byte[] {65, 66}, StringUtils.Base16Decode("4142"));
        }

        [Test]
        public void TestHash()
        {
            const string sha1ForEmptyString = "da39a3ee5e6b4b0d3255bfef95601890afd80709";
            Assert.AreEqual(sha1ForEmptyString, StringUtils.Hash("", SHA1.Create()));
        }

        [Test]
        public void TestExpandUnixVariables()
        {
            var variables = new StringDictionary
            {
                {"key1", "value1"},
                {"key2", "value2"},
                {"long key", "long value"}
            };

            Assert.AreEqual("value1value2/value1 value2 long value ", StringUtils.ExpandUnixVariables("$KEY1$KEY2/$KEY1 $KEY2 ${LONG KEY} $NOKEY", variables));

            Assert.AreEqual("", StringUtils.ExpandUnixVariables("", variables));
        }
    }
}
