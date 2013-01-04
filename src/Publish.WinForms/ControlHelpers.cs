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
    }
}
