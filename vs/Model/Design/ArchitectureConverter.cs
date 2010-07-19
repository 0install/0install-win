/*
 * Copyright 2010 Bastian Eicher
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
        /// <summary>The number of arguments <see cref="Architecture"/> has.</summary>
        protected override int NoArguments { get { return 2; } }

        /// <returns>The constructor used to create new instances of <see cref="Architecture"/> (deserialization).</returns>
        protected override ConstructorInfo GetConstuctor()
        {
            return typeof(Architecture).GetConstructor(new[] { typeof(OS), typeof(Cpu) });
        }

        /// <returns>The unconverted arguments of <see cref="Architecture"/>.</returns>
        protected override object[] GetArguments(Architecture value)
        {
            return new object[] { value.OS, value.Cpu };
        }

        /// <returns>The arguments of <see cref="Architecture"/> converted to string.</returns>
        protected override string[] GetValues(Architecture value, ITypeDescriptorContext context, CultureInfo culture)
        {
            return new[] { value.OS.ToString(), value.Cpu.ToString() };
        }

        /// <returns>A new instance of <see cref="Architecture"/>.</returns>
        protected override Architecture GetObject(string[] values, CultureInfo culture)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException("values");
            #endregion

            return new Architecture((OS)Enum.Parse(typeof(OS), values[0]), (Cpu)Enum.Parse(typeof(Cpu), values[1]));
        }

        /// <returns>A new instance of <see cref="Architecture"/>.</returns>
        protected override Architecture GetObject(IDictionary propertyValues)
        {
            #region Sanity checks
            if (propertyValues == null) throw new ArgumentNullException("propertyValues");
            #endregion

            return new Architecture(
                (OS)Enum.Parse(typeof(OS), propertyValues["OS"].ToString()),
                (Cpu)Enum.Parse(typeof(Cpu), propertyValues["Cpu"].ToString()));
        }
    }
}