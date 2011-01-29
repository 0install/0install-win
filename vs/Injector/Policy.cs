/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.IO;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Combines configuration and resources used to solve dependencies and download implementations.
    /// </summary>
    /// <remarks>This object primarily serves as a working environment used by <see cref="ISolver"/>s.</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    [Serializable]
    public class Policy
    {
        #region Properties
        /// <summary>
        /// User-preferences controlling network behaviour, etc.
        /// </summary>
        public Preferences Preferences { get; private set; }

        /// <summary>
        /// Allows configuration of the source used to request <see cref="Feed"/>s.
        /// </summary>
        public FeedManager FeedManager { get; private set; }

        /// <summary>
        /// Used to download missing <see cref="Model.Implementation"/>s.
        /// </summary>
        public IFetcher Fetcher { get; private set; }

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
        /// <param name="preferences">User-preferences controlling network behaviour, etc.</param>
        /// <param name="feedManager">The source used to request <see cref="Feed"/>s.</param>
        /// <param name="fetcher">Used to download missing <see cref="Model.Implementation"/>s.</param>
        public Policy(Preferences preferences, FeedManager feedManager, IFetcher fetcher)
        {
            #region Sanity checks
            if (preferences == null) throw new ArgumentNullException("preferences");
            if (feedManager == null) throw new ArgumentNullException("feedManager");
            if (fetcher == null) throw new ArgumentNullException("fetcher");
            #endregion

            Preferences = preferences;
            FeedManager = feedManager;
            Fetcher = fetcher;
        }
        #endregion

        #region Factory method
        /// <summary>
        /// Creates a new policy using the default <see cref="Preferences"/>, <see cref="FeedCacheProvider"/> and <see cref="FetcherProvider"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public static Policy CreateDefault()
        {
            return new Policy(Preferences.LoadDefault(), new FeedManager(FeedCacheProvider.Default), FetcherProvider.Default);
        }
        #endregion
    }
}
