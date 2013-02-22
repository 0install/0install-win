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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Common.Values.Design;
using NUnit.Framework;

namespace Common.Collections
{
    /// <summary>
    /// Contains test methods for <see cref="StringMappedCollection{T}"/>.
    /// </summary>
    [TestFixture]
    public class StringMappedCollectionTest
    {
        #region Test data class
        [TypeConverter(typeof(StringConstructorConverter<TestData>))]
        private sealed class TestData
        {
            private readonly string _data;

            public TestData(string data)
            {
                #region Sanity checks
                if (data == null) throw new ArgumentNullException("data");
                #endregion

                _data = data;
            }

            public override string ToString()
            {
                return _data;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(TestData)) return false;
                return ((TestData)obj)._data == _data;
            }

            public override int GetHashCode()
            {
                return _data.GetHashCode();
            }
        }
        #endregion

        [Test]
        public void TestToString()
        {
            var original = new List<TestData> {new TestData("data1"), new TestData("data2")};
            var mapped = new StringMappedCollection<TestData>(original);
            CollectionAssert.AreEqual(new[] {"data1", "data2"}, mapped);
        }

        [Test]
        public void TestFromString()
        {
            var original = new List<TestData> {new TestData("data1")};
            // ReSharper disable ObjectCreationAsStatement
            new StringMappedCollection<TestData>(original) {"data2", "data3"};
            // ReSharper restore ObjectCreationAsStatement
            CollectionAssert.AreEqual(new[] {new TestData("data1"), new TestData("data2"), new TestData("data3")}, original);
        }

        [Test]
        public void TestContains()
        {
            var original = new List<TestData> {new TestData("data1"), new TestData("data2")};
            var mapped = new StringMappedCollection<TestData>(original);
            Assert.IsTrue(mapped.Contains("data1"));
            Assert.IsTrue(mapped.Contains("data2"));
        }
    }
}
