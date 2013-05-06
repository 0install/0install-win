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

using System.Collections.Generic;
using Moq;
using NUnit.Framework.Constraints;

namespace Common
{
    /// <summary>
    /// Custom matchers for <see cref="Moq"/>.
    /// </summary>
    public static class Matchers
    {
        /// <summary>
        /// Ensures a collection is equal to this one (same elements in same order).
        /// </summary>
        [Matcher]
        public static IEnumerable<T> IsEqual<T>(this IEnumerable<T> expected)
        {
            return Match.Create<IEnumerable<T>>(new EqualConstraint(expected).Matches);
        }

        /// <summary>
        /// Ensures a collection is equivalet to this one (same elements in any order).
        /// </summary>
        [Matcher]
        public static IEnumerable<T> IsEquivalent<T>(this IEnumerable<T> expected)
        {
            return Match.Create<IEnumerable<T>>(new CollectionEquivalentConstraint(expected).Matches);
        }
    }
}
