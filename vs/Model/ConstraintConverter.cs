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
            return new[] { value.NotBeforeVersion, value.BeforeVersion };
        }

        /// <returns>A new instance of <see cref="Constraint"/>.</returns>
        protected override Constraint GetObject(string[] values, CultureInfo culture)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException("values");
            #endregion

            return new Constraint(values[0], values[1]);
        }

        /// <returns>A new instance of <see cref="Constraint"/>.</returns>
        protected override Constraint GetObject(IDictionary propertyValues)
        {
            #region Sanity checks
            if (propertyValues == null) throw new ArgumentNullException("propertyValues");
            #endregion

            return new Constraint(propertyValues["NotBeforeVersion"].ToString(), propertyValues["BeforeVersion"].ToString());
        }
    }
}