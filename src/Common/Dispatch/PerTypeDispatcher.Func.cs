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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NanoByte.Common.Properties;

namespace NanoByte.Common.Dispatch
{
    /// <summary>
    /// Calls different function delegates (with return values) based on the runtime types of objects.
    /// Types must be exact matches. Inheritance is not considered.
    /// </summary>
    /// <typeparam name="TBase">The common base type of all objects to be dispatched.</typeparam>
    /// <typeparam name="TResult">The return value of the delegates.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class PerTypeDispatcher<TBase, TResult> : IEnumerable<KeyValuePair<Type, Func<TBase, TResult>>> where TBase : class
    {
        private readonly Dictionary<Type, Func<TBase, TResult>> _map = new Dictionary<Type, Func<TBase, TResult>>();

        /// <summary><see langword="true"/> to silently ignore dispatch attempts on unknown types; <see langword="false"/> to throw exceptions.</summary>
        private readonly bool _ignoreMissing;

        /// <summary>
        /// Creates a new dispatcher.
        /// </summary>
        /// <param name="ignoreMissing"><see langword="true"/> to return the default value (usually <see langword="null"/>) for dispatch attempts on unknown types; <see langword="false"/> to throw exceptions.</param>
        public PerTypeDispatcher(bool ignoreMissing)
        {
            _ignoreMissing = ignoreMissing;
        }

        /// <summary>
        /// Adds a dispatch delegate.
        /// </summary>
        /// <typeparam name="TSpecific">The specific type to call the delegate for. Does not match subtypes.</typeparam>
        /// <param name="function">The delegate to call.</param>
        public void Add<TSpecific>(Func<TSpecific, TResult> function) where TSpecific : TBase
        {
            #region Sanity checks
            if (function == null) throw new ArgumentNullException("function");
            #endregion

            _map.Add(typeof(TSpecific), obj => function((TSpecific)obj));
        }

        /// <summary>
        /// Dispatches an element to the delegate matching the type. Set up with <see cref="Add{TSpecific}"/> first.
        /// </summary>
        /// <param name="element">The element to be dispatched.</param>
        /// <returns>The value returned by the matching delegate.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no delegate matching the <paramref name="element"/> type was <see cref="Add{TSpecific}"/>ed and <see cref="_ignoreMissing"/> is <see langword="false"/>.</exception>
        public TResult Dispatch(TBase element)
        {
            #region Sanity checks
            if (element == null) throw new ArgumentNullException("element");
            #endregion

            var type = element.GetType();
            Func<TBase, TResult> function;
            if (_map.TryGetValue(type, out function)) return function(element);
            else
            {
                if (_ignoreMissing) return default(TResult);
                else throw new KeyNotFoundException(string.Format(Resources.MissingDispatchAction, type.Name));
            }
        }

        /// <summary>
        /// Calls <see cref="Dispatch(TBase)"/> for every element in a collection. Set up with <see cref="Add{TSpecific}"/> first.
        /// </summary>
        /// <param name="elements">The elements to be dispatched.</param>
        /// <returns>The values returned by the matching delegates.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no delegate matching one of the element types was <see cref="Add{TSpecific}"/>ed and <see cref="_ignoreMissing"/> is <see langword="false"/>.</exception>
        public IEnumerable<TResult> Dispatch(IEnumerable<TBase> elements)
        {
            #region Sanity checks
            if (elements == null) throw new ArgumentNullException("elements");
            #endregion

            return elements.Select(Dispatch);
        }

        #region IEnumerable
        public IEnumerator<KeyValuePair<Type, Func<TBase, TResult>>> GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _map.GetEnumerator();
        }
        #endregion
    }
}
