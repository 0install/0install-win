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
using System.ComponentModel;

namespace NanoByte.Common.Collections
{
    /// <summary>
    /// An equatable element that can be merged using 3-way merging.
    /// </summary>
    /// <typeparam name="T">The type the interface is being applied to.</typeparam>
    public interface IMergeable<T> : IEquatable<T>
    {
        /// <summary>
        /// A unique identifier used when comparing for merging. Should always remain the same, even when the element is modified.
        /// </summary>
        [DefaultValue(0)]
        string MergeID { get; }

        /// <summary>
        /// The time this element was last modified. This is used to determine preceedence with sync conflicts.
        /// </summary>
        /// <remarks>This value is ignored by clone and equality methods.</remarks>
        [DefaultValue(0)]
        DateTime Timestamp { get; set; }
    }
}
