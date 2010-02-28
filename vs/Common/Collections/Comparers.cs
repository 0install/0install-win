﻿//******************************
// Written by Peter Golde
// Copyright (c) 2004-2005, Wintellect
//
// Use and restribution of this code is subject to the license agreement 
// contained in the file "License.txt" accompanying this file.
//******************************

using System;
using System.Collections.Generic;
using System.Globalization;
using Common.Properties;

namespace Common.Collections
{
    /// <summary>
    /// A collection of methods to create IComparer and IEqualityComparer instances in various ways.
    /// </summary>
    internal static class Comparers
    {
        /// <summary>
        /// Class to change an IEqualityComparer&lt;TKey&gt; to an IEqualityComparer&lt;KeyValuePair&lt;TKey, TValue&gt;&gt; 
        /// Only the keys are compared.
        /// </summary>
        [Serializable]
        private class KeyValueEqualityComparer<TKey, TValue> : IEqualityComparer<KeyValuePair<TKey, TValue>>
        {
            private readonly IEqualityComparer<TKey> keyEqualityComparer;

            public KeyValueEqualityComparer(IEqualityComparer<TKey> keyEqualityComparer)
            {
                this.keyEqualityComparer = keyEqualityComparer;
            }

            public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
            {
                return keyEqualityComparer.Equals(x.Key, y.Key);
            }

            public int GetHashCode(KeyValuePair<TKey, TValue> obj)
            {
                return Util.GetHashCode(obj.Key, keyEqualityComparer);
            }

            public override bool Equals(object obj)
            {
                var comparer = obj as KeyValueEqualityComparer<TKey, TValue>;
                if (comparer == null) return false;
                else return Equals(keyEqualityComparer, comparer.keyEqualityComparer);
            }

            public override int GetHashCode()
            {
                return keyEqualityComparer.GetHashCode();
            }
        }

        /// <summary>
        /// Class to change an IComparer&lt;TKey&gt; to an IComparer&lt;KeyValuePair&lt;TKey, TValue&gt;&gt; 
        /// Only the keys are compared.
        /// </summary>
        [Serializable]
        private class KeyValueComparer<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>>
        {
            private readonly IComparer<TKey> keyComparer;

            public KeyValueComparer(IComparer<TKey> keyComparer)
            {
                this.keyComparer = keyComparer;
            }

            public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
            {
                return keyComparer.Compare(x.Key, y.Key);
            }

            public override bool Equals(object obj)
            {
                var value = obj as KeyValueComparer<TKey, TValue>;
                if (value != null)
                    return Equals(keyComparer, value.keyComparer);
                else
                    return false;
            }

            public override int GetHashCode()
            {
                return keyComparer.GetHashCode();
            }
        }

        /// <summary>
        /// Class to change an IComparer&lt;TKey&gt; and IComparer&lt;TValue&gt; to an IComparer&lt;KeyValuePair&lt;TKey, TValue&gt;&gt; 
        /// Keys are compared, followed by values.
        /// </summary>
        [Serializable]
        private class PairComparer<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>>
        {
            private readonly IComparer<TKey> keyComparer;
            private readonly IComparer<TValue> valueComparer;

            public PairComparer(IComparer<TKey> keyComparer, IComparer<TValue> valueComparer)
            {
                this.keyComparer = keyComparer;
                this.valueComparer = valueComparer;
            }

            public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
            {
                int keyCompare = keyComparer.Compare(x.Key, y.Key);

                if (keyCompare == 0)
                    return valueComparer.Compare(x.Value, y.Value);
                else
                    return keyCompare;
            }

            public override bool Equals(object obj)
            {
                var comparer = obj as PairComparer<TKey, TValue>;

                if (comparer == null) return false;
                else return Equals(keyComparer, comparer.keyComparer) &&
               Equals(valueComparer, comparer.valueComparer);
            }

            public override int GetHashCode()
            {
                return keyComparer.GetHashCode() ^ valueComparer.GetHashCode();
            }
        }

        /// <summary>
        /// Class to change an Comparison&lt;T&gt; to an IComparer&lt;T&gt;.
        /// </summary>
        [Serializable]
        private class ComparisonComparer<T> : IComparer<T>
        {
            private readonly Comparison<T> comparison;

            public ComparisonComparer(Comparison<T> comparison)
            {
                this.comparison = comparison;
            }

            public int Compare(T x, T y)
            {
                return comparison(x, y);
            }

            public override bool Equals(object obj)
            {
                var comparer = obj as ComparisonComparer<T>;
                if (comparer == null) return false;
                else return comparison.Equals(comparer.comparison);
            }

            public override int GetHashCode()
            {
                return comparison.GetHashCode();
            }
        }

        /// <summary>
        /// Class to change an Comparison&lt;TKey&gt; to an IComparer&lt;KeyValuePair&lt;TKey, TValue&gt;&gt;.
        /// GetHashCode cannot be used on this class.
        /// </summary>
        [Serializable]
        private class ComparisonKeyValueComparer<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>>
        {
            private readonly Comparison<TKey> comparison;

            public ComparisonKeyValueComparer(Comparison<TKey> comparison)
            {
                this.comparison = comparison;
            }

            public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
            {
                return comparison(x.Key, y.Key);
            }

            public override bool Equals(object obj)
            {
                var comparer = obj as ComparisonKeyValueComparer<TKey, TValue>;
                if (comparer == null) return false;
                else return comparison.Equals(comparer.comparison);
            }

            public override int GetHashCode()
            {
                return comparison.GetHashCode();
            }
        }


        /// <summary>
        /// Given an Comparison on a type, returns an IComparer on that type. 
        /// </summary>
        /// <typeparam name="T">T to compare.</typeparam>
        /// <param name="comparison">Comparison delegate on T</param>
        /// <returns>IComparer that uses the comparison.</returns>
        public static IComparer<T> ComparerFromComparison<T>(Comparison<T> comparison)
        {
            if (comparison == null)
                throw new ArgumentNullException("comparison");

            return new ComparisonComparer<T>(comparison);
        }

        /// <summary>
        /// Given an IComparer on TKey, returns an IComparer on
        /// key-value Pairs. 
        /// </summary>
        /// <typeparam name="TKey">TKey of the pairs</typeparam>
        /// <typeparam name="TValue">TValue of the apris</typeparam>
        /// <param name="keyComparer">IComparer on TKey</param>
        /// <returns>IComparer for comparing key-value pairs.</returns>
        public static IComparer<KeyValuePair<TKey, TValue>> ComparerKeyValueFromComparerKey<TKey, TValue>(IComparer<TKey> keyComparer)
        {
            if (keyComparer == null)
                throw new ArgumentNullException("keyComparer");

            return new KeyValueComparer<TKey, TValue>(keyComparer);
        }

        /// <summary>
        /// Given an IEqualityComparer on TKey, returns an IEqualityComparer on
        /// key-value Pairs. 
        /// </summary>
        /// <typeparam name="TKey">TKey of the pairs</typeparam>
        /// <typeparam name="TValue">TValue of the apris</typeparam>
        /// <param name="keyEqualityComparer">IComparer on TKey</param>
        /// <returns>IEqualityComparer for comparing key-value pairs.</returns>
        public static IEqualityComparer<KeyValuePair<TKey, TValue>> EqualityComparerKeyValueFromComparerKey<TKey, TValue>(IEqualityComparer<TKey> keyEqualityComparer)
        {
            if (keyEqualityComparer == null)
                throw new ArgumentNullException("keyEqualityComparer");

            return new KeyValueEqualityComparer<TKey, TValue>(keyEqualityComparer);
        }

        /// <summary>
        /// Given an IComparer on TKey and TValue, returns an IComparer on
        /// key-value Pairs of TKey and TValue, comparing first keys, then values. 
        /// </summary>
        /// <typeparam name="TKey">TKey of the pairs</typeparam>
        /// <typeparam name="TValue">TValue of the apris</typeparam>
        /// <param name="keyComparer">IComparer on TKey</param>
        /// <param name="valueComparer">IComparer on TValue</param>
        /// <returns>IComparer for comparing key-value pairs.</returns>
        public static IComparer<KeyValuePair<TKey, TValue>> ComparerPairFromKeyValueComparers<TKey, TValue>(IComparer<TKey> keyComparer, IComparer<TValue> valueComparer)
        {
            if (keyComparer == null)
                throw new ArgumentNullException("keyComparer");
            if (valueComparer == null)
                throw new ArgumentNullException("valueComparer");

            return new PairComparer<TKey, TValue>(keyComparer, valueComparer);
        }

        /// <summary>
        /// Given an Comparison on TKey, returns an IComparer on
        /// key-value Pairs. 
        /// </summary>
        /// <typeparam name="TKey">TKey of the pairs</typeparam>
        /// <typeparam name="TValue">TValue of the apris</typeparam>
        /// <param name="keyComparison">Comparison delegate on TKey</param>
        /// <returns>IComparer for comparing key-value pairs.</returns>
        public static IComparer<KeyValuePair<TKey, TValue>> ComparerKeyValueFromComparisonKey<TKey, TValue>(Comparison<TKey> keyComparison)
        {
            if (keyComparison == null)
                throw new ArgumentNullException("keyComparison");

            return new ComparisonKeyValueComparer<TKey, TValue>(keyComparison);
        }

        /// <summary>
        /// Given an element type, check that it implements IComparable&lt;T&gt; or IComparable, then returns
        /// a IComparer that can be used to compare elements of that type.
        /// </summary>
        /// <returns>The IComparer&lt;T&gt; instance.</returns>
        /// <exception cref="InvalidOperationException">T does not implement IComparable&lt;T&gt;.</exception>
        public static IComparer<T> DefaultComparer<T>()
        {
            if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)) ||
                typeof(IComparable).IsAssignableFrom(typeof(T)))
            {
                return Comparer<T>.Default;
            }
            else
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, Resources.UncomparableType, typeof(T).FullName));
            }
        }
    }
}