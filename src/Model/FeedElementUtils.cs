/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Info;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Helper methods for handling <see cref="FeedElement"/>s.
    /// </summary>
    public static class FeedElementUtils
    {
        private static readonly ImplementationVersion _zeroInstallVersion = new ImplementationVersion(AppInfo.Load(Assembly.GetCallingAssembly()).Version);

        public static bool FilterMismatch<T>(this T element)
            where T : FeedElement
        {
            #region Sanity checks
            if (element == null) throw new ArgumentNullException("element");
            #endregion

            return element.IfZeroInstallVersion != null && !element.IfZeroInstallVersion.Match(_zeroInstallVersion);
        }

        /// <summary>
        /// Filters those elements that do not pass an <see cref="FeedElement.IfZeroInstallVersion"/> test.
        /// </summary>
        public static void RemoveFiltered<T>(this ICollection<T> elements)
            where T : FeedElement
        {
            #region Sanity checks
            if (elements == null) throw new ArgumentNullException("elements");
            #endregion

            var toRemove = elements.Where(FilterMismatch);
            foreach (var element in toRemove.ToList()) elements.Remove(element);
        }

        /// <summary>
        /// Filters those elements that do not pass an <see cref="FeedElement.IfZeroInstallVersion"/> test.
        /// </summary>
        public static void RemoveFiltered(this ICollection<IRecipeStep> elements)
        {
            #region Sanity checks
            if (elements == null) throw new ArgumentNullException("elements");
            #endregion

            var toRemove = elements.OfType<FeedElement>().Where(FilterMismatch);
            foreach (var element in toRemove.ToList()) elements.Remove((IRecipeStep)element);
        }
    }
}
