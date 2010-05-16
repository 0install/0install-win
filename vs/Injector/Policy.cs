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
    /// <remarks>Brings together a <see cref="Solver"/> and a <see cref="Fetcher"/> to create a <see cref="Launcher"/>.</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public class Policy
    {
        #region Variables
        /// <summary>The URI or local path to the feed to solve the dependencies for.</summary>
        private readonly string _feed;

        /// <summary>Used to create <see cref="ISolver"/> instances.</summary>
        private readonly SolverProvider _solverProvider;

        /// <summary>The source used to request <see cref="Interface"/>s.</summary>
        private readonly InterfaceProvider _interfaceProvider;

        /// <summary>Used to download missing <see cref="Implementation"/>s.</summary>
        private readonly Fetcher _fetcher;

        /// <summary>Cached <see cref="ISolver"/> results.</summary>
        private Selections _selections;
        #endregion

        #region Properties
        /// <summary>
        /// Allows configuration of the source used to request <see cref="Interface"/>s.
        /// </summary>
        public InterfaceProvider InterfaceProvider { get { return _interfaceProvider; } }

        /// <summary>
        /// Only choose <see cref="Implementation"/>s with a version number older than this.
        /// </summary>
        public ImplementationVersion Before { get; set; }

        /// <summary>
        /// Only choose <see cref="Implementation"/>s with a version number at least this new or newer.
        /// </summary>
        public ImplementationVersion NotBefore { get; set; }

        /// <summary>
        /// Download source code instead of compiled binaries.
        /// </summary>
        public bool Source { get; set; }

        /// <summary>
        /// A location to search for cached <see cref="Implementation"/>s in addition to <see cref="Fetcher.Store"/>; may be <see langword="null"/>.
        /// </summary>
        public IStore AdditionalStore { get; set; }

        /// <summary>
        /// The locations to search for cached <see cref="Implementation"/>s.
        /// </summary>
        protected IStore SearchStore
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
        /// <param name="interfaceProvider">The source used to request <see cref="Interface"/>s.</param>
        /// <param name="fetcher">Used to download missing <see cref="Implementation"/>s.</param>
        public Policy(string feed, SolverProvider solverProvider, InterfaceProvider interfaceProvider, Fetcher fetcher)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feed)) throw new ArgumentNullException("feed");
            if (solverProvider == null) throw new ArgumentNullException("solverProvider");
            if (interfaceProvider == null) throw new ArgumentNullException("interfaceProvider");
            if (fetcher == null) throw new ArgumentNullException("fetcher");
            #endregion

            _feed = feed;
            _solverProvider = solverProvider;
            _interfaceProvider = interfaceProvider;
            _fetcher = fetcher;
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
            var solver = _solverProvider.CreateSolver(_interfaceProvider, SearchStore);

            // Transfer user-selected limitations
            solver.Before = Before;
            solver.NotBefore = NotBefore;

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
        /// Prepares to launch an application.
        /// </summary>
        /// <returns>An object that allows the main <see cref="Implementation"/> to be executed with all its <see cref="Dependency"/>s injected.</returns>
        /// <exception cref="InvalidOperationException">Thrown if neither <see cref="Solve"/> nor <see cref="SetSelections"/> was not called first.</exception>
        /// <remarks>Implementation archives may be downloaded, hash validation is performed.</remarks>
        // ToDo: Add callbacks and make asynchronous
        public void DownloadMissingImplementations()
        {
            #region Sanity checks
            if (_selections == null) throw new InvalidOperationException(Resources.NotSolved);
            #endregion

            // Find out which implementations are missing and download them
            var missing = _selections.GetUncachedImplementations(SearchStore, InterfaceProvider);
            if (InterfaceProvider.NetworkLevel != NetworkLevel.Offline) _fetcher.RunSync(new FetcherRequest(missing));
        }
        #endregion

        #region Launcher
        /// <summary>
        /// Prepares to launch an application.
        /// </summary>
        /// <returns>An object that allows the main <see cref="Implementation"/> to be executed with all its <see cref="Dependency"/>s injected.</returns>
        /// <exception cref="InvalidOperationException">Thrown if neither <see cref="Solve"/> nor <see cref="SetSelections"/> was not called first.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if <see cref="DownloadMissingImplementations"/> was not called first.</exception>
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
