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

namespace NanoByte.Common.Utils
{
    /// <summary>
    /// Provides neat little code-shortcuts for updating properties.
    /// </summary>
    public static class UpdateUtils
    {
        #region Ref bool

        #region Struct
        /// <summary>
        /// Updates a value and sets a boolean flag to <see langword="true"/> if the original value actually changed
        /// </summary>
        /// <typeparam name="T">The type of data to update</typeparam>
        /// <param name="value">The new value</param>
        /// <param name="original">The original value to update</param>
        /// <param name="updated">Gets set to <see langword="true"/> if value is different from original</param>
        public static void To<T>(this T value, ref T original, ref bool updated) where T : struct
        {
            // If the values already match, nothing needs to be done
            if (original.Equals(value)) return;

            // Otherwise set the "updated" flag and change the value
            updated = true;
            original = value;
        }

        /// <summary>
        /// Updates a value and sets two boolean flags to <see langword="true"/> if the original value actually changed
        /// </summary>
        /// <typeparam name="T">The type of data to update</typeparam>
        /// <param name="value">The new value</param>
        /// <param name="original">The original value to update</param>
        /// <param name="updated1">Gets set to <see langword="true"/> if value is different from original</param>
        /// <param name="updated2">Gets set to <see langword="true"/> if value is different from original</param>
        public static void To<T>(this T value, ref T original, ref bool updated1, ref bool updated2) where T : struct
        {
            // If the values already match, nothing needs to be done
            if (original.Equals(value)) return;

            updated1 = true;
            updated2 = true;
            original = value;
        }
        #endregion

        #region String
        /// <summary>
        /// Updates a value and sets a boolean flag to <see langword="true"/> if the original value actually changed
        /// </summary>
        /// <param name="value">The new value</param>
        /// <param name="original">The original value to update</param>
        /// <param name="updated">Gets set to <see langword="true"/> if value is different from original</param>
        public static void To(this string value, ref string original, ref bool updated)
        {
            // If the values already match, nothing needs to be done
            if (original == value) return;

            // Otherwise set the "updated" flag and change the value
            updated = true;
            original = value;
        }

        /// <summary>
        /// Updates a value and sets two boolean flags to <see langword="true"/> if the original value actually changed
        /// </summary>
        /// <param name="value">The new value</param>
        /// <param name="original">The original value to update</param>
        /// <param name="updated1">Gets set to <see langword="true"/> if value is different from original</param>
        /// <param name="updated2">Gets set to <see langword="true"/> if value is different from original</param>
        public static void To(this string value, ref string original, ref bool updated1, ref bool updated2)
        {
            if (original == value) return;

            updated1 = true;
            updated2 = true;
            original = value;
        }
        #endregion

        #endregion

        #region Exec Delegate

        #region Struct
        /// <summary>
        /// Updates a value and calls back a delegate if the original value actually changed
        /// </summary>
        /// <typeparam name="T">The type of data to update</typeparam>
        /// <param name="value">The new value</param>
        /// <param name="original">The original value to update</param>
        /// <param name="updated">Gets called if value is different from original</param>
        public static void To<T>(this T value, ref T original, Action updated = null) where T : struct
        {
            // If the values already match, nothing needs to be done
            if (original.Equals(value)) return;

            // Backup the original value in case it needs to be reverted
            T backup = original;

            // Set the new value
            original = value;

            if (updated != null)
            {
                // Execute the "updated" delegate
                try
                {
                    updated();
                }
                catch
                {
                    // Restore the original value before passing exceptions upwards
                    original = backup;
                    throw;
                }
            }
        }
        #endregion

        #region String
        /// <summary>
        /// Updates a value and calls back a delegate if the original value actually changed
        /// </summary>
        /// <param name="value">The new value</param>
        /// <param name="original">The original value to update</param>
        /// <param name="updated">Gets called if value is different from original</param>
        public static void To(this string value, ref string original, Action updated)
        {
            // If the values already match, nothing needs to be done
            if (original == value) return;

            // Backup the original value in case it needs to be reverted
            string backup = original;

            // Set the new value
            original = value;

            if (updated != null)
            {
                // Execute the "updated" delegate
                try
                {
                    updated();
                }
                catch
                {
                    // Restore the original value before passing exceptions upwards
                    original = backup;
                    throw;
                }
            }
        }
        #endregion

        #endregion

        //--------------------//

        #region Swap
        /// <summary>
        /// Swaps the content of two fields
        /// </summary>
        /// <typeparam name="T">The type of objects to swap</typeparam>
        /// <param name="value1">The first field which will afterwards carry the content of <paramref name="value2"/></param>
        /// <param name="value2">The first field which will afterwards carry the content of <paramref name="value1"/></param>
        public static void Swap<T>(ref T value1, ref T value2)
        {
            T tempValue = value1;
            value1 = value2;
            value2 = tempValue;
        }
        #endregion
    }
}
