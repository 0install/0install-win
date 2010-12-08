/*
 * Copyright 2006-2010 Bastian Eicher
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
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace Common.Values.Design
{
    /// <summary>
    /// Abstract base-class for easily creating a <see cref="TypeConverter"/> for a value type (struct).
    /// </summary>
    /// <typeparam name="T">The struct to create the <see cref="TypeConverter"/> for.</typeparam>
    /// <remarks>Providing a <see cref="TypeConverter"/> for a struct vastly improves the runtime experience with <see cref="System.Windows.Forms.PropertyGrid"/>s.</remarks>
    /// <example>
    ///   Add this attribute to the struct:
    ///   <code>[TypeConverter(typeof(ClassDerivedFromThisOne))]</code>
    /// </example>
    public abstract class ValueTypeConverter<T> : TypeConverter where T : struct
    {
        #region Capabilities
        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        { return true; }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        { return true; }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(value, attributes);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(InstanceDescriptor)) || base.CanConvertFrom(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType);
        }
        #endregion

        #region Convert to
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
                return new InstanceDescriptor(GetConstuctor(), GetArguments((T)value));

            if (destinationType == typeof(string))
                return string.Join(culture.TextInfo.ListSeparator + " ", GetValues((T)value, context, culture));

            return base.ConvertTo(context, culture, value, destinationType);
        }
        #endregion

        #region Convert from
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var sValue = value as string;
            if (sValue == null) return base.ConvertFrom(context, culture, value);

            sValue = sValue.Trim();
            if (sValue.Length == 0) return null;

            var arguments = sValue.Split(culture.TextInfo.ListSeparator[0]);
            if (arguments.Length != NoArguments) return null;
            return GetObject(arguments, culture);
        }
        #endregion

        #region Create instance
        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            return GetObject(propertyValues);
        }
        #endregion

        //--------------------//

        #region Hooks
        /// <summary>The number of arguments <typeparamref name="T"/> has.</summary>
        protected abstract int NoArguments { get; }

        /// <returns>The constructor used to create new instances of <typeparamref name="T"/> (deserialization).</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected abstract ConstructorInfo GetConstuctor();

        /// <returns>The unconverted arguments of <typeparamref name="T"/>.</returns>
        protected abstract object[] GetArguments(T value);

        /// <returns>The arguments of <typeparamref name="T"/> converted to strings.</returns>
        protected abstract string[] GetValues(T value, ITypeDescriptorContext context, CultureInfo culture);

        /// <returns>A new instance of <typeparamref name="T"/>.</returns>
        protected abstract T GetObject(string[] values, CultureInfo culture);

        /// <returns>A new instance of <typeparamref name="T"/>.</returns>
        protected abstract T GetObject(IDictionary propertyValues);
        #endregion
    }
}
