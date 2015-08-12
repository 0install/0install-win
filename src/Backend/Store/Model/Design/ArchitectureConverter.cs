/*
 * Copyright 2010-2016 Bastian Eicher
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
using NanoByte.Common.Values;
using NanoByte.Common.Values.Design;

namespace ZeroInstall.Store.Model.Design
{
    internal class ArchitectureConverter : ValueTypeConverter<Architecture>
    {
        /// <inheritdoc/>
        protected override string GetElementSeparator(CultureInfo culture)
        {
            return "-";
        }

        /// <inheritdoc/>
        protected override int NoArguments => 2;

        /// <inheritdoc/>
        protected override ConstructorInfo GetConstructor()
        {
            return typeof(Architecture).GetConstructor(new[] {typeof(OS), typeof(Cpu)});
        }

        /// <inheritdoc/>
        protected override object[] GetArguments(Architecture value)
        {
            return new object[] {value.OS, value.Cpu};
        }

        /// <inheritdoc/>
        protected override string[] GetValues(Architecture value, ITypeDescriptorContext context, CultureInfo culture)
        {
            return new[] {value.OS.ConvertToString(), value.Cpu.ConvertToString()};
        }

        /// <inheritdoc/>
        protected override Architecture GetObject(string[] values, CultureInfo culture)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException(nameof(values));
            #endregion

            return new Architecture(
                values[0].ConvertFromString<OS>(),
                values[1].ConvertFromString<Cpu>());
        }

        /// <inheritdoc/>
        protected override Architecture GetObject(IDictionary propertyValues)
        {
            #region Sanity checks
            if (propertyValues == null) throw new ArgumentNullException(nameof(propertyValues));
            #endregion

            return new Architecture(
                propertyValues["OS"].ToString().ConvertFromString<OS>(),
                propertyValues["Cpu"].ToString().ConvertFromString<Cpu>());
        }
    }
}
