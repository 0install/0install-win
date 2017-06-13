/*
 * Copyright 2010-2016 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using ZeroInstall.Services.Executors;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Fetchers;
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
        /// <summary>
        /// Creates a new service locator.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        public ServiceLocator([NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            Handler = handler;
        }

        /// <summary>
        /// A callback object used when the the user needs to be asked questions or informed about download and IO tasks.
        /// </summary>
        [NotNull]
        public ITaskHandler Handler { get; }

        private Config _config;

        /// <summary>
        /// User settings controlling network behaviour, solving, etc.
        /// </summary>
        [NotNull]
        public Config Config { get => Get(ref _config, Config.Load); set => _config = value; }

        private IStore _store;

        /// <summary>
        /// Describes an object that allows the storage and retrieval of <see cref="Implementation"/> directories.
        /// </summary>
        [NotNull]
        public IStore Store { get => Get(ref _store, StoreFactory.CreateDefault); set => _store = value; }

        private IOpenPgp _openPgp;

        /// <summary>
        /// Provides access to an encryption/signature system compatible with the OpenPGP standard.
        /// </summary>
        [NotNull]
        public IOpenPgp OpenPgp { get => Get(ref _openPgp, OpenPgpFactory.CreateDefault); set => _openPgp = value; }

        private IFeedCache _feedCache;

        /// <summary>
        /// Provides access to a cache of <see cref="Feed"/>s that were downloaded via HTTP(S).
        /// </summary>
        [NotNull]
        public IFeedCache FeedCache { get { return Get(ref _feedCache, () => FeedCacheFactory.CreateDefault(OpenPgp)); } set => _feedCache = value; }

        private TrustDB _trustDB;

        /// <summary>
        /// A database of OpenPGP signature fingerprints the users trusts to sign <see cref="Feed"/>s coming from specific domains.
        /// </summary>
        [NotNull]
        public TrustDB TrustDB { get => Get(ref _trustDB, TrustDB.LoadSafe); set => _trustDB = value; }

        private ITrustManager _trustManager;

        /// <summary>
        /// Methods for verifying signatures and user trust.
        /// </summary>
        [NotNull]
        public ITrustManager TrustManager { get { return Get(ref _trustManager, () => new TrustManager(Config, OpenPgp, TrustDB, FeedCache, Handler)); } set => _trustManager = value; }

        private IFeedManager _feedManager;

        /// <summary>
        /// Allows configuration of the source used to request <see cref="Feed"/>s.
        /// </summary>
        [NotNull]
        public IFeedManager FeedManager { get { return Get(ref _feedManager, () => new FeedManager(Config, FeedCache, TrustManager, Handler)); } set => _feedManager = value; }

        private ICatalogManager _catalogManager;

        /// <summary>
        /// Provides access to remote and local <see cref="Catalog"/>s. Handles downloading, signature verification and caching.
        /// </summary>
        [NotNull]
        public ICatalogManager CatalogManager { get { return Get(ref _catalogManager, () => new CatalogManager(TrustManager, Handler)); } set => _catalogManager = value; }

        private IPackageManager _packageManager;

        /// <summary>
        /// An external package manager that can install <see cref="PackageImplementation"/>s.
        /// </summary>
        [NotNull]
        public IPackageManager PackageManager { get => Get(ref _packageManager, PackageManagerFactory.Create); set => _packageManager = value; }

        private ISolver _solver;

        /// <summary>
        /// Chooses a set of <see cref="Implementation"/>s to satisfy the requirements of a program and its user.
        /// </summary>
        [NotNull]
        public ISolver Solver
        {
            get
            {
                return Get(ref _solver, () =>
                {
                    ISolver
                        backtrackingSolver = new BacktrackingSolver(Config, FeedManager, Store, PackageManager, Handler),
                        externalSolver = new ExternalSolver(backtrackingSolver, SelectionsManager, Fetcher, Executor, Config, FeedManager, Handler);
                    return new FallbackSolver(backtrackingSolver, externalSolver);
                });
            }
            set => _solver = value;
        }

        private IFetcher _fetcher;

        /// <summary>
        /// Used to download missing <see cref="Implementation"/>s.
        /// </summary>
        [NotNull]
        public IFetcher Fetcher { get { return Get(ref _fetcher, () => new SequentialFetcher(Config, Store, Handler)); } set => _fetcher = value; }

        private IExecutor _executor;

        /// <summary>
        /// Executes a <see cref="Selections"/> document as a program using dependency injection.
        /// </summary>
        [NotNull]
        public IExecutor Executor { get { return Get(ref _executor, () => new Executor(Store)); } set => _executor = value; }

        private ISelectionsManager _selectionsManager;

        /// <summary>
        /// Contains helper methods for filtering <see cref="Selections"/>.
        /// </summary>
        [NotNull]
        public ISelectionsManager SelectionsManager { get { return Get(ref _selectionsManager, () => _selectionsManager = new SelectionsManager(FeedManager, Store, PackageManager)); } set => _selectionsManager = value; }

        private static T Get<T>(ref T value, Func<T> build) where T : class
        {
            if (value == null)
            {
                value = build();
                Log.Debug("Initialized by Service Locator: " + value);
            }

            return value;
        }
    }
}
