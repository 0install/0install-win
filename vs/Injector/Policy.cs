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
using ZeroInstall.DownloadBroker;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Model;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Interface;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Takes an interface URI and provides operations like solving and launching.
    /// </summary>
    /// <remarks>Brings together a <see cref="Solver"/>, <see cref="InterfaceCache"/> and a <see cref="Fetcher"/> to create a <see cref="Launcher"/>.</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public class Policy
    {
        #region Variables
        /// <summary>The URI or local path to the feed to solve the dependencies for.</summary>
        private readonly string _feed;

        /// <summary>Used to create <see cref="ISolver"/> instances.</summary>
        private readonly SolverFactory _solverFactory;

        /// <summary>Used to download missing <see cref="Implementation"/>s.</summary>
        private readonly Fetcher _fetcher;

        /// <summary>Cached <see cref="ISolver"/> results.</summary>
        private Selections _selections;
        #endregion

        #region Properties
        private readonly InterfaceCache _interfaceCache;
        /// <summary>
        /// Allows configuration of the source used to request <see cref="Interface"/>s.
        /// </summary>
        public InterfaceCache InterfaceCache { get { return _interfaceCache; } }

        /// <summary>
        /// Only choose <see cref="Implementation"/>s with certain version numbers.
        /// </summary>
        public Constraint Constraint { get; set; }

        /// <summary>
        /// Download source code instead of compiled binaries.
        /// </summary>
        public bool Source { get; set; }

        /// <summary>
        /// The architecture to to find <see cref="Implementation"/>s for. Leave unchanged to use the current system's architecture.
        /// </summary>
        public Architecture Architecture { get; set; }

        /// <summary>
        /// A location to search for cached <see cref="Implementation"/>s in addition to <see cref="Fetcher.Store"/>; may be <see langword="null"/>.
        /// </summary>
        public IStore AdditionalStore { get; set; }

        /// <summary>
        /// The locations to search for cached <see cref="Implementation"/>s.
        /// </summary>
        private IStore SearchStore
        {
            get
            {
                return (AdditionalStore == null
                    // No additional Store => search in same Stores the Fetcher writes to
                    ? _fetcher.Store
                    // Additional Stores => search in more Stores than the Fetcher writes to
                    : new StoreSet(new[] { AdditionalStore, _fetcher.Store }));
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new policy for a specific feed.
        /// </summary>
        /// <param name="feed">The URI or local path to the feed to solve the dependencies for.</param>
        /// <param name="solverProvider">Used to create <see cref="ISolver"/> instances.</param>
        /// <param name="interfaceCache">The source used to request <see cref="Interface"/>s.</param>
        /// <param name="fetcher">Used to download missing <see cref="Implementation"/>s.</param>
        public Policy(string feed, SolverFactory solverProvider, InterfaceCache interfaceCache, Fetcher fetcher)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feed)) throw new ArgumentNullException("feed");
            if (solverProvider == null) throw new ArgumentNullException("solverProvider");
            if (interfaceCache == null) throw new ArgumentNullException("interfaceCache");
            if (fetcher == null) throw new ArgumentNullException("fetcher");
            #endregion

            _feed = feed;
            _solverFactory = solverProvider;
            _interfaceCache = interfaceCache;
            _fetcher = fetcher;
        }
        #endregion

        #region Factory methods
        /// <summary>
        /// Creates a new policy for a specific feed using the default <see cref="SolverFactory"/>, <see cref="InterfaceCache"/> and <see cref="Fetcher"/>.
        /// </summary>
        /// <param name="feed">The URI or local path to the feed to solve the dependencies for.</param>
        public static Policy CreateDefault(string feed)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feed)) throw new ArgumentNullException("feed");
            #endregion

            return new Policy(feed, SolverFactory.Default, new InterfaceCache(), Fetcher.Default);
        }
        #endregion

        //--------------------//

        #region Selections
        /// <summary>
        /// Uses an <see cref="ISolver"/> to solve the dependencies for the specified feed.
        /// </summary>
        /// <remarks>Interface files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        // ToDo: Add exceptions (feed problem, dependency problem)
        public void Solve()
        {
            // Initialize a new solver with the current settings
            var solver = _solverFactory.CreateSolver(_interfaceCache, SearchStore);

            // Transfer user-selected limitations
            solver.Constraint = Constraint;

            // ToDo: Detect and set current architecture
            if (Source) solver.Architecture = new Architecture(solver.Architecture.OS, Cpu.Source);

            // Run the solver algorithm
            _selections = solver.Solve(_feed);
        }

        /// <summary>
        /// Uses an external set of <see cref="Selections"/> instead of using a <see cref="ISolver"/>.
        /// </summary>
        /// <param name="selections">The external set of <see cref="Selections"/> to use.</param>
        public void SetSelections(Selections selections)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            #endregion

            _selections = selections.CloneSelections();
        }

        /// <summary>
        /// Returns the <see cref="Selections"/> made earlier.
        /// </summary>
        /// <returns>The <see cref="ImplementationSelection"/>s chosen for the feed.</returns>
        /// <exception cref="InvalidOperationException">Thrown if neither <see cref="Solve"/> nor <see cref="SetSelections"/> was not called first.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method returns a new clone on each call")]
        public Selections GetSelections()
        {
            #region Sanity checks
            if (_selections == null) throw new InvalidOperationException(Resources.NotSolved);
            #endregion

            return _selections.CloneSelections();
        }
        #endregion

        #region Implementation
        /// <summary>
        /// Lists 
        /// </summary>
        /// <returns>An object that allows the main <see cref="Implementation"/> to be executed with all its <see cref="Dependency"/>s injected.</returns>
        /// <exception cref="InvalidOperationException">Thrown if neither <see cref="Solve"/> nor <see cref="SetSelections"/> was not called first.</exception>
        /// <remarks>Implementation archives may be downloaded, hash validation is performed. Will do nothing, if <see cref="NetworkLevel"/> is <see cref="NetworkLevel.Offline"/>.</remarks>
        public IEnumerable<Implementation> ListUncachedImplementations()
        {
            ICollection<Implementation> notCached = new LinkedList<Implementation>();

            foreach (var implementation in _selections.Implementations)
            {
                // Local paths are considered to be always available
                if (!string.IsNullOrEmpty(implementation.LocalPath)) continue;

                // Check if an implementation with a matching digest is available in the cache
                if (SearchStore.Contains(implementation.ManifestDigest)) continue;

                // If not, get download information for the implementation by checking the original interface file
                var interfaceInfo = _interfaceCache.GetInterface(implementation.Interface);
                interfaceInfo.Simplify();
                notCached.Add(interfaceInfo.GetImplementation(implementation.ID));
            }

            return notCached;
        }

        /// <summary>
        /// Downloads any selected <see cref="Implementation"/>s that are missing from the <see cref="SearchStore"/>.
        /// </summary>
        /// <returns>An object that allows the main <see cref="Implementation"/> to be executed with all its <see cref="Dependency"/>s injected.</returns>
        /// <exception cref="InvalidOperationException">Thrown if neither <see cref="Solve"/> nor <see cref="SetSelections"/> was not called first.</exception>
        /// <remarks>Implementation archives may be downloaded, hash validation is performed. Will do nothing, if <see cref="NetworkLevel"/> is <see cref="NetworkLevel.Offline"/>.</remarks>
        // ToDo: Add callbacks and make asynchronous
        public void DownloadUncachedImplementations()
        {
            #region Sanity checks
            if (_selections == null) throw new InvalidOperationException(Resources.NotSolved);
            #endregion

            if (InterfaceCache.NetworkLevel == NetworkLevel.Offline) return;

            _fetcher.RunSync(new FetcherRequest(ListUncachedImplementations()));
        }
        #endregion

        #region Launcher
        /// <summary>
        /// Prepares to launch an application.
        /// </summary>
        /// <returns>An object that allows the main <see cref="Implementation"/> to be executed with all its <see cref="Dependency"/>s injected.</returns>
        /// <exception cref="InvalidOperationException">Thrown if neither <see cref="Solve"/> nor <see cref="SetSelections"/> was not called first.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if <see cref="DownloadUncachedImplementations"/> was not called first.</exception>
        /// <remarks>Implementation archives may be downloaded, hash validation is performed.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method performs extensive disk and network IO")]
        public Launcher GetLauncher()
        {
            #region Sanity checks
            if (_selections == null) throw new InvalidOperationException(Resources.NotSolved);
            #endregion

            // Read to run the application
            return new Launcher(_selections, SearchStore);
        }
        #endregion
    }
}
