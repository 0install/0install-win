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

namespace Common.Collections
{
    /// <summary>
    /// Calls different delegates based on the runtime types of objects.
    /// </summary>
    /// <typeparam name="TBase">The common base type of all objects to be dispatched.</typeparam>
    /// <remarks>Types must be exact matches. Inheritance is not considered.</remarks>
    public class PerTypeDispatcher<TBase> : Dictionary<Type, Action<object>> where TBase : class
    {
        /// <summary>
        /// Adds a dispatch delegate.
        /// </summary>
        /// <typeparam name="TSpecific">The specific type to call the delegate for.</typeparam>
        /// <param name="action">The delegate to call.</param>
        public void Add<TSpecific>(Action<TSpecific> action) where TSpecific : TBase
        {
            Add(typeof(TSpecific), obj => action((TSpecific)obj));
        }

        /// <summary>
        /// Dispatches an element to the delegate matching the type.
        /// </summary>
        /// <param name="element">The element to be dispatched.</param>
        public void Dispatch(TBase element)
        {
            #region Sanity checks
            if (element == null) throw new ArgumentNullException("element");
            #endregion

            this[element.GetType()](element);
        }

        /// <summary>
        /// Calls <see cref="Dispatch(TBase)"/> for every element in a collection.
        /// </summary>
        /// <param name="elements">The elements to be dispatched.</param>
        public void Dispatch(IEnumerable<TBase> elements)
        {
            #region Sanity checks
            if (elements == null) throw new ArgumentNullException("elements");
            #endregion

            foreach (var element in elements)
                Dispatch(element);
        }
    }
}
