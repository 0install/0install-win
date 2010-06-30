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

using Common.Storage;
using NUnit.Framework;

namespace Common.Collections
{
    /// <summary>
    /// Contains test methods for <see cref="LanguageCollection"/>.
    /// </summary>
    [TestFixture]
    public class LanguageCollectionTest
    {
        /// <summary>
        /// Ensures that the class is correctly serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoad()
        {
            var collection1 = new LanguageCollection {"en-US", "de"};

            // Serialize and deserialize data
            string data = XmlStorage.ToString(collection1);
            var collection2 = XmlStorage.FromString<LanguageCollection>(data);

            // Ensure data stayed the same
            Assert.AreEqual(collection1, collection2, "Serialized objects should be equal.");
            Assert.AreEqual(collection1.GetSequencedHashCode(), collection2.GetSequencedHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(collection1, collection2), "Serialized objects should not return the same reference.");
        }

        [Test]
        public void TestToString()
        {
            var collection = new LanguageCollection { "en-US", "de" };
            Assert.AreEqual("de en_US", collection.ToString());
        }

        [Test]
        public void TestFromString()
        {
            var collection = LanguageCollection.FromString("en_US de");
            CollectionAssert.AreEquivalent(new LanguageCollection {"de", "en-US"}, collection);
        }
    }
}
