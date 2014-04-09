/*
 * Copyright 2006-2014 Bastian Eicher
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

namespace NanoByte.Common.Utils
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
            Assert.IsTrue(StringUtils.EqualsIgnoreCase("abc", "abc"));
            Assert.IsTrue(StringUtils.EqualsIgnoreCase("abc", "ABC"));

            Assert.IsFalse(StringUtils.EqualsIgnoreCase("abc", "123"));
            Assert.IsFalse(StringUtils.EqualsIgnoreCase("abc", "abc "));
        }

        [Test]
        public void TestContains()
        {
            Assert.IsTrue("This is a test.".ContainsIgnoreCase("TEST"));
            Assert.IsTrue("This is a test.".ContainsIgnoreCase("test"));

            Assert.IsFalse("abc".ContainsIgnoreCase("123"));
            Assert.IsFalse("test".ContainsIgnoreCase("This is a test."));
        }

        [Test]
        public void TestCountOccurences()
        {
            Assert.AreEqual(0, StringUtils.CountOccurences(null, '/'));
            Assert.AreEqual(0, "abc".CountOccurences('/'));
            Assert.AreEqual(1, "ab/c".CountOccurences('/'));
            Assert.AreEqual(2, "ab/c/".CountOccurences('/'));
        }

        [Test]
        public void TestGetLastWord()
        {
            Assert.AreEqual("sentence", "This is a sentence.".GetLastWord());
            Assert.AreEqual("words", "some words".GetLastWord());
        }

        [Test]
        public void TestSplitMultilineText()
        {
            CollectionAssert.AreEqual(new[] {"123", "abc"}, "123\nabc".SplitMultilineText(), "Should split Linux-stlye linebreaks");
            CollectionAssert.AreEqual(new[] {"123", "abc"}, "123\rabc".SplitMultilineText(), "Should split old Mac-stlye linebreaks");
            CollectionAssert.AreEqual(new[] {"123", "abc"}, "123\r\nabc".SplitMultilineText(), "Should split Windows-stlye linebreaks");
        }

        [Test]
        public void TestJoin()
        {
            Assert.AreEqual("part1", StringUtils.Join(" ", new[] {"part1"}));
            Assert.AreEqual("part1 part2", StringUtils.Join(" ", new[] {"part1", "part2"}));
            Assert.AreEqual("\"part1 part2\" part3", new[] {"part1 part2", "part3"}.JoinEscapeArguments());
        }

        [Test]
        public void TestJoinEscapeArguments()
        {
            Assert.AreEqual("part1", new[] {"part1"}.JoinEscapeArguments());
            Assert.AreEqual("part1 part2", new[] {"part1", "part2"}.JoinEscapeArguments());
            Assert.AreEqual("\"part1 \\\" part2\" part3", new[] {"part1 \" part2", "part3"}.JoinEscapeArguments());
        }

        [Test]
        public void TestGetLeftRightPartChar()
        {
            const string testString = "text1 text2 text3";
            Assert.AreEqual("text1", testString.GetLeftPartAtFirstOccurrence(' '));
            Assert.AreEqual("text2 text3", testString.GetRightPartAtFirstOccurrence(' '));
            Assert.AreEqual("text1 text2", testString.GetLeftPartAtLastOccurrence(' '));
            Assert.AreEqual("text3", testString.GetRightPartAtLastOccurrence(' '));
        }

        [Test]
        public void TestGetLeftRightPartString()
        {
            const string testString = "text1 - text2 - text3";
            Assert.AreEqual("text1", testString.GetLeftPartAtFirstOccurrence(" - "));
            Assert.AreEqual("text2 - text3", testString.GetRightPartAtFirstOccurrence(" - "));
            Assert.AreEqual("text1 - text2", testString.GetLeftPartAtLastOccurrence(" - "));
            Assert.AreEqual("text3", testString.GetRightPartAtLastOccurrence(" - "));
        }

        [Test]
        public void TestRemoveAll()
        {
            Assert.AreEqual("ac", "abcd".RemoveAll("bd"));
        }

        [Test]
        public void TestStripCharacters()
        {
            Assert.AreEqual("ab", "a!b?".StripCharacters(new[] {'!', '?'}));
        }

        [Test]
        public void TestEscapeArgument()
        {
            Assert.AreEqual("test", "test".EscapeArgument(), "Simple strings shouldn't be modified");
            Assert.AreEqual("\"test1 test2\"", "test1 test2".EscapeArgument(), "Strings with whitespaces should be encapsulated");
            Assert.AreEqual("\"test1 test2\\\\\"", "test1 test2\\".EscapeArgument(), "Trailing backslashes should be escaped");
            Assert.AreEqual("test1\\\"test2", "test1\"test2".EscapeArgument(), "Quotation marks should be escaped");
            Assert.AreEqual("test1\\\\test2", "test1\\\\test2".EscapeArgument(), "Consecutive slashes without quotation marks should not be escaped");
            Assert.AreEqual("test1\\\\\\\"test2", "test1\\\"test2".EscapeArgument(), "Slashes with quotation marks should be escaped");
        }

        [Test]
        public void TestBase64Utf8Encode()
        {
            Assert.AreEqual(null, StringUtils.Base64Utf8Encode(null));
            Assert.AreEqual("", "".Base64Utf8Encode());
            Assert.AreEqual("dGVzdA==", "test".Base64Utf8Encode());
        }

        [Test]
        public void TestBase64Utf8Decode()
        {
            Assert.AreEqual(null, StringUtils.Base64Utf8Decode(null));
            Assert.AreEqual("", "".Base64Utf8Decode());
            Assert.AreEqual("test", "dGVzdA==".Base64Utf8Decode());
        }

        [Test]
        public void TestBase32Encode()
        {
            Assert.AreEqual("IFBA", new byte[] {65, 66}.Base32Encode());
        }

        [Test]
        public void TestBase16Encode()
        {
            Assert.AreEqual("4142", new byte[] {65, 66}.Base16Encode());
        }

        [Test]
        public void TestBase16Decode()
        {
            Assert.AreEqual(new byte[] {65, 66}, "4142".Base16Decode());
        }

        [Test]
        public void TestHash()
        {
            const string sha1ForEmptyString = "da39a3ee5e6b4b0d3255bfef95601890afd80709";
            Assert.AreEqual(sha1ForEmptyString, "".Hash(SHA1.Create()));
        }

        [Test]
        public void TestGeneratePassword()
        {
            for (int i = 0; i < 128; i++)
            {
                string result = StringUtils.GeneratePassword(i);
                Assert.That(result, Is.Not.StringContaining("="));
                Assert.That(result, Is.Not.StringContaining("l"));
                Assert.AreEqual(result.Length, i);
            }
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

            Assert.AreEqual(
                "value1value2/value1 value2 long value ",
                StringUtils.ExpandUnixVariables("$KEY1$KEY2/$KEY1 $KEY2 ${LONG KEY} $NOKEY", variables));

            Assert.AreEqual(
                "value1-bla",
                StringUtils.ExpandUnixVariables("$KEY1-bla", variables));

            Assert.AreEqual("", StringUtils.ExpandUnixVariables("", variables));
        }
    }
}
