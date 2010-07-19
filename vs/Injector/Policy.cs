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
using System.Diagnostics.CodeAnalysis;
using ZeroInstall.DownloadBroker;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Describes user settings controlling the dependency solving process.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public class Policy
    {
        #region Properties
        /// <summary>
        /// Allows configuration of the source used to request <see cref="Feed"/>s.
        /// </summary>
        public InterfaceCache InterfaceCache { get; private set; }

        /// <summary>
        /// Used to download missing <see cref="Implementation"/>s.
        /// </summary>
        public Fetcher Fetcher { get; private set; }

        /// <summary>
        /// The architecture to find executables for. Find for the current system if left at default value.
        /// </summary>
        public Architecture Architecture { get; set; }

        /// <summary>
        /// Only choose <see cref="Implementation"/>s with certain version numbers.
        /// </summary>
        public Constraint Constraint { get; set; }
        
        /// <summary>
        /// A location to search for cached <see cref="IDImplementation"/>s in addition to <see cref="DownloadBroker.Fetcher.Store"/>; may be <see langword="null"/>.
        /// </summary>
        public IStore AdditionalStore { get; set; }

        /// <summary>
        /// The locations to search for cached <see cref="IDImplementation"/>s.
        /// </summary>
        public IStore SearchStore
        {
            get
            {
                return (AdditionalStore == null
                    // No additional Store => search in same Stores the Fetcher writes to
                    ? Fetcher.Store
                    // Additional Stores => search in more Stores than the Fetcher writes to
                    : new StoreSet(new[] { AdditionalStore, Fetcher.Store }));
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new policy.
        /// </summary>
        /// <param name="interfaceCache">The source used to request <see cref="Feed"/>s.</param>
        /// <param name="fetcher">Used to download missing <see cref="Implementation"/>s.</param>
        public Policy(InterfaceCache interfaceCache, Fetcher fetcher)
        {
            #region Sanity checks
            if (interfaceCache == null) throw new ArgumentNullException("interfaceCache");
            if (fetcher == null) throw new ArgumentNullException("fetcher");
            #endregion

            InterfaceCache = interfaceCache;
            Fetcher = fetcher;
        }
        #endregion

        #region Factory methods
        /// <summary>
        /// Creates a new policy using the default <see cref="InterfaceCache"/> and <see cref="DownloadBroker.Fetcher"/>.
        /// </summary>
        /// <param name="handler">A callback object used if the the user needs to be asked any questions (such as whether he trusts a certain GPG key).</param>
        public static Policy CreateDefault(FeedHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            return new Policy(new InterfaceCache(handler), Fetcher.Default);
        }
        #endregion
    }
}
