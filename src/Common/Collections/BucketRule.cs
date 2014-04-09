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

namespace NanoByte.Common.Collections
{
    /// <summary>
    /// A rule for <see cref="Bucketizer{T}"/>.
    /// </summary>
    public class BucketRule<T>
    {
        /// <summary>
        /// A condition to check elements against.
        /// </summary>
        public readonly Predicate<T> Predicate;

        /// <summary>
        /// The collection elements are added to if they match the <see cref="Predicate"/>.
        /// </summary>
        public readonly ICollection<T> Bucket;

        /// <summary>
        /// Creates a new bucket rule.
        /// </summary>
        /// <param name="predicate">A condition to check elements against.</param>
        /// <param name="bucket">The collection elements are added to if they match the <paramref name="predicate"/>.</param>
        public BucketRule(Predicate<T> predicate, ICollection<T> bucket)
        {
            #region Sanity checks
            if (predicate == null) throw new ArgumentNullException("predicate");
            if (bucket == null) throw new ArgumentNullException("bucket");
            #endregion

            Predicate = predicate;
            Bucket = bucket;
        }
    }
}
