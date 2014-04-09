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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NanoByte.Common.Collections
{
    /// <summary>
    /// Contains test methods for <see cref="EnumerableExtensions"/>.
    /// </summary>
    [TestFixture]
    public class EnumerableExtensionsTest
    {
        #region Equality
        [Test]
        public void TestSequencedEqualsList()
        {
            Assert.IsTrue(new[] {"A", "B", "C"}.ToList().SequencedEquals(new[] {"A", "B", "C"}));
            Assert.IsTrue(new string[0].ToList().SequencedEquals(new string[0]));
            Assert.IsFalse(new[] {"A", "B", "C"}.ToList().SequencedEquals(new[] {"C", "B", "A"}));
            Assert.IsFalse(new[] {"A", "B", "C"}.ToList().SequencedEquals(new[] {"X", "Y", "Z"}));
            Assert.IsFalse(new[] {"A", "B", "C"}.ToList().SequencedEquals(new[] {"A", "B"}));
            Assert.IsFalse(new[] {new object()}.ToList().SequencedEquals(new[] {new object()}));
        }

        [Test]
        public void TestSequencedEqualsArray()
        {
            Assert.IsTrue(new[] {"A", "B", "C"}.SequencedEquals(new[] {"A", "B", "C"}));
            Assert.IsTrue(new string[0].SequencedEquals(new string[0]));
            Assert.IsFalse(new[] {"A", "B", "C"}.SequencedEquals(new[] {"C", "B", "A"}));
            Assert.IsFalse(new[] {"A", "B", "C"}.SequencedEquals(new[] {"X", "Y", "Z"}));
            Assert.IsFalse(new[] {"A", "B", "C"}.SequencedEquals(new[] {"A", "B"}));
            Assert.IsFalse(new[] {new object()}.SequencedEquals(new[] {new object()}));
        }

        [Test]
        public void TestUnsequencedEquals()
        {
            Assert.IsTrue(new[] {"A", "B", "C"}.UnsequencedEquals(new[] {"A", "B", "C"}));
            Assert.IsTrue(new string[0].UnsequencedEquals(new string[0]));
            Assert.IsTrue(new[] {"A", "B", "C"}.UnsequencedEquals(new[] {"C", "B", "A"}));
            Assert.IsFalse(new[] {"A", "B", "C"}.UnsequencedEquals(new[] {"X", "Y", "Z"}));
            Assert.IsFalse(new[] {"A", "B", "C"}.UnsequencedEquals(new[] {"A", "B"}));
            Assert.IsFalse(new[] {new object()}.UnsequencedEquals(new[] {new object()}));
        }

        [Test]
        public void TestGetSequencedHashCode()
        {
            Assert.AreEqual(new[] {"A", "B", "C"}.GetSequencedHashCode(), new[] {"A", "B", "C"}.GetSequencedHashCode());
            Assert.AreEqual(new string[0].GetSequencedHashCode(), new string[0].GetSequencedHashCode());
            Assert.AreNotEqual(new[] {"A", "B", "C"}.GetSequencedHashCode(), new[] {"C", "B", "A"}.GetSequencedHashCode());
            Assert.AreNotEqual(new[] {"A", "B", "C"}.GetSequencedHashCode(), new[] {"X", "Y", "Z"}.GetSequencedHashCode());
            Assert.AreNotEqual(new[] {"A", "B", "C"}.GetSequencedHashCode(), new[] {"A", "B"}.GetSequencedHashCode());
        }

        [Test]
        public void TestGetUnsequencedHashCode()
        {
            Assert.AreEqual(new[] {"A", "B", "C"}.GetUnsequencedHashCode(), new[] {"A", "B", "C"}.GetUnsequencedHashCode());
            Assert.AreEqual(new string[0].GetUnsequencedHashCode(), new string[0].GetUnsequencedHashCode());
            Assert.AreEqual(new[] {"A", "B", "C"}.GetUnsequencedHashCode(), new[] {"C", "B", "A"}.GetUnsequencedHashCode());
            Assert.AreNotEqual(new[] {"A", "B", "C"}.GetUnsequencedHashCode(), new[] {"X", "Y", "Z"}.GetUnsequencedHashCode());
            Assert.AreNotEqual(new[] {"A", "B", "C"}.GetUnsequencedHashCode(), new[] {"A", "B"}.GetUnsequencedHashCode());
        }
        #endregion

        #region Added elements
        /// <summary>
        /// Ensures that <see cref="EnumerableExtensions.GetAddedElements{T}(T[],T[])"/> correctly detects elements added to an ordered collection.
        /// </summary>
        [Test]
        public void TestGetAddedElements()
        {
            CollectionAssert.AreEqual(new[] {"B", "H"}, new[] {"A", "B", "C", "E", "G", "H"}.GetAddedElements(new[] {"A", "C", "E", "G"}));
            CollectionAssert.AreEqual(new[] {"C"}, new[] {"C", "D"}.GetAddedElements(new[] {"A", "D"}));
        }
        #endregion

        #region Transactions
        /// <summary>
        /// Ensures that <see cref="EnumerableExtensions.ApplyWithRollback{T}"/> correctly performs rollbacks on exceptions.
        /// </summary>
        [Test]
        public void TestApplyWithRollback()
        {
            var applyCalledFor = new List<int>();
            var rollbackCalledFor = new List<int>();
            Assert.Throws<ArgumentException>(() => new[] {1, 2, 3}.ApplyWithRollback(value =>
            {
                applyCalledFor.Add(value);
                if (value == 2) throw new ArgumentException("Test exception");
            }, rollbackCalledFor.Add), "Exceptions should be passed through after rollback.");

            CollectionAssert.AreEqual(new[] {1, 2}, applyCalledFor);
            CollectionAssert.AreEqual(new[] {2, 1}, rollbackCalledFor);
        }

        /// <summary>
        /// Ensures that <see cref="EnumerableExtensions.Try{T}"/> correctly handles fail conditions followed by success conditions.
        /// </summary>
        [Test]
        public void TestTrySucceed()
        {
            var actionCalledFor = new List<int>();
            new[] {1, 2, 3}.Try(value =>
            {
                actionCalledFor.Add(value);
                if (value == 1) throw new ArgumentException("Test exception");
            });

            CollectionAssert.AreEqual(new[] {1, 2}, actionCalledFor);
        }

        /// <summary>
        /// Ensures that <see cref="EnumerableExtensions.Try{T}"/> correctly handles pure fail conditions.
        /// </summary>
        [Test]
        public void TestTryFail()
        {
            var actionCalledFor = new List<int>();
            Assert.Throws<ArgumentException>(() => new[] {1, 2, 3}.Try(value =>
            {
                actionCalledFor.Add(value);
                throw new ArgumentException("Test exception");
            }), "Last exceptions should be passed through.");

            CollectionAssert.AreEqual(new[] {1, 2, 3}, actionCalledFor);
        }
        #endregion

        #region List
        /// <summary>
        /// Ensures that <see cref="EnumerableExtensions.RemoveLast{T}"/> correctly removes the last n elements from a list.
        /// </summary>
        [Test]
        public void TestRemoveLast()
        {
            var list = new List<string> {"a", "b", "c"};
            list.RemoveLast(2);
            CollectionAssert.AreEqual(new[] {"a"}, list);

            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveLast(-1));
        }
        #endregion
    }
}
