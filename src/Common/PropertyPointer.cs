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
using System.Globalization;

namespace NanoByte.Common
{
    /// <summary>
    /// Wraps delegate-based access to a value as a property.
    /// </summary>
    /// <typeparam name="T">The type of value the property contains.</typeparam>
    public sealed class PropertyPointer<T> : MarshalByRefObject
    {
        private readonly Func<T> _getValue;
        private readonly Action<T> _setValue;

        /// <summary>
        /// Transparent access to the wrapper value.
        /// </summary>
        public T Value { get { return _getValue(); } set { _setValue(value); } }

        private readonly T _defaultValue;

        /// <summary>
        /// The default value of the property.
        /// </summary>
        public T DefaultValue { get { return _defaultValue; } }

        private readonly bool _needsEncoding;

        /// <summary>
        /// Indicates that this property needs to be encoded (e.g. as base64) before it can be stored in a file.
        /// </summary>
        public bool NeedsEncoding { get { return _needsEncoding; } }

        /// <summary>
        /// Creates a property pointer.
        /// </summary>
        /// <param name="getValue">A delegate that returns the current value.</param>
        /// <param name="setValue">A delegate that sets the valuel.</param>
        /// <param name="defaultValue">The default value of the property</param>
        /// <param name="needsEncoding">Indicates that this property needs to be encoded (e.g. as base64) before it can be stored in a file.</param>
        public PropertyPointer(Func<T> getValue, Action<T> setValue, T defaultValue = default(T), bool needsEncoding = false)
        {
            #region Sanity checks
            if (getValue == null) throw new ArgumentNullException("getValue");
            if (setValue == null) throw new ArgumentNullException("setValue");
            #endregion

            _getValue = getValue;
            _setValue = setValue;
            _defaultValue = defaultValue;
            _needsEncoding = needsEncoding;
        }
    }

    /// <summary>
    /// Provides factory methods for <see cref="PropertyPointer{T}"/>.
    /// </summary>
    public static class PropertyPointer
    {
        /// <summary>
        /// Wraps a <see cref="bool"/> pointer in a <see cref="string"/> pointer.
        /// </summary>
        public static PropertyPointer<string> ToStringPointer(this PropertyPointer<bool> pointer)
        {
            #region Sanity checks
            if (pointer == null) throw new ArgumentNullException("pointer");
            #endregion

            return new PropertyPointer<string>(
                getValue: () => pointer.Value.ToString(CultureInfo.InvariantCulture),
                setValue: value => pointer.Value = (value == "1" || (value != "0" && bool.Parse(value))),
                defaultValue: pointer.DefaultValue.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Wraps an <see cref="int"/> pointer in a <see cref="string"/> pointer.
        /// </summary>
        public static PropertyPointer<string> ToStringPointer(this PropertyPointer<int> pointer)
        {
            #region Sanity checks
            if (pointer == null) throw new ArgumentNullException("pointer");
            #endregion

            return new PropertyPointer<string>(
                getValue: () => pointer.Value.ToString(CultureInfo.InvariantCulture),
                setValue: value => pointer.Value = int.Parse(value),
                defaultValue: pointer.DefaultValue.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Wraps an <see cref="long"/> pointer in a <see cref="string"/> pointer.
        /// </summary>
        public static PropertyPointer<string> ToStringPointer(this PropertyPointer<long> pointer)
        {
            #region Sanity checks
            if (pointer == null) throw new ArgumentNullException("pointer");
            #endregion

            return new PropertyPointer<string>(
                getValue: () => pointer.Value.ToString(CultureInfo.InvariantCulture),
                setValue: value => pointer.Value = long.Parse(value),
                defaultValue: pointer.DefaultValue.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Wraps a <see cref="TimeSpan"/> pointer in a <see cref="string"/> pointer.
        /// </summary>
        public static PropertyPointer<string> ToStringPointer(this PropertyPointer<TimeSpan> pointer)
        {
            #region Sanity checks
            if (pointer == null) throw new ArgumentNullException("pointer");
            #endregion

            return new PropertyPointer<string>(
                getValue: () => ((int)pointer.Value.TotalSeconds).ToString(CultureInfo.InvariantCulture),
                setValue: value => pointer.Value = TimeSpan.FromSeconds(int.Parse(value)),
                defaultValue: ((int)pointer.DefaultValue.TotalSeconds).ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Wraps an <see cref="Uri"/> pointer in a <see cref="string"/> pointer.
        /// </summary>
        public static PropertyPointer<string> ToStringPointer(this PropertyPointer<Uri> pointer)
        {
            #region Sanity checks
            if (pointer == null) throw new ArgumentNullException("pointer");
            #endregion

            return new PropertyPointer<string>(
                getValue: () => (pointer.Value == null) ? null : pointer.Value.ToString(),
                setValue: value => pointer.Value = (value == null) ? null : new Uri(value),
                defaultValue: (pointer.DefaultValue == null) ? null : pointer.DefaultValue.OriginalString);
        }
    }
}
