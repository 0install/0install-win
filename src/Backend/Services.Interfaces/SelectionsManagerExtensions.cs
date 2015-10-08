/*
 * Copyright 2010-2015 Bastian Eicher
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
using JetBrains.Annotations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services
{
    /// <summary>
    /// Provides extension methods for <see cref="ISelectionsManager"/>.
    /// </summary>
    public static class SelectionsManagerExtensions
    {
        /// <summary>
        /// Combines <see cref="ISelectionsManager.GetUncachedSelections"/> and <see cref="ISelectionsManager.GetImplementations"/>.
        /// </summary>
        /// <param name="selectionsManager">The <see cref="ISelectionsManager"/></param>
        /// <param name="selections">The selections to search for <see cref="ImplementationSelection"/>s that are missing.</param>
        public static ICollection<Implementation> GetUncachedImplementations([NotNull] this ISelectionsManager selectionsManager, [NotNull] Selections selections)
        {
            #region Sanity checks
            if (selectionsManager == null) throw new ArgumentNullException("selectionsManager");
            if (selections == null) throw new ArgumentNullException("selections");
            #endregion

            return selectionsManager.GetImplementations(selectionsManager.GetUncachedSelections(selections)).ToList();
        }
    }
}