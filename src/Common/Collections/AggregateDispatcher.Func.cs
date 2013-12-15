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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Common.Collections
{
    /// <summary>
    /// Calls different function delegates (with enumerable return values) based on the runtime types of objects.
    /// Aggregates results when multiple delegates match a type (through inheritance).
    /// </summary>
    /// <typeparam name="TBase">The common base type of all objects to be dispatched.</typeparam>
    /// <typeparam name="TResultElement">The enumerable return values of the delegates.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class AggregateDispatcher<TBase, TResultElement> : IEnumerable<Func<TBase, IEnumerable<TResultElement>>>
        where TBase : class
    {
        private readonly List<Func<TBase, IEnumerable<TResultElement>>> _delegates = new List<Func<TBase, IEnumerable<TResultElement>>>();

        /// <summary>
        /// Adds a dispatch delegate.
        /// </summary>
        /// <typeparam name="TSpecific">The specific type to call the delegate for. Matches all subtypes as well.</typeparam>
        /// <param name="function">The delegate to call.</param>
        public void Add<TSpecific>(Func<TSpecific, IEnumerable<TResultElement>> function) where TSpecific : class, TBase
        {
            #region Sanity checks
            if (function == null) throw new ArgumentNullException("function");
            #endregion

            _delegates.Add(value =>
            {
                var specificValue = value as TSpecific;
                return specificValue == null ? null : function(specificValue);
            });
        }

        /// <summary>
        /// Dispatches an element to all delegates matching the type. Set up with <see cref="Add{TSpecific}"/> first.
        /// </summary>
        /// <param name="element">The element to be dispatched.</param>
        /// <returns>The values returned by all matching delegates aggregated.</returns>
        public IEnumerable<TResultElement> Dispatch(TBase element)
        {
            #region Sanity checks
            if (element == null) throw new ArgumentNullException("element");
            #endregion

            return _delegates.Select(del => del(element)).Where(x => x != null).SelectMany(x => x);
        }

        #region IEnumerable
        public IEnumerator<Func<TBase, IEnumerable<TResultElement>>> GetEnumerator()
        {
            return _delegates.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _delegates.GetEnumerator();
        }
        #endregion
    }
}
