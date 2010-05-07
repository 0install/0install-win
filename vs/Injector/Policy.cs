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

using ZeroInstall.DownloadBroker;
using ZeroInstall.Model;
using ZeroInstall.Solver;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Brings together a <see cref="Solver"/> and a <see cref="Fetcher"/> to create a <see cref="Launcher"/>.
    /// </summary>
    public class Policy
    {
        #region Properties
        /// <summary>
        /// Used to solve <see cref="Dependency"/>s.
        /// </summary>
        public ISolver Solver { get; private set; }

        /// <summary>
        /// Used to download missing <see cref="Implementation"/>s.
        /// </summary>
        public Fetcher Fetcher { get; private set; }

        /// <summary>
        /// The location to search for cached <see cref="Implementation"/>s.
        /// </summary>
        /// <remarks>This is usually the same as <see cref="DownloadBroker.Fetcher.Store"/>.</remarks>
        public virtual IStore Store { get { return Fetcher.Store; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new policy.
        /// </summary>
        /// <param name="solver">Used to solve <see cref="Dependency"/>s.</param>
        /// <param name="fetcher">Used to download missing <see cref="Implementation"/>s.</param>
        public Policy(ISolver solver, Fetcher fetcher)
        {
            Solver = solver;
            Fetcher = fetcher;
        }
        #endregion

        //--------------------//

        #region Actions
        /// <summary>
        /// Solves the dependencies for a specific feed based on the current settings.
        /// </summary>
        /// <param name="feed">The URI or local path to the feed to solve the dependencies for.</param>
        /// <returns>The <see cref="ImplementationSelection"/>s chosen for the feed.</returns>
        /// <remarks>Interface files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        // ToDo: Add exceptions (feed problem, dependency problem)
        public Selections GetSelections(string feed)
        {
            return Solver.Solve(feed);
        }

        /// <summary>
        /// Prepares to launch an application identified by a feed URI.
        /// </summary>
        /// <param name="feed">The URI or local path to the feed to be launched.</param>
        /// <returns>An object that allows the main <see cref="Implementation"/> to be executed with all its <see cref="Dependency"/>s injected.</returns>
        // ToDo: Add callbacks and make asynchronous
        // ToDo: Add exceptions (feed problem, dependency problem)
        public Launcher GetLauncher(string feed)
        {
            // Solve the dependencies
            var selections = GetSelections(feed);

            // Find out which implementations are missing and download them
            var missing = selections.GetUncachedImplementations(Store, Solver.InterfaceProvider);
            Fetcher.RunSync(new FetcherRequest(missing));

            // Read to run the application
            return new Launcher(selections, Store);
        }
        #endregion
    }
}
