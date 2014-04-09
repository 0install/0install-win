/*
 * Copyright 2010-2014 Bastian Eicher
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
using NanoByte.Common.Values.Design;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.Store.Model.Design
{
    internal class InstallCommandsConverter : ValueTypeConverter<InstallCommands>
    {
        /// <summary>The number of arguments <see cref="InstallCommands"/> has.</summary>
        protected override int NoArguments { get { return 6; } }

        /// <returns>The constructor used to create new instances of <see cref="InstallCommands"/> (deserialization).</returns>
        protected override ConstructorInfo GetConstructor()
        {
            return typeof(InstallCommands).GetConstructor(new[] {typeof(string), typeof(string), typeof(string), typeof(string)});
        }

        /// <returns>The unconverted arguments of <see cref="InstallCommands"/>.</returns>
        protected override object[] GetArguments(InstallCommands value)
        {
            return new object[] {value.Reinstall, value.ReinstallArgs, value.ShowIcons, value.ShowIconsArgs, value.HideIcons, value.HideIconsArgs};
        }

        /// <returns>The arguments of <see cref="InstallCommands"/> converted to string.</returns>
        protected override string[] GetValues(InstallCommands value, ITypeDescriptorContext context, CultureInfo culture)
        {
            return new[] {value.Reinstall, value.ReinstallArgs, value.ShowIcons, value.ShowIconsArgs, value.HideIcons, value.HideIconsArgs};
        }

        /// <returns>A new instance of <see cref="InstallCommands"/>.</returns>
        protected override InstallCommands GetObject(string[] values, CultureInfo culture)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException("values");
            #endregion

            return new InstallCommands
            {
                Reinstall = values[0], ReinstallArgs = values[1],
                ShowIcons = values[2], ShowIconsArgs = values[3],
                HideIcons = values[4], HideIconsArgs = values[5]
            };
        }

        /// <returns>A new instance of <see cref="InstallCommands"/>.</returns>
        protected override InstallCommands GetObject(IDictionary propertyValues)
        {
            #region Sanity checks
            if (propertyValues == null) throw new ArgumentNullException("propertyValues");
            #endregion

            return new InstallCommands
            {
                Reinstall = (string)propertyValues["Reinstall"], ReinstallArgs = (string)propertyValues["ReinstallArgs"],
                ShowIcons = (string)propertyValues["ShowIcons"], ShowIconsArgs = (string)propertyValues["ShowIconsArgs"],
                HideIcons = (string)propertyValues["HideIcons"], HideIconsArgs = (string)propertyValues["HideIconsArgs"]
            };
        }
    }
}
