/*
 * Copyright 2010-2014 Bastian Eicher
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
using NanoByte.Common.Tasks;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Fetchers;
using ZeroInstall.Services.Injector;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Services
{
    /// <summary>
    /// Instantiates requested services transparently on first use. Handles dependency injection internally.
    /// Use exactly one instance of the service locator per user request to ensure consistent state during execution.
    /// </summary>
    /// <remarks>Use the property setters to override default service implementations, e.g. for mocking.</remarks>
    public class ServiceLocator
    {
        #region Variables
        private Config _config;
        private IOpenPgp _openPgp;
        private IFeedCache _feedCache;
        private IStore _store;
        private ITrustManager _trustManager;
        private IFeedManager _feedManager;
        private ICatalogManager _catalogManager;
        private IPackageManager _packageManager;
        private ISolver _solver;
        private IFetcher _fetcher;
        private IExecutor _executor;
        private SelectionsManager _selectionsManager;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new service locator.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        public ServiceLocator(ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            Handler = handler;
        }
        #endregion

        /// <summary>
        /// A callback object used when the the user needs to be asked questions or informed about download and IO tasks.
        /// </summary>
        public ITaskHandler Handler { get; private set; }

        /// <summary>
        /// User settings controlling network behaviour, solving, etc.
        /// </summary>
        public Config Config { get { return Get(ref _config, Config.Load); } set { _config = value; } }

        /// <summary>
        /// Describes an object that allows the storage and retrieval of <see cref="Implementation"/> directories.
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
        public ITrustManager TrustManager { get { return Get(ref _trustManager, () => new TrustManager(Config, OpenPgp, FeedCache, Handler)); } set { _trustManager = value; } }

        /// <summary>
        /// Allows configuration of the source used to request <see cref="Feed"/>s.
        /// </summary>
        public IFeedManager FeedManager { get { return Get(ref _feedManager, () => new FeedManager(Config, FeedCache, TrustManager, Handler)); } set { _feedManager = value; } }

        /// <summary>
        /// Provides access to remote and local <see cref="Catalog"/>s. Handles downloading, signature verification and caching.
        /// </summary>
        public ICatalogManager CatalogManager { get { return Get(ref _catalogManager, () => new CatalogManager(TrustManager)); } set { _catalogManager = value; } }

        /// <summary>
        /// An external package manager that can install <see cref="PackageImplementation"/>s.
        /// </summary>
        public IPackageManager PackageManager { get { return Get(ref _packageManager, PackageManagerFactory.Create); } set { _packageManager = value; } }

        /// <summary>
        /// Chooses a set of <see cref="Implementation"/>s to satisfy the requirements of a program and its user.
        /// </summary>
        public ISolver Solver
        {
            get
            {
                return Get(ref _solver, () => new FallbackSolver(
                    new BacktrackingSolver(Config, FeedManager, Store, PackageManager, Handler),
                    new PythonSolver(Config, FeedManager, Handler)));
            }
            set { _solver = value; }
        }

        /// <summary>
        /// Used to download missing <see cref="Implementation"/>s.
        /// </summary>
        public IFetcher Fetcher { get { return Get(ref _fetcher, () => new SequentialFetcher(Store, Handler)); } set { _fetcher = value; } }

        /// <summary>
        /// Executes a set of <see cref="Selections"/> as a program using dependency injection.
        /// </summary>
        public IExecutor Executor { get { return Get(ref _executor, () => new Executor(Store)); } set { _executor = value; } }

        /// <summary>
        /// Contains helper methods for filtering <see cref="Selections"/>.
        /// </summary>
        public SelectionsManager SelectionsManager { get { return Get(ref _selectionsManager, () => _selectionsManager = new SelectionsManager(FeedCache, Store, PackageManager)); } set { _selectionsManager = value; } }

        private static T Get<T>(ref T value, Func<T> build) where T : class
        {
            return value ?? (value = build());
        }
    }
}
