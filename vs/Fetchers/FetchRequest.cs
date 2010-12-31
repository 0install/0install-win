/*
 * Copyright 2010 Bastian Eicher, Roland Leopold Walkling
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
using System.Diagnostics.CodeAnalysis;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Fetchers
{
    /// <summary>
    /// Lists one or more <see cref="Model.Implementation"/>s that need to be downloaded and extracted into an <see cref="IStore"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public class FetchRequest
    {
        #region Properties
        // Preserve order, duplicate entries are not allowed
        private readonly C5.ISequenced<Implementation> _implementations;
        /// <summary>
        /// The <see cref="Model.Implementation"/>s to be downloaded.
        /// </summary>
        public IEnumerable<Implementation> Implementations { get { return _implementations; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new download request.
        /// </summary>
        /// <param name="implementations">The <see cref="Model.Implementation"/>s to be downloaded.</param>
        public FetchRequest(IEnumerable<Implementation> implementations)
        {
            #region Sanity checks
            if (implementations == null) throw new ArgumentNullException("implementations");
            #endregion

            // Defensive copy
            var tempList = new C5.ArrayList<Implementation>();
            tempList.AddAll(implementations);

            // Make the collections immutable
            _implementations = new C5.GuardedList<Implementation>(tempList);
        }
        #endregion
    }
}
