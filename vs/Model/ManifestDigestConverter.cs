using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Common.Values;

namespace ZeroInstall.Model
{
    internal class ManifestDigestConverter : ValueTypeConverter<ManifestDigest>
    {
        /// <summary>The number of arguments <see cref="ManifestDigest"/> has.</summary>
        protected override int NoArguments { get { return 3; } }

        /// <returns>The constructor used to create new instances of <see cref="ManifestDigest"/> (deserialization).</returns>
        protected override ConstructorInfo GetConstuctor()
        {
            return typeof(ManifestDigest).GetConstructor(new[] { typeof(string), typeof(string), typeof(string) });
        }

        /// <returns>The unconverted arguments of <see cref="ManifestDigest"/>.</returns>
        protected override object[] GetArguments(ManifestDigest value)
        {
            return new object[] { value.Sha1, value.Sha1New, value.Sha256 };
        }

        /// <returns>The arguments of <see cref="ManifestDigest"/> converted to string.</returns>
        protected override string[] GetValues(ManifestDigest value, ITypeDescriptorContext context, CultureInfo culture)
        {
            return new[] { value.Sha1, value.Sha1New, value.Sha256 };
        }

        /// <returns>A new instance of <see cref="ManifestDigest"/>.</returns>
        protected override ManifestDigest GetObject(string[] values, CultureInfo culture)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException("values");
            #endregion

            return new ManifestDigest(values[0], values[1], values[2]);
        }

        /// <returns>A new instance of <see cref="ManifestDigest"/>.</returns>
        protected override ManifestDigest GetObject(IDictionary propertyValues)
        {
            #region Sanity checks
            if (propertyValues == null) throw new ArgumentNullException("propertyValues");
            #endregion

            return new ManifestDigest(propertyValues["Sha1"].ToString(), propertyValues["Sha1New"].ToString(), propertyValues["Sha256"].ToString());
        }
    }
}