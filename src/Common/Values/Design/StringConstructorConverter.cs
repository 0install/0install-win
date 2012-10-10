/*
 * Copyright 2010-2012 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.ComponentModel;
using System.Globalization;

namespace Common.Values.Design
{
    /// <summary>
    /// Generic type converter for classes that have a constructor with a single string argument and a coressponding <see cref="object.ToString"/> implementation.
    /// </summary>
    /// <typeparam name="T">The type the converter is used for.</typeparam>
    /// <example>
    ///   Add this attribute to the type:
    ///   <code>[TypeConverter(typeof(StringConstructorConverter&lt;NameOfType&gt;))]</code>
    /// </example>
    public class StringConstructorConverter<T> : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var stringValue = value as string;
            if (stringValue != null)
            {
                var constructor = typeof(T).GetConstructor(new[] {typeof(string)});
                if (constructor != null)
                {
                    try
                    {
                        return constructor.Invoke(new object[] {stringValue});
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null) throw ex.InnerException;
                        else throw;
                    }
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc/>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string)) return value.ToString();
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
