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
using System.Xml.Serialization;
using Common.Utils;

namespace Common.Values.Design
{
    /// <summary>
    /// Type converter for <see cref="Enum"/> <see cref="Enum"/>s annotated with <see cref="XmlEnumAttribute"/>s.
    /// </summary>
    /// <typeparam name="T">The type the converter is used for.</typeparam>
    /// <example>
    ///   Add this attribute to the <see cref="Enum"/>:
    ///   <code>[TypeConverter(typeof(XmlEnumConverter&lt;NameOfEnum&gt;))]</code>
    /// </example>
    /// <remarks><see cref="XmlEnumAttribute.Name"/> is used as the case-insensitive string representation (falls back to element name).</remarks>
    public class XmlEnumConverter<T> : TypeConverter where T : struct
    {
        private static string GetString(Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attributes = (XmlEnumAttribute[])fieldInfo.GetCustomAttributes(typeof(XmlEnumAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Name : value.ToString();
        }

        private static object GetEnum(Type type, string stringValue)
        {
            if (stringValue == "") throw new ArgumentNullException();
            foreach (var field in type.GetFields())
            {
                var attributes = (XmlEnumAttribute[])field.GetCustomAttributes(typeof(XmlEnumAttribute), false);
                if (attributes.Length > 0 && StringUtils.Compare(attributes[0].Name, stringValue))
                    return field.GetValue(field.Name);
            }
            return Enum.Parse(type, stringValue, true);
        }

        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var stringValue = value as string;
            if (stringValue != null) return GetEnum(typeof(T), stringValue);
            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc/>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var enumValue = value as Enum;
            if (enumValue != null && destinationType == typeof(string)) return GetString(enumValue);
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
