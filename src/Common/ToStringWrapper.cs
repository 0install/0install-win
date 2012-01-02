/*
 * Copyright 2006-2011 Bastian Eicher
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

namespace Common
{
    /// <summary>
    /// Replaces the <see cref="ToString"/> method of an object with a custom deleagte.
    /// </summary>
    public class ToStringWrapper<T>
    {
        /// <summary>
        /// The element being wrapped.
        /// </summary>
        public readonly T Element;

        private readonly SimpleResult<string> _toString;

        /// <summary>
        /// Creates a new wrapper.
        /// </summary>
        /// <param name="element">The element being wrapped.</param>
        /// <param name="toString">The method to be called for the <see cref="ToString"/> result.</param>
        public ToStringWrapper(T element, SimpleResult<string> toString)
        {
            #region Sanity checks
            if (toString == null) throw new ArgumentNullException("toString");
            #endregion

            Element = element;
            _toString = toString;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _toString();
        }
    }
}
