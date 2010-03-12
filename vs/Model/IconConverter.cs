using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Common.Values;

namespace ZeroInstall.Model
{
    internal class IconConverter : ValueTypeConverter<Icon>
    {
        /// <summary>The number of arguments <see cref="Icon"/> has.</summary>
        protected override int NoArguments { get { return 2; } }

        /// <returns>The constructor used to create new instances of <see cref="Icon"/> (deserialization).</returns>
        protected override ConstructorInfo GetConstuctor()
        {
            return typeof(Icon).GetConstructor(new[] { typeof(Uri), typeof(string) });
        }

        /// <returns>The unconverted arguments of <see cref="Icon"/>.</returns>
        protected override object[] GetArguments(Icon value)
        {
            return new object[] { value.LocationString, value.MimeType };
        }

        /// <returns>The arguments of <see cref="Icon"/> converted to string.</returns>
        protected override string[] GetValues(Icon value, ITypeDescriptorContext context, CultureInfo culture)
        {
            return new[] { value.LocationString, value.MimeType };
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