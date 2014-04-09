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

using System.Globalization;
using NanoByte.Common.Storage;
using NUnit.Framework;

namespace NanoByte.Common.Collections
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
                {"en-US", "americaValue"},
                {"en-GB", "gbValue"},
                {"de", "germanValue"},
                {"de-DE", "germanyValue"}
            };

            // Serialize and deserialize data
            Assert.That(collection1, Is.XmlSerializable);
            string data = collection1.ToXmlString();
            var collection2 = XmlStorage.FromXmlString<LocalizableStringCollection>(data);

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
                {"de-DE", "germanyValue"}
            };

            Assert.IsTrue(dictionary.ContainsExactLanguage(LocalizableString.DefaultLanguage), "Unspecified language should default to English generic");
            Assert.IsTrue(dictionary.ContainsExactLanguage(new CultureInfo("de-DE")));
            Assert.IsFalse(dictionary.ContainsExactLanguage(new CultureInfo("de")));
            Assert.IsFalse(dictionary.ContainsExactLanguage(new CultureInfo("de-AT")));
            Assert.IsFalse(dictionary.ContainsExactLanguage(new CultureInfo("en-US")));
        }

        [Test]
        public void TestRemoveRange()
        {
            var dictionary = new LocalizableStringCollection
            {
                "neutralValue",
                {"de-DE", "germanyValue"},
                // Intential duplicates (should be ignored)
                "neutralValue",
                {"de-DE", "germanyValue"}
            };

            dictionary.Set(LocalizableString.DefaultLanguage, null);
            Assert.IsFalse(dictionary.ContainsExactLanguage(LocalizableString.DefaultLanguage), "Unspecified language should default to English generic");
            dictionary.Set(new CultureInfo("de-DE"), null);
            Assert.IsFalse(dictionary.ContainsExactLanguage(new CultureInfo("de-DE")));
        }

        [Test]
        public void TestSet()
        {
            var dictionary = new LocalizableStringCollection
            {
                "neutralValue",
                {"de-DE", "germanyValue"},
                // Intential duplicates (should be removed)
                "neutralValue",
                {"de-DE", "germanyValue"}
            };

            dictionary.Set(LocalizableString.DefaultLanguage, "neutralValue2");
            dictionary.Set(new CultureInfo("de-DE"), "germanyValue2");

            Assert.AreEqual("neutralValue2", dictionary.GetExactLanguage(LocalizableString.DefaultLanguage));
            Assert.AreEqual("germanyValue2", dictionary.GetExactLanguage(new CultureInfo("de-DE")));
        }

        [Test]
        public void TestGetExactLanguage()
        {
            var dictionary = new LocalizableStringCollection
            {
                "neutralValue",
                {"en-US", "americaValue"},
                {"en-GB", "gbValue"},
                {"de", "germanValue"},
                {"de-DE", "germanyValue"}
            };

            Assert.AreEqual("neutralValue", dictionary.GetExactLanguage(LocalizableString.DefaultLanguage), "Unspecified language should default to English generic");
            Assert.AreEqual("americaValue", dictionary.GetExactLanguage(new CultureInfo("en-US")));
            Assert.IsNull(dictionary.GetExactLanguage(new CultureInfo("en-CA")));
            Assert.AreEqual("gbValue", dictionary.GetExactLanguage(new CultureInfo("en-GB")));
            Assert.AreEqual("germanValue", dictionary.GetExactLanguage(new CultureInfo("de")));
            Assert.AreEqual("germanyValue", dictionary.GetExactLanguage(new CultureInfo("de-DE")));
            Assert.IsNull(dictionary.GetExactLanguage(new CultureInfo("de-AT")));
        }

        [Test]
        public void TestGetBestLanguage()
        {
            var dictionary = new LocalizableStringCollection
            {
                {"de", "germanValue"},
                {"de-DE", "germanyValue"},
                {"en-US", "americaValue"},
                {"en-GB", "gbValue"},
                "neutralValue"
            };

            Assert.AreEqual("neutralValue", dictionary.GetBestLanguage(LocalizableString.DefaultLanguage), "Unspecified language should default to English generic");
            Assert.AreEqual("americaValue", dictionary.GetBestLanguage(new CultureInfo("en-US")));
            Assert.AreEqual("neutralValue", dictionary.GetBestLanguage(new CultureInfo("en-CA")), "No exact match, should fall back to English generic");
            Assert.AreEqual("gbValue", dictionary.GetBestLanguage(new CultureInfo("en-GB")));
            Assert.AreEqual("germanValue", dictionary.GetBestLanguage(new CultureInfo("de")));
            Assert.AreEqual("germanyValue", dictionary.GetBestLanguage(new CultureInfo("de-DE")), "No exact match, should fall back to German generic");
            Assert.AreEqual("germanValue", dictionary.GetBestLanguage(new CultureInfo("de-AT")), "No exact match, should fall back to German generic");
            Assert.AreEqual("neutralValue", dictionary.GetBestLanguage(new CultureInfo("es-ES")), "No match, should fall back to English generic");

            dictionary.Set(LocalizableString.DefaultLanguage, null);
            Assert.AreEqual("americaValue", dictionary.GetBestLanguage(new CultureInfo("es-ES")), "No English generic, should fall back to English US");

            dictionary.Set(new CultureInfo("en-US"), null);
            Assert.AreEqual("germanValue", dictionary.GetBestLanguage(new CultureInfo("es-ES")), "No English US, should fall back to first entry in collection");
        }
    }
}
