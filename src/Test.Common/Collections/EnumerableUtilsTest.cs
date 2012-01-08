/*
 * Copyright 2006-2012 Bastian Eicher
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
using NUnit.Framework;

namespace Common.Collections
{
    /// <summary>
    /// Contains test methods for <see cref="EnumerableUtils"/>.
    /// </summary>
    [TestFixture]
    public class EnumerableUtilsTest
    {
        /// <summary>
        /// Ensures that <see cref="EnumerableUtils.GetFirst{T}"/> correctly returns the first element of a collection or <see langword="null"/> if it is empty.
        /// </summary>
        [Test]
        public void TestGetFirst()
        {
            Assert.AreEqual("first", EnumerableUtils.GetFirst(new[] {"first", "second"}));
            Assert.IsNull(EnumerableUtils.GetFirst(new string[0]));
        }

        /// <summary>
        /// Ensures that <see cref="EnumerableUtils.IsEmpty{T}"/> correctly determines whether a collection is empty.
        /// </summary>
        [Test]
        public void TestIsEmpty()
        {
            Assert.IsTrue(EnumerableUtils.IsEmpty(new int[0]));
            Assert.IsFalse(EnumerableUtils.IsEmpty(new[] {1, 2, 3}));
        }

        /// <summary>
        /// Ensures that <see cref="EnumerableUtils.OfType{TResult}"/> correctly filters out elements of specific types.
        /// </summary>
        [Test]
        public void TestOfType()
        {
            var source = new object[] {1, "a", 2, "b", 3, "c"};
            CollectionAssert.AreEqual(new[] {1, 2, 3}, EnumerableUtils.OfType<int>(source));
        }

        /// <summary>
        /// Ensures that <see cref="EnumerableUtils.GetAddedElements{T}(T[],T[])"/> correctly detects elements added to an ordered collection.
        /// </summary>
        [Test]
        public void TestGetAddedElements()
        {
            CollectionAssert.AreEqual(new[] {"B", "H"}, EnumerableUtils.GetAddedElements(new[] {"A", "C", "E", "G"}, new[] {"A", "B", "C", "E", "G", "H"}));
            CollectionAssert.AreEqual(new[] {"C"}, EnumerableUtils.GetAddedElements(new[] {"A", "D"}, new[] {"C", "D"}));
        }

        /// <summary>
        /// Ensures that <see cref="EnumerableUtils.ApplyWithRollback{T}"/> correctly performs rollbacks on exceptions.
        /// </summary>
        [Test]
        public void TestApplyWithRollback()
        {
            var applyCalledFor = new LinkedList<int>();
            var rollbackCalledFor = new LinkedList<int>();
            Assert.Throws<Exception>(() => EnumerableUtils.ApplyWithRollback(new[] {1, 2, 3},
                value =>
                {
                    applyCalledFor.AddLast(value);
                    if (value == 2) throw new Exception("Test exception");
                },
                value => rollbackCalledFor.AddLast(value)),
                "Exceptions should be passed through after rollback.");

            CollectionAssert.AreEqual(new[] {1, 2}, applyCalledFor);
            CollectionAssert.AreEqual(new[] {2, 1}, rollbackCalledFor);
        }

        #region Private inner class
        private class MergeTest : IMergeable<MergeTest>
        {
            public string MergeID { get; set; }

            public string Data { get; set; }

            public DateTime Timestamp { get; set; }

            public static MergeTest[] BuildArray(params string[] mergeIDs)
            {
                return Array.ConvertAll(mergeIDs, value => new MergeTest {MergeID = value});
            }

            public override string ToString()
            {
                return MergeID + " (" + Data + ")";
            }

            #region Equality
            public bool Equals(MergeTest other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other.MergeID, MergeID) && Equals(other.Data, Data);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == typeof(MergeTest) && Equals((MergeTest)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((MergeID != null ? MergeID.GetHashCode() : 0) * 397) ^ (Data != null ? Data.GetHashCode() : 0);
                }
            }
            #endregion
        }
        #endregion

        /// <summary>
        /// Ensures that <see cref="EnumerableUtils.Merge{T}(IEnumerable{T},IEnumerable{T},Action{T},Action{T})"/> correctly detects added and removed elements.
        /// </summary>
        [Test]
        public void TestMergeSimple()
        {
            var mineList = new[] {1, 2, 4};
            var theirsList = new[] {16, 8, 4};

            ICollection<int> toRemove = new LinkedList<int>();
            ICollection<int> toAdd = new LinkedList<int>();
            EnumerableUtils.Merge(theirsList, mineList, toAdd.Add, toRemove.Add);

            CollectionAssert.AreEqual(new[] {16, 8}, toAdd);
            CollectionAssert.AreEqual(new[] {1, 2}, toRemove);
        }

        /// <summary>
        /// Ensures that <see cref="EnumerableUtils.Merge{T}(IEnumerable{T},IEnumerable{T},Action{T},Action{T})"/> correctly detects unchanged lists.
        /// </summary>
        [Test]
        public void TestMergeEquals()
        {
            var list = new[] {new MergeTest {MergeID = "1"}};

            EnumerableUtils.Merge(new MergeTest[0], list, list,
                delegate(MergeTest element) { throw new AssertionException(element + " should not be detected as added."); },
                delegate(MergeTest element) { throw new AssertionException(element + " should not be detected as removed."); });
        }

        /// <summary>
        /// Ensures that <see cref="EnumerableUtils.Merge{T}(IEnumerable{T},IEnumerable{T},Action{T},Action{T})"/> correctly detects added and removed elements.
        /// </summary>
        [Test]
        public void TestMergeAddAndRemove()
        {
            var baseList = MergeTest.BuildArray("a", "b", "c");
            var theirsList = MergeTest.BuildArray("a", "b", "d");
            var mineList = MergeTest.BuildArray("a", "c", "e");

            ICollection<MergeTest> toRemove = new LinkedList<MergeTest>();
            ICollection<MergeTest> toAdd = new LinkedList<MergeTest>();
            EnumerableUtils.Merge(baseList, theirsList, mineList, toAdd.Add, toRemove.Add);

            CollectionAssert.AreEqual(MergeTest.BuildArray("d"), toAdd);
            CollectionAssert.AreEqual(MergeTest.BuildArray("c"), toRemove);
        }

        /// <summary>
        /// Ensures that <see cref="EnumerableUtils.Merge{T}(IEnumerable{T},IEnumerable{T},Action{T},Action{T})"/> correctly modified elements.
        /// </summary>
        [Test]
        public void TestMergeModify()
        {
            var baseList = MergeTest.BuildArray("a", "b", "c", "d", "e");
            var theirsList = new[]
            {
                new MergeTest {MergeID = "a"},
                new MergeTest {MergeID = "b", Data = "123", Timestamp = new DateTime(2000, 1, 1)},
                new MergeTest {MergeID = "c"},
                new MergeTest {MergeID = "d", Data = "456", Timestamp = new DateTime(2000, 1, 1)},
                new MergeTest {MergeID = "e", Data = "789", Timestamp = new DateTime(2999, 1, 1)}
            };
            var mineList = new[]
            {
                new MergeTest {MergeID = "a"},
                new MergeTest {MergeID = "b"},
                new MergeTest {MergeID = "c", Data = "abc", Timestamp = new DateTime(2000, 1, 1)},
                new MergeTest {MergeID = "d", Data = "def", Timestamp = new DateTime(2999, 1, 1)},
                new MergeTest {MergeID = "e", Data = "ghi", Timestamp = new DateTime(2000, 1, 1)}
            };

            ICollection<MergeTest> toRemove = new LinkedList<MergeTest>();
            ICollection<MergeTest> toAdd = new LinkedList<MergeTest>();
            EnumerableUtils.Merge(baseList, theirsList, mineList, toAdd.Add, toRemove.Add);

            CollectionAssert.AreEqual(new[] {mineList[1], mineList[4]}, toRemove);
            CollectionAssert.AreEqual(new[] {theirsList[1], theirsList[4]}, toAdd);
        }
    }
}
