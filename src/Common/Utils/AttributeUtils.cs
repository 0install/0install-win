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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace NanoByte.Common.Utils
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
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static IEnumerable<TAttribute> GetAttributes<TAttribute, TTarget>() where TAttribute : Attribute
        {
            var attributes = typeof(TTarget).GetCustomAttributes(typeof(TAttribute), inherit: true);
            return attributes.OfType<TAttribute>();
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
            var attributes = (TAttribute[])fieldInfo.GetCustomAttributes(typeof(TAttribute), inherit: true);
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
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (value == null) throw new ArgumentNullException("value");
            // ReSharper restore CompareNonConstrainedGenericWithNull
            #endregion

            return TypeDescriptor.GetConverter(typeof(TType)).ConvertToInvariantString(value);
        }

        /// <summary>
        /// Retrieves a single value from a Custom <see cref="Attribute"/> associated with an <see cref="Assembly"/>.
        /// </summary>
        /// <typeparam name="TAttribute">The type of Custom <see cref="Attribute"/> associated with the <paramref name="assembly"/> to retrieve.</typeparam>
        /// <typeparam name="TValue">The type of the value to retrieve from the <typeparamref name="TAttribute"/>.</typeparam>
        /// <param name="assembly">The <see cref="Assembly"/> to retrieve the <typeparamref name="TAttribute"/> from.</param>
        /// <param name="valueRetrieval">A callback used to retrieve a <typeparamref name="TValue"/> from a <typeparamref name="TAttribute"/>.</param>
        /// <returns>The retrieved value or <see langword="null"/> if no <typeparamref name="TAttribute"/> was found.</returns>
        public static TValue GetAttributeValue<TAttribute, TValue>(this Assembly assembly, Func<TAttribute, TValue> valueRetrieval)
            where TAttribute : Attribute
        {
            #region Sanity checks
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (valueRetrieval == null) throw new ArgumentNullException("valueRetrieval");
            #endregion

            var attributes = assembly.GetCustomAttributes(typeof(TAttribute), inherit: false);
            return (attributes.Length > 0) ? valueRetrieval((TAttribute)attributes[0]) : default(TValue);
        }
    }
}
