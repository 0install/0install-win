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

using System.Globalization;
using System.IO;
using Common.Storage;
using NUnit.Framework;

namespace Common.Collections
{
    /// <summary>
    /// Contains test methods for <see cref="XmlLocalizableStringCollection"/>.
    /// </summary>
    [TestFixture]
    public class XmlLocalizableStringCollectionTest
    {
        /// <summary>
        /// Ensures that the class is correctly serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoad()
        {
            XmlLocalizableStringCollection collection1, collection2;
            string tempFile = null;
            try
            {
                tempFile = Path.GetTempFileName();

                // Write and read file
                collection1 = new XmlLocalizableStringCollection
                {
                    "value1",
                    {"value2", new CultureInfo("de-DE")}
                };
                XmlStorage.Save(tempFile, collection1);
                collection2 = XmlStorage.Load<XmlLocalizableStringCollection>(tempFile);
            }
            finally
            { // Clean up
                if (tempFile != null) File.Delete(tempFile);
            }

            // Ensure data stayed the same
            Assert.AreEqual(collection1, collection2, "Serialized objects should be equal.");
            Assert.AreEqual(collection1.GetSequencedHashCode(), collection2.GetSequencedHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(collection1, collection2), "Serialized objects should not return the same reference.");
        }

        [Test]
        public void TestContainsLanguage()
        {
            var dictionary = new XmlLocalizableStringCollection
            {
                "value1",
                {"value2", new CultureInfo("de-DE")}
            };
            Assert.IsTrue(dictionary.ContainsLanguage(null));
            Assert.IsTrue(dictionary.ContainsLanguage(new CultureInfo("de-DE")));
            Assert.IsFalse(dictionary.ContainsLanguage(new CultureInfo("en-US")));
        }

        [Test]
        public void TestGetLanguage()
        {
            var dictionary = new XmlLocalizableStringCollection
            {
                "value1",
                {"value2", new CultureInfo("de-DE")}
            };
            Assert.AreEqual("value1", dictionary.GetLanguage(null));
            Assert.AreEqual("value2", dictionary.GetLanguage(new CultureInfo("de-DE")));
        }
    }
}
