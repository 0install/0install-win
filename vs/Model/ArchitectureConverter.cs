using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Common.Values;

namespace ZeroInstall.Model
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