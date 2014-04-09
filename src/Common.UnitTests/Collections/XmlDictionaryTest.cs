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

using System;
using NanoByte.Common.Storage;
using NUnit.Framework;

namespace NanoByte.Common.Collections
{
    /// <summary>
    /// Contains test methods for <see cref="XmlDictionary"/>.
    /// </summary>
    [TestFixture]
    public class XmlDictionaryTest
    {
        [Test(Description = "Ensures that the class is correctly serialized and deserialized.")]
        public void TestSaveLoad()
        {
            var dictionary1 = new XmlDictionary
            {
                {"key1", "value1"},
                {"key2", "value2"}
            };

            // Serialize and deserialize data
            string data = dictionary1.ToXmlString();
            var dictionary2 = XmlStorage.FromXmlString<XmlDictionary>(data);

            // Ensure data stayed the same
            Assert.AreEqual(dictionary1, dictionary2, "Serialized objects should be equal.");
            Assert.IsFalse(ReferenceEquals(dictionary1, dictionary2), "Serialized objects should not return the same reference.");
        }

        [Test]
        public void TestContainsKey()
        {
            var dictionary = new XmlDictionary
            {
                {"key1", "value1"}
            };
            Assert.IsTrue(dictionary.ContainsKey("key1"));
            Assert.IsFalse(dictionary.ContainsKey("key2"));
        }

        [Test]
        public void TestContainsValue()
        {
            var dictionary = new XmlDictionary
            {
                {"key1", "value1"}
            };
            Assert.IsTrue(dictionary.ContainsValue("value1"));
            Assert.IsFalse(dictionary.ContainsValue("value2"));
        }

        [Test]
        public void TestGetValue()
        {
            var dictionary = new XmlDictionary
            {
                {"key1", "value1"},
                {"key2", "value2"}
            };
            Assert.AreEqual("value1", dictionary.GetValue("key1"));
        }

        [Test]
        public void ShouldRejectDuplicateKeys()
        {
            var dictionary = new XmlDictionary
            {
                {"key1", "value1"},
                {"key2", "value2"}
            };

            // Check for duplicate keys when adding new entries
            Assert.Throws<ArgumentException>(() => dictionary.Add("key1", "newValue1"));

            // Check for duplicate keys when modifying existing entries
            var entry = new XmlDictionaryEntry("key3", "value3");
            dictionary.Add(entry);
            Assert.Throws<InvalidOperationException>(() => entry.Key = "key1");
        }
    }
}
