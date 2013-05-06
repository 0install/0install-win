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
using ZeroInstall.Fetchers;
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// A light-weight, hard-coded dependency injection resolver/container. Instantiates classes and their dependencies transparently on first use.
    /// </summary>
    /// <remarks>Use the property setters to override default implementations, e.g. for mocking.</remarks>
    public class Resolver
    {
        #region Variables
        private Config _config;
        private IOpenPgp _openPgp;
        private IFeedCache _feedCache;
        private IStore _store;
        private TrustManager _trustManager;
        private CatalogManager _catalogManager;
        private ISolver _solver;
        private IFetcher _fetcher;
        private SelectionsManager _selectionsManager;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new dependency resolver
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        public Resolver(IHandler handler)
        {
            Handler = handler;
        }
        #endregion

        /// <summary>
        /// A callback object used when the the user needs to be asked questions or informed about download and IO tasks.
        /// </summary>
        public IHandler Handler { get; private set; }

        /// <summary>
        /// User settings controlling network behaviour, solving, etc.
        /// </summary>
        public Config Config { get { return Get(ref _config, Config.Load); } set { _config = value; } }

        /// <summary>
        /// Describes an object that allows the storage and retrieval of <see cref="Model.Implementation"/> directories.
        /// </summary>
        public IStore Store { get { return Get(ref _store, StoreFactory.CreateDefault); } set { _store = value; } }

        /// <summary>
        /// Provides access to an encryption/signature system compatible with the OpenPGP standard.
        /// </summary>
        public IOpenPgp OpenPgp { get { return Get(ref _openPgp, OpenPgpFactory.CreateDefault); } set { _openPgp = value; } }

        /// <summary>
        /// Provides access to a cache of <see cref="Feed"/>s that were downloaded via HTTP(S).
        /// </summary>
        public IFeedCache FeedCache { get { return Get(ref _feedCache, () => FeedCacheFactory.CreateDefault(OpenPgp)); } set { _feedCache = value; } }

        /// <summary>
        /// Methods for verifying signatures and user trust.
        /// </summary>
        public TrustManager TrustManager { get { return Get(ref _trustManager, () => new TrustManager(Config, OpenPgp, FeedCache, Handler)); } set { _trustManager = value; } }

        private IFeedManager _feedManager;

        /// <summary>
        /// Allows configuration of the source used to request <see cref="Feed"/>s.
        /// </summary>
        public IFeedManager FeedManager { get { return Get(ref _feedManager, () => new FeedManager(Config, FeedCache, TrustManager, Handler)); } set { _feedManager = value; } }

        /// <summary>
        /// Provides access to remote and local <see cref="Catalog"/>s. Handles downloading, signature verification and caching.
        /// </summary>
        public CatalogManager CatalogManager { get { return Get(ref _catalogManager, () => new CatalogManager(TrustManager)); } set { _catalogManager = value; } }

        /// <summary>
        /// Chooses a set of <see cref="Model.Implementation"/>s to satisfy the requirements of a program and its user. 
        /// </summary>
        public ISolver Solver { get { return Get(ref _solver, () => new ExternalSolver(Config, FeedCache, FeedManager, Handler)); } set { _solver = value; } }

        /// <summary>
        /// Used to download missing <see cref="Model.Implementation"/>s.
        /// </summary>
        public IFetcher Fetcher { get { return Get(ref _fetcher, () => new SequentialFetcher(Store, Handler)); } set { _fetcher = value; } }

        /// <summary>
        /// Contains helper methods for filtering <see cref="Selections"/>.
        /// </summary>
        public SelectionsManager SelectionsManager { get { return Get(ref _selectionsManager, () => _selectionsManager = new SelectionsManager(FeedCache, Store)); } set { _selectionsManager = value; } }

        private static T Get<T>(ref T value, Func<T> build) where T : class
        {
            return value ?? (value = build());
        }
    }
}
