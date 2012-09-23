/*
 * Copyright 2010 Simon E. Silva Lauinger
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
using System.ComponentModel;
using System.Reflection;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms
{
    /// <summary>
    /// Contains methods for all feed editor's controls.
    /// </summary>
    public static class ControlHelpers
    {
        /// <summary>
        /// Checks if the properties of a <see cref="Archive"/> have their default values.
        /// </summary>
        /// <param name="toCheck"><see cref="Archive"/> to check.</param>
        /// <returns>true, if Archive has default values.</returns>
        public static bool IsEmpty(Archive toCheck)
        {
            if (toCheck == null) throw new ArgumentNullException("toCheck");

            return (toCheck.Extract == default(String) && toCheck.Location == default(Uri) && toCheck.MimeType == default(String) && toCheck.Size == default(long) && toCheck.StartOffset == default(long));
        }

        /// <summary>
        /// Get the <see cref="DescriptionAttribute.Description"/> text of a <see langword="Enum"/> value.
        /// </summary>
        /// <param name="toGetDescriptionFrom">Description of this <see langword="Enum"/></param>
        /// <returns>A Description of a <see langword="Enum"/>.</returns>
        public static string GetEnumDescription(Enum toGetDescriptionFrom)
        {
            #region Sanity checks
            if (toGetDescriptionFrom == null) throw new ArgumentNullException("toGetDescriptionFrom");
            #endregion

            // ReSharper disable SpecifyACultureInStringConversionExplicitly
            FieldInfo fi = toGetDescriptionFrom.GetType().GetField(toGetDescriptionFrom.ToString());
            // ReSharper restore SpecifyACultureInStringConversionExplicitly

            var attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute),
                    false);

            // ReSharper disable SpecifyACultureInStringConversionExplicitly
            return attributes.Length > 0 ? attributes[0].Description : toGetDescriptionFrom.ToString();
            // ReSharper restore SpecifyACultureInStringConversionExplicitly
        }
    }
}
