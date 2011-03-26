/*
 * Copyright 2010-2011 Bastian Eicher, Roland Leopold Walkling
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
using Common;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Fetchers
{
    /// <summary>
    /// Downloads <see cref="Implementation"/>s, extracts them and adds them to the <see cref="Store"/>.
    /// </summary>
    public class Fetcher : MarshalByRefObject, IFetcher
    {
        #region Variables
        /// <summary>
        /// Associates the current <see cref="ImplementationFetch"/> to each running <see cref="FetchRequest"/>.
        /// A key is added as soon as <see cref="Start"/> is called (initial value <see langword="null"/>) and remains until <see cref="Join"/> is called.
        /// </summary>
        private readonly Dictionary<FetchRequest, ImplementationFetch> _currentFetchProcess = new Dictionary<FetchRequest, ImplementationFetch>();
        #endregion

        #region Properties
        /// <inheritdoc/>
        public IStore Store { get; private set; }
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

        #region Start
        /// <inheritdoc/>
        public void Start(FetchRequest fetchRequest)
        {
            #region Sanity checks
            if (fetchRequest == null) throw new ArgumentNullException("fetchRequest");
            #endregion

            // ToDo: Make asynchronous
            _currentFetchProcess.Add(fetchRequest, null);
        }
        #endregion

        #region Join
        /// <inheritdoc/>
        public void Join(FetchRequest fetchRequest)
        {
            #region Sanity checks
            if (fetchRequest == null) throw new ArgumentNullException("fetchRequest");
            #endregion

            // ToDo: Make asynchronous
            foreach (var implementation in fetchRequest.Implementations)
            {
                // Check if the process has been canceled
                if (!_currentFetchProcess.ContainsKey(fetchRequest)) throw new UserCancelException();
                
                var fetchProcess = CreateFetch(implementation);
                _currentFetchProcess[fetchRequest] = fetchProcess; // Store current step for cancellation
                fetchProcess.Execute(fetchRequest.Handler);
                if (!fetchProcess.Completed) throw fetchProcess.Problems.Last;
            }
            _currentFetchProcess.Remove(fetchRequest);
        }
        #endregion

        #region Cancel
        /// <inheritdoc/>
        public void Cancel(FetchRequest fetchRequest)
        {
            #region Sanity checks
            if (fetchRequest == null) throw new ArgumentNullException("fetchRequest");
            #endregion

            // Check if the request is running
            ImplementationFetch fetchProcess;
            if (!_currentFetchProcess.TryGetValue(fetchRequest, out fetchProcess)) return;

            // Cancel the current task
            if (fetchProcess != null) fetchProcess.Cancel();

            // Prevent any further tasks form being started
            _currentFetchProcess.Remove(fetchRequest);
        }
        #endregion
    }
}
