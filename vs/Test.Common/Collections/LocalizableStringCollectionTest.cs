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

using System.Collections.Generic;
using System.Globalization;
using Common.Storage;
using NUnit.Framework;

namespace Common.Collections
{
    /// <summary>
    /// Contains test methods for <see cref="LocalizableStringCollection"/>.
    /// </summary>
    [TestFixture]
    public class LocalizableStringCollectionTest
    {
        /// <summary>
        /// Ensures that the class is correctly serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoad()
        {
            var collection1 = new LocalizableStringCollection
            {
                "neutralValue",
                {"americaValue", new CultureInfo("en-US")},
                {"gbValue", new CultureInfo("en-GB")},
                {"germanValue", new CultureInfo("de")},
                {"germanyValue", new CultureInfo("de-DE")}
            };

            // Serialize and deserialize data
            string data = XmlStorage.ToString(collection1);
            var collection2 = XmlStorage.FromString<LocalizableStringCollection>(data);

            // Ensure data stayed the same
            Assert.AreEqual(collection1, collection2, "Serialized objects should be equal.");
            Assert.AreEqual(collection1.GetSequencedHashCode(), collection2.GetSequencedHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(collection1, collection2), "Serialized objects should not return the same reference.");
        }

        [Test]
        public void TestContainsExactLanguage()
        {
            var dictionary = new LocalizableStringCollection
            {
                "neutralValue",
                {"germanyValue", new CultureInfo("de-DE")}
            };

            Assert.IsTrue(dictionary.ContainsExactLanguage(CultureInfo.InvariantCulture));
            Assert.IsTrue(dictionary.ContainsExactLanguage(new CultureInfo("de-DE")));
            Assert.IsFalse(dictionary.ContainsExactLanguage(new CultureInfo("de")));
            Assert.IsFalse(dictionary.ContainsExactLanguage(new CultureInfo("de-AT")));
            Assert.IsFalse(dictionary.ContainsExactLanguage(new CultureInfo("en-US")));
        }

        [Test]
        public void TestRemoveAll()
        {
            var dictionary = new LocalizableStringCollection
            {
                "neutralValue",
                {"germanyValue", new CultureInfo("de-DE")},
                "neutralValue",
                {"germanyValue", new CultureInfo("de-DE")}
            };

            dictionary.RemoveAll(CultureInfo.InvariantCulture);
            Assert.IsFalse(dictionary.ContainsExactLanguage(CultureInfo.InvariantCulture));
            dictionary.RemoveAll(new CultureInfo("de-DE"));
            Assert.IsFalse(dictionary.ContainsExactLanguage(new CultureInfo("de-DE")));
        }

        [Test]
        public void TestGetExactLanguage()
        {
            var dictionary = new LocalizableStringCollection
            {
                "neutralValue",
                {"americaValue", new CultureInfo("en-US")},
                {"gbValue", new CultureInfo("en-GB")},
                {"germanValue", new CultureInfo("de")},
                {"germanyValue", new CultureInfo("de-DE")}
            };

            Assert.AreEqual("neutralValue", dictionary.GetExactLanguage(CultureInfo.InvariantCulture));
            Assert.AreEqual("americaValue", dictionary.GetExactLanguage(new CultureInfo("en-US")));
            Assert.Throws<KeyNotFoundException>(() => dictionary.GetExactLanguage(new CultureInfo("en-CA")));
            Assert.AreEqual("gbValue", dictionary.GetExactLanguage(new CultureInfo("en-GB")));
            Assert.AreEqual("germanValue", dictionary.GetExactLanguage(new CultureInfo("de")));
            Assert.AreEqual("germanyValue", dictionary.GetExactLanguage(new CultureInfo("de-DE")));
            Assert.Throws<KeyNotFoundException>(() => dictionary.GetExactLanguage(new CultureInfo("de-AT")));
        }

        [Test]
        public void TestGetBestLanguage()
        {
            var dictionary = new LocalizableStringCollection
            {
                {"americaValue", new CultureInfo("en-US")},
                {"gbValue", new CultureInfo("en-GB")},
                {"germanValue", new CultureInfo("de")},
                {"germanyValue", new CultureInfo("de-DE")},
                "neutralValue"
            };

            Assert.AreEqual("neutralValue", dictionary.GetBestLanguage(CultureInfo.InvariantCulture));
            Assert.AreEqual("americaValue", dictionary.GetBestLanguage(new CultureInfo("en-US"))); // Exact match
            Assert.AreEqual("neutralValue", dictionary.GetBestLanguage(new CultureInfo("en-CA"))); // No English generic, fall back to neutral
            Assert.AreEqual("gbValue", dictionary.GetBestLanguage(new CultureInfo("en-GB"))); // Exact match
            Assert.AreEqual("germanValue", dictionary.GetBestLanguage(new CultureInfo("de"))); // Exact match
            Assert.AreEqual("germanyValue", dictionary.GetBestLanguage(new CultureInfo("de-DE"))); // Fall back to German generic
            Assert.AreEqual("germanValue", dictionary.GetBestLanguage(new CultureInfo("de-AT"))); // Fall back to German generic
            Assert.AreEqual("neutralValue", dictionary.GetBestLanguage(new CultureInfo("es-ES"))); // No match, fall back to neutral
        }
    }
}
