/*
 * Copyright 2010-2012 Bastian Eicher, Roland Leopold Walkling
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
using Common.Tasks;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Fetchers
{
    /// <summary>
    /// Downloads <see cref="Implementation"/>s, extracts them and adds them to the <see cref="Store"/>.
    /// </summary>
    public class Fetcher : MarshalByRefObject, IFetcher
    {
        #region Properties
        private IStore _store;

        /// <inheritdoc/>
        public IStore Store
        {
            get { return _store; }
            set
            {
                #region Sanity checks
                if (value == null) throw new ArgumentNullException("value");
                #endregion

                _store = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new download fetcher.
        /// </summary>
        /// <param name="store">The location to store the downloaded and unpacked <see cref="Model.Implementation"/>s in.</param>
        public Fetcher(IStore store)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            Store = store;
        }
        #endregion

        //--------------------//

        protected virtual ImplementationFetch CreateFetch(Implementation implementation)
        {
            return new ImplementationFetch(this, implementation);
        }

        /// <inheritdoc/>
        public void FetchImplementations(IEnumerable<Implementation> implementations, ITaskHandler handler)
        {
            #region Sanity checks
            if (implementations == null) throw new ArgumentNullException("implementations");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            foreach (var fetchProcess in implementations.Select(CreateFetch))
            {
                fetchProcess.Execute(handler);
                if (!fetchProcess.Completed) throw fetchProcess.Problems.Last;
            }
        }
    }
}
