/*
 * Copyright 2006-2013 Bastian Eicher
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
        [Test(Description = "Ensures that the class is correctly serialized and deserialized.")]
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
            Assert.That(collection1, Is.XmlSerializable);
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

            Assert.IsTrue(dictionary.ContainsExactLanguage(new CultureInfo("en")), "Unspecified language should default to English generic");
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
                // Intential duplicates (should be ignored)
                "neutralValue",
                {"germanyValue", new CultureInfo("de-DE")}
            };

            dictionary.RemoveAll(new CultureInfo("en"));
            Assert.IsFalse(dictionary.ContainsExactLanguage(new CultureInfo("en")), "Unspecified language should default to English generic");
            dictionary.RemoveAll(new CultureInfo("de-DE"));
            Assert.IsFalse(dictionary.ContainsExactLanguage(new CultureInfo("de-DE")));
        }

        [Test]
        public void TestSet()
        {
            var dictionary = new LocalizableStringCollection
            {
                "neutralValue",
                {"germanyValue", new CultureInfo("de-DE")},
                // Intential duplicates (should be removed)
                "neutralValue",
                {"germanyValue", new CultureInfo("de-DE")}
            };

            dictionary.Set("neutralValue2");
            dictionary.Set("germanyValue2", new CultureInfo("de-DE"));

            Assert.AreEqual("neutralValue2", dictionary.GetExactLanguage(new CultureInfo("en")));
            Assert.AreEqual("germanyValue2", dictionary.GetExactLanguage(new CultureInfo("de-DE")));
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

            Assert.AreEqual("neutralValue", dictionary.GetExactLanguage(new CultureInfo("en")), "Unspecified language should default to English generic");
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
                {"germanValue", new CultureInfo("de")},
                {"germanyValue", new CultureInfo("de-DE")},
                {"americaValue", new CultureInfo("en-US")},
                {"gbValue", new CultureInfo("en-GB")},
                "neutralValue"
            };

            Assert.AreEqual("neutralValue", dictionary.GetBestLanguage(new CultureInfo("en")), "Unspecified language should default to English generic");
            Assert.AreEqual("americaValue", dictionary.GetBestLanguage(new CultureInfo("en-US")));
            Assert.AreEqual("neutralValue", dictionary.GetBestLanguage(new CultureInfo("en-CA")), "No exact match, should fall back to English generic");
            Assert.AreEqual("gbValue", dictionary.GetBestLanguage(new CultureInfo("en-GB")));
            Assert.AreEqual("germanValue", dictionary.GetBestLanguage(new CultureInfo("de")));
            Assert.AreEqual("germanyValue", dictionary.GetBestLanguage(new CultureInfo("de-DE")), "No exact match, should fall back to German generic");
            Assert.AreEqual("germanValue", dictionary.GetBestLanguage(new CultureInfo("de-AT")), "No exact match, should fall back to German generic");
            Assert.AreEqual("neutralValue", dictionary.GetBestLanguage(new CultureInfo("es-ES")), "No match, should fall back to English generic");

            dictionary.RemoveAll(new CultureInfo("en"));
            Assert.AreEqual("americaValue", dictionary.GetBestLanguage(new CultureInfo("es-ES")), "No English generic, should fall back to English US");

            dictionary.RemoveAll(new CultureInfo("en-US"));
            Assert.AreEqual("germanValue", dictionary.GetBestLanguage(new CultureInfo("es-ES")), "No English US, should fall back to first entry in collection");
        }
    }
}
