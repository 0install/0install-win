/*
 * Copyright 2010 Bastian Eicher
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
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.DownloadBroker
{
    /// <summary>
    /// Manages one or more <see cref="FetcherRequest"/>s and keeps clients informed of the progress.
    /// </summary>
    public class Fetcher
    {
        #region Variables
        private static readonly Fetcher _defaultFetcher = new Fetcher(StoreFactory.DefaultStore());
        #endregion

        #region Properties
        /// <summary>
        /// The location to store the downloaded and unpacked <see cref="Implementation"/>s in.
        /// </summary>
        public IStore Store { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new download request. Use <see cref="DefaultFetcher"/> whenever possible instead.
        /// </summary>
        /// <param name="store">The location to store the downloaded and unpacked <see cref="Implementation"/>s in.</param>
        public Fetcher(IStore store)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            Store = store;
        }
        #endregion

        #region Static access
        /// <summary>
        /// Returns a singleton-instance of the fetcher using <see cref="StoreFactory.DefaultStore"/>.
        /// </summary>
        public static Fetcher DefaultFetcher()
        {
            return _defaultFetcher;
        }
        #endregion
        
        //--------------------//

        /// <summary>
        /// Execute a complete request and block until it is done.
        /// </summary>
        public void RunSync(FetcherRequest fetcherRequest)
        {
            // ToDo: Implement
            throw new NotImplementedException();
        }
    }
}
