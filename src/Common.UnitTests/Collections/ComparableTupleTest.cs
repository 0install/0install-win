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
    /// Contains test methods for <see cref="ComparableTuple{T}"/>.
    /// </summary>
    [TestFixture]
    public class ComparableTupleTest
    {
        [Test(Description = "Ensures tuples are compared correctly.")]
        public void TestComparTo()
        {
            var tuple1 = new ComparableTuple<int>(1, 1);
            var tuple2 = new ComparableTuple<int>(1, 2);
            var tuple3 = new ComparableTuple<int>(2, 1);

            Assert.AreEqual(tuple1, tuple1);

            Assert.That(tuple1, Is.LessThan(tuple2));
            Assert.That(tuple2, Is.LessThan(tuple3));
            Assert.That(tuple3, Is.GreaterThan(tuple2));
            Assert.That(tuple2, Is.GreaterThan(tuple1));
        }
    }
}
