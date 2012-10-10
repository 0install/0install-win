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
    internal class ArchitectureConverter : ValueTypeConverter<Architecture>
    {
        /// <inheritdoc/>
        protected override int NoArguments { get { return 2; } }

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
            return new[] {value.OS.ToString(), value.Cpu.ToString()};
        }

        /// <inheritdoc/>
        protected override Architecture GetObject(string[] values, CultureInfo culture)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException("values");
            #endregion

            return new Architecture(
                (OS)Enum.Parse(typeof(OS), values[0], true),
                (Cpu)Enum.Parse(typeof(Cpu), values[1], true));
        }

        /// <inheritdoc/>
        protected override Architecture GetObject(IDictionary propertyValues)
        {
            #region Sanity checks
            if (propertyValues == null) throw new ArgumentNullException("propertyValues");
            #endregion

            return new Architecture(
                (OS)Enum.Parse(typeof(OS), propertyValues["OS"].ToString(), true),
                (Cpu)Enum.Parse(typeof(Cpu), propertyValues["Cpu"].ToString(), true));
        }
    }
}
