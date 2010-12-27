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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using ZeroInstall.Fetchers;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Launcher
{
    /// <summary>
    /// Describes user preferences and restrictions controlling the dependency solving and implementation launching process.
    /// </summary>
    /// <remarks>The data for this object is accumulated from the system state, preference files, command-line arguments and GUI choices.</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    [Serializable]
    public class Policy
    {
        #region Properties
        /// <summary>
        /// Allows configuration of the source used to request <see cref="Feed"/>s.
        /// </summary>
        public FeedManager FeedManager { get; private set; }

        /// <summary>
        /// Used to download missing <see cref="Implementation"/>s.
        /// </summary>
        public Fetcher Fetcher { get; private set; }

        /// <summary>
        /// The architecture to find executables for. Find for the current system if left at default value.
        /// </summary>
        public Architecture Architecture { get; set; }

        private readonly List<CultureInfo> _languages = new List<CultureInfo>();
        /// <summary>
        /// The preferred languages for implementations in decreasing order. Use system locale if empty.
        /// </summary>
        public ICollection<CultureInfo> Languages { get { return _languages; } }

        private readonly Constraint _constraint = new Constraint();
        /// <summary>
        /// Only choose <see cref="Implementation"/>s with certain version numbers.
        /// </summary>
        public Constraint Constraint { get { return _constraint; } }

        /// <summary>
        /// A location to search for cached <see cref="ImplementationBase"/>s in addition to <see cref="Fetchers.Fetcher.Store"/>; may be <see langword="null"/>.
        /// </summary>
        public IStore AdditionalStore { get; set; }

        /// <summary>
        /// The locations to search for cached <see cref="ImplementationBase"/>s.
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
        /// <param name="feedManager">The source used to request <see cref="Feed"/>s.</param>
        /// <param name="fetcher">Used to download missing <see cref="Implementation"/>s.</param>
        public Policy(FeedManager feedManager, Fetcher fetcher)
        {
            #region Sanity checks
            if (feedManager == null) throw new ArgumentNullException("feedManager");
            if (fetcher == null) throw new ArgumentNullException("fetcher");
            #endregion

            FeedManager = feedManager;
            Fetcher = fetcher;
        }
        #endregion

        #region Factory methods
        /// <summary>
        /// Creates a new policy using the default <see cref="FeedManager"/> and <see cref="Fetchers.Fetcher"/>.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked any questions or informed about progress.</param>
        /// <exception cref="InvalidOperationException">Thrown if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public static Policy CreateDefault(IHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            return new Policy(new FeedManager(FeedCacheProvider.Default, handler), new Fetcher(handler));
        }
        #endregion
    }
}
