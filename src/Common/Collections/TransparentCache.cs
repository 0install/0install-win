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

namespace Common.Collections
{
    /// <summary>
    /// Transparently caches retrieval requests, passed through to a callback on first request.
    /// </summary>
    /// <typeparam name="TKey">The type of keys used to request values.</typeparam>
    /// <typeparam name="TValue">The type of values returned.</typeparam>
    public class TransparentCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _lookup = new Dictionary<TKey, TValue>();

        private readonly Func<TKey, TValue> _retriever;

        /// <summary>
        /// Creates a new transparent cache.
        /// </summary>
        /// <param name="retriever">The callback used to retrieve values not yet in the cache.</param>
        public TransparentCache(Func<TKey, TValue> retriever)
        {
            _retriever = retriever;
        }

        /// <summary>
        /// Retrieves a value from the cache.
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                lock (_lookup)
                {
                    TValue result;
                    if (!_lookup.TryGetValue(key, out result))
                        _lookup.Add(key, result = _retriever(key));
                    return result;
                }
            }
        }
    }
}
