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
using Common.Values;

namespace ZeroInstall.Model
{
    internal class ConstraintConverter : ValueTypeConverter<Constraint>
    {
        /// <summary>The number of arguments <see cref="Constraint"/> has.</summary>
        protected override int NoArguments { get { return 2; } }

        /// <returns>The constructor used to create new instances of <see cref="Constraint"/> (deserialization).</returns>
        protected override ConstructorInfo GetConstuctor()
        {
            return typeof(Constraint).GetConstructor(new[] { typeof(string), typeof(string) });
        }

        /// <returns>The unconverted arguments of <see cref="Constraint"/>.</returns>
        protected override object[] GetArguments(Constraint value)
        {
            return new object[] { value.NotBeforeVersion, value.BeforeVersion };
        }

        /// <returns>The arguments of <see cref="Constraint"/> converted to strings.</returns>
        protected override string[] GetValues(Constraint value, ITypeDescriptorContext context, CultureInfo culture)
        {
            return new[] { value.NotBeforeVersion.ToString(), value.BeforeVersion.ToString() };
        }

        /// <returns>A new instance of <see cref="Constraint"/>.</returns>
        protected override Constraint GetObject(string[] values, CultureInfo culture)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException("values");
            #endregion

            return new Constraint(new ImplementationVersion(values[0]), new ImplementationVersion(values[1]));
        }

        /// <returns>A new instance of <see cref="Constraint"/>.</returns>
        protected override Constraint GetObject(IDictionary propertyValues)
        {
            #region Sanity checks
            if (propertyValues == null) throw new ArgumentNullException("propertyValues");
            #endregion

            return new Constraint(
                new ImplementationVersion(propertyValues["NotBeforeVersion"].ToString()),
                new ImplementationVersion(propertyValues["BeforeVersion"].ToString()));
        }
    }
}