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
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Common.Utils
{
    /// <summary>
    /// Provides helper methods for <see cref="Attribute"/>s.
    /// </summary>
    public static class AttributeUtils
    {
        /// <summary>
        /// Gets the first <typeparamref name="TAttribute"/> attribute set on the <typeparamref name="TTarget"/> type.
        /// </summary>
        /// <returns>Falls back to <see cref="object.ToString"/> if the attribute is missing.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static TAttribute GetAttribute<TAttribute, TTarget>() where TAttribute : Attribute
        {
            var attributes = typeof(TTarget).GetCustomAttributes(typeof(TAttribute), true);
            return attributes.OfType<TAttribute>().FirstOrDefault();
        }

        /// <summary>
        /// Gets the first <typeparamref name="TAttribute"/> attribute set on the <paramref name="target"/> enum value.
        /// Then retrieves a value from the attribute using <paramref name="valueRetriever"/>.
        /// </summary>
        /// <returns>Falls back to <see cref="object.ToString"/> if the attribute is missing.</returns>
        public static string GetEnumAttributeValue<TAttribute>(this Enum target, Converter<TAttribute, string> valueRetriever) where TAttribute : Attribute
        {
            #region Sanity checks
            if (target == null) throw new ArgumentNullException("target");
            if (valueRetriever == null) throw new ArgumentNullException("valueRetriever");
            #endregion

            FieldInfo fieldInfo = target.GetType().GetField(target.ToString());
            var attributes = (TAttribute[])fieldInfo.GetCustomAttributes(typeof(TAttribute), true);
            var attribute = attributes.FirstOrDefault();
            return (attribute == null) ? target.ToString() : valueRetriever(attribute);
        }

        /// <summary>
        /// Uses the type converter for <typeparamref name="TType"/> (set by <see cref="TypeConverterAttribute"/>) to parse a string.
        /// </summary>
        public static TType ConvertFromString<TType>(this string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            return (TType)(TypeDescriptor.GetConverter(typeof(TType)).ConvertFromInvariantString(value));
        }

        /// <summary>
        /// Uses the type converter for <typeparamref name="TType"/> (set by <see cref="TypeConverterAttribute"/>) to generate a string.
        /// </summary>
        public static string ConvertToString<TType>(this TType value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            return TypeDescriptor.GetConverter(typeof(TType)).ConvertToInvariantString(value);
        }
    }
}
