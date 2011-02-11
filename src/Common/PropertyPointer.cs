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
    /// Wraps delegate-based access to a value as a property.
    /// </summary>
    /// <typeparam name="T">The type of value the property contains.</typeparam>
    public sealed class PropertyPointer<T> : MarshalByRefObject
    {
        private readonly SimpleResult<T> _getValue;
        private readonly Action<T> _setValue;

        /// <summary>
        /// Transparent access to the wrapper value.
        /// </summary>
        public T Value { get { return _getValue(); } set { _setValue(value); } }

        /// <summary>
        /// Creates a property pointer.
        /// </summary>
        /// <param name="getValue">A delegate that returns the current value.</param>
        /// <param name="setValue">A delegate that sets the valuel.</param>
        public PropertyPointer(SimpleResult<T> getValue, Action<T> setValue)
        {
            #region Sanity checks
            if (getValue == null) throw new ArgumentNullException("getValue");
            if (setValue == null) throw new ArgumentNullException("setValue");
            #endregion

            _getValue = getValue;
            _setValue = setValue;
        }
    }
}
