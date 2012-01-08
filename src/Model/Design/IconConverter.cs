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
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Common.Values.Design;

namespace ZeroInstall.Model.Design
{
    internal class IconConverter : ValueTypeConverter<Icon>
    {
        /// <summary>The number of arguments <see cref="Icon"/> has.</summary>
        protected override int NoArguments { get { return 2; } }

        /// <returns>The constructor used to create new instances of <see cref="Icon"/> (deserialization).</returns>
        protected override ConstructorInfo GetConstructor()
        {
            return typeof(Icon).GetConstructor(new[] {typeof(Uri), typeof(string)});
        }

        /// <returns>The unconverted arguments of <see cref="Icon"/>.</returns>
        protected override object[] GetArguments(Icon value)
        {
            return new object[] {value.LocationString, value.MimeType};
        }

        /// <returns>The arguments of <see cref="Icon"/> converted to string.</returns>
        protected override string[] GetValues(Icon value, ITypeDescriptorContext context, CultureInfo culture)
        {
            return new[] {value.LocationString, value.MimeType};
        }

        /// <returns>A new instance of <see cref="Icon"/>.</returns>
        protected override Icon GetObject(string[] values, CultureInfo culture)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException("values");
            #endregion

            return new Icon(new Uri(values[0]), values[1]);
        }

        /// <returns>A new instance of <see cref="Icon"/>.</returns>
        protected override Icon GetObject(IDictionary propertyValues)
        {
            #region Sanity checks
            if (propertyValues == null) throw new ArgumentNullException("propertyValues");
            #endregion

            return new Icon(new Uri(propertyValues["Location"].ToString()), propertyValues["MimeType"].ToString());
        }
    }
}
