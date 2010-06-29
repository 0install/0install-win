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
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Takes an interface URI and provides operations like solving and launching.
    /// </summary>
    public class Controller
    {
        #region Variables
        /// <summary>Cached <see cref="ISolver"/> results.</summary>
        private Selections _selections;
        #endregion

        #region Properties
        /// <summary>
        /// The URI or local path to the feed to solve the dependencies for.
        /// </summary>
        public string Feed { get; private set; }

        /// <summary>
        /// The solver to use for solving dependencies.
        /// </summary>
        public ISolver Solver { get; private set; }

        /// <summary>
        /// The user settings controlling the solving process.
        /// </summary>
        public Policy Policy { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new Launcher for a specific feed.
        /// </summary>
        /// <param name="feed">The URI or local path to the feed to solve the dependencies for.</param>
        /// <param name="solver">The solver to use for solving dependencies.</param>
        /// <param name="policy">The user settings controlling the solving process.</param>
        public Controller(string feed, ISolver solver, Policy policy)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feed)) throw new ArgumentNullException("feed");
            if (solver == null) throw new ArgumentNullException("solver");
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            Feed = feed;
            Solver = solver;
            Policy = policy;
        }
        #endregion
        
        //--------------------//

        #region Selections
        /// <summary>
        /// Uses an <see cref="ISolver"/> to solve the dependencies for the specified feed.
        /// </summary>
        /// <remarks>Feed files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        // ToDo: Add exceptions (feed problem, dependency problem)
        public void Solve()
        {
            // Run the solver algorithm
            _selections = Solver.Solve(Feed, Policy);
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
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Returns a new clone on each call")]
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
        /// <returns>An object that allows the main <see cref="IDImplementation"/> to be executed with all its <see cref="Dependency"/>s injected.</returns>
        /// <exception cref="InvalidOperationException">Thrown if neither <see cref="Solve"/> nor <see cref="SetSelections"/> was not called first.</exception>
        /// <remarks>Feed files may be downloaded, no implementations are downloaded.</remarks>
        public IEnumerable<Implementation> ListUncachedImplementations()
        {
            ICollection<Implementation> notCached = new LinkedList<Implementation>();

            foreach (var implementation in _selections.Implementations)
            {
                // Local paths are considered to be always available
                if (!string.IsNullOrEmpty(implementation.LocalPath)) continue;

                // Check if an implementation with a matching digest is available in the cache
                if (Policy.SearchStore.Contains(implementation.ManifestDigest)) continue;

                // If not, get download information for the implementation by checking the original feed file
                var feed = Policy.InterfaceCache.GetFeed(implementation.FromFeed ?? implementation.Interface);
                feed.Simplify();
                notCached.Add(feed.GetImplementation(implementation.ID));
            }

            return notCached;
        }

        /// <summary>
        /// Downloads any selected <see cref="IDImplementation"/>s that are missing from the <see cref="Injector.Policy.SearchStore"/>.
        /// </summary>
        /// <returns>An object that allows the main <see cref="IDImplementation"/> to be executed with all its <see cref="Dependency"/>s injected.</returns>
        /// <exception cref="InvalidOperationException">Thrown if neither <see cref="Solve"/> nor <see cref="SetSelections"/> was not called first.</exception>
        /// <remarks>Implementation archives may be downloaded, digest validation is performed. Will do nothing, if <see cref="NetworkLevel"/> is <see cref="NetworkLevel.Offline"/>.</remarks>
        // ToDo: Add callbacks and make asynchronous
        public void DownloadUncachedImplementations()
        {
            #region Sanity checks
            if (_selections == null) throw new InvalidOperationException(Resources.NotSolved);
            #endregion

            if (Policy.InterfaceCache.NetworkLevel == NetworkLevel.Offline) return;

            Policy.Fetcher.RunSync(new FetcherRequest(ListUncachedImplementations()));
        }
        #endregion

        #region Launcher
        /// <summary>
        /// Prepares to run an application.
        /// </summary>
        /// <returns>An object that allows the main <see cref="IDImplementation"/> to be executed with all its <see cref="Dependency"/>s injected.</returns>
        /// <exception cref="InvalidOperationException">Thrown if neither <see cref="Solve"/> nor <see cref="SetSelections"/> was not called first.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if <see cref="DownloadUncachedImplementations"/> was not called first.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performs extensive disk and network IO")]
        public Launcher GetLauncher()
        {
            #region Sanity checks
            if (_selections == null) throw new InvalidOperationException(Resources.NotSolved);
            #endregion

            // Read to run the application
            return new Launcher(_selections, Policy.SearchStore);
        }
        #endregion
    }
}
