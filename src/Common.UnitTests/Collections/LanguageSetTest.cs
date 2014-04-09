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

using NUnit.Framework;

namespace NanoByte.Common.Collections
{
    /// <summary>
    /// Contains test methods for <see cref="LanguageSet"/>.
    /// </summary>
    [TestFixture]
    public class LanguageSetTest
    {
        [Test]
        public void TestToString()
        {
            var collection = new LanguageSet {"en-US", "de"};
            Assert.AreEqual("de en_US", collection.ToString());
        }

        [Test]
        public void TestFromString()
        {
            CollectionAssert.AreEquivalent(new LanguageSet {"de", "en-US"}, new LanguageSet("en_US de"));
        }

        [Test]
        public void TestDuplicateDetection()
        {
            var collection = new LanguageSet("en_US");
            Assert.IsFalse(collection.Add("en-US"));
            CollectionAssert.AreEquivalent(new LanguageSet {"en-US"}, collection);
        }

        [Test]
        public void TestIsCompatible()
        {
            Assert.IsTrue(new LanguageSet {"de", "en"}.ContainsAny(new LanguageSet {"en", "fr"}));
            Assert.IsTrue(new LanguageSet {"en", "fr"}.ContainsAny(new LanguageSet {"de", "en"}));

            Assert.IsTrue(new LanguageSet().ContainsAny(new LanguageSet {"de"}));
            Assert.IsTrue(new LanguageSet {"de"}.ContainsAny(new LanguageSet()));

            Assert.IsFalse(new LanguageSet {"de", "en"}.ContainsAny(new LanguageSet {"fr"}));
            Assert.IsFalse(new LanguageSet {"fr"}.ContainsAny(new LanguageSet {"de", "en"}));
        }
    }
}
