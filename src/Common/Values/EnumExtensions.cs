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
using System.Diagnostics.CodeAnalysis;

namespace NanoByte.Common.Values
{
    /// <summary>
    /// Contains extension methods for <see cref="Enum"/>s.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Checks whether a flag is set.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag"), SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flag")]
        public static bool HasFlag(this Enum enumRef, Enum flag)
        {
            long enumValue = Convert.ToInt64(enumRef);
            long flagVal = Convert.ToInt64(flag);
            return (enumValue & flagVal) == flagVal;
        }

        /// <summary>
        /// Checks whether a flag is set.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag"), SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flag")]
        [CLSCompliant(false)]
        public static bool HasFlag(this ushort enumRef, ushort flag)
        {
            return (enumRef & flag) == flag;
        }

        /// <summary>
        /// Checks whether a flag is set.
        /// </summary>
        public static bool HasFlag(this int enumRef, int flag)
        {
            return (enumRef & flag) == flag;
        }
    }
}