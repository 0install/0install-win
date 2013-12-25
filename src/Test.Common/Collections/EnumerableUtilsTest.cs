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
using System.Linq;
using NUnit.Framework;

namespace Common.Collections
{
    /// <summary>
    /// Contains test methods for <see cref="EnumerableUtils"/>.
    /// </summary>
    [TestFixture]
    public class EnumerableUtilsTest
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
        /// Ensures that <see cref="EnumerableUtils.GetAddedElements{T}(T[],T[])"/> correctly detects elements added to an ordered collection.
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
        /// Ensures that <see cref="EnumerableUtils.ApplyWithRollback{T}"/> correctly performs rollbacks on exceptions.
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
        /// Ensures that <see cref="EnumerableUtils.Try{T}"/> correctly handles fail conditions followed by success conditions.
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
        /// Ensures that <see cref="EnumerableUtils.Try{T}"/> correctly handles pure fail conditions.
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

        #region Merge
        private class MergeTest : IMergeable<MergeTest>
        {
            public string MergeID { get; set; }

            public string Data { get; set; }

            public DateTime Timestamp { get; set; }

            public static IEnumerable<MergeTest> BuildList(params string[] mergeIDs)
            {
                return mergeIDs.Select(value => new MergeTest {MergeID = value});
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

        /// <summary>
        /// Ensures that <see cref="EnumerableUtils.Merge{T}(System.Collections.Generic.ICollection{T},System.Collections.Generic.ICollection{T},System.Action{T},System.Action{T})"/> correctly detects added and removed elements.
        /// </summary>
        [Test]
        public void TestMergeSimple()
        {
            var mineList = new[] {1, 2, 4};
            var theirsList = new[] {16, 8, 4};

            ICollection<int> toRemove = new List<int>();
            ICollection<int> toAdd = new List<int>();
            EnumerableUtils.Merge(theirsList, mineList, toAdd.Add, toRemove.Add);

            CollectionAssert.AreEqual(new[] {16, 8}, toAdd);
            CollectionAssert.AreEqual(new[] {1, 2}, toRemove);
        }

        /// <summary>
        /// Ensures that <see cref="EnumerableUtils.Merge{T}(System.Collections.Generic.IEnumerable{T},System.Collections.Generic.IEnumerable{T},System.Collections.Generic.IEnumerable{T},System.Action{T},System.Action{T})"/> correctly detects unchanged lists.
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
        /// Ensures that <see cref="EnumerableUtils.Merge{T}(System.Collections.Generic.IEnumerable{T},System.Collections.Generic.IEnumerable{T},System.Collections.Generic.IEnumerable{T},System.Action{T},System.Action{T})"/> correctly detects added and removed elements.
        /// </summary>
        [Test]
        public void TestMergeAddAndRemove()
        {
            var baseList = MergeTest.BuildList("a", "b", "c");
            var theirsList = MergeTest.BuildList("a", "b", "d");
            var mineList = MergeTest.BuildList("a", "c", "e");

            ICollection<MergeTest> toRemove = new List<MergeTest>();
            ICollection<MergeTest> toAdd = new List<MergeTest>();
            EnumerableUtils.Merge(baseList, theirsList, mineList, toAdd.Add, toRemove.Add);

            CollectionAssert.AreEqual(MergeTest.BuildList("d"), toAdd);
            CollectionAssert.AreEqual(MergeTest.BuildList("c"), toRemove);
        }

        /// <summary>
        /// Ensures that <see cref="EnumerableUtils.Merge{T}(System.Collections.Generic.IEnumerable{T},System.Collections.Generic.IEnumerable{T},System.Collections.Generic.IEnumerable{T},System.Action{T},System.Action{T})"/> correctly modified elements.
        /// </summary>
        [Test]
        public void TestMergeModify()
        {
            var baseList = MergeTest.BuildList("a", "b", "c", "d", "e");
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

            ICollection<MergeTest> toRemove = new List<MergeTest>();
            ICollection<MergeTest> toAdd = new List<MergeTest>();
            EnumerableUtils.Merge(baseList, theirsList, mineList, toAdd.Add, toRemove.Add);

            CollectionAssert.AreEqual(new[] {mineList[1], mineList[4]}, toRemove);
            CollectionAssert.AreEqual(new[] {theirsList[1], theirsList[4]}, toAdd);
        }
        #endregion

        #region List
        /// <summary>
        /// Ensures that <see cref="EnumerableUtils.RemoveLast{T}"/> correctly removes the last n elements from a list.
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
