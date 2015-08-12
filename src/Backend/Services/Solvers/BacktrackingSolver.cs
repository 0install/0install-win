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
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.PackageManagers;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Solvers
{
    /// <summary>
    /// Uses limited backtracking to solve <see cref="Requirements"/>. Does not find all possible solutions!
    /// </summary>
    /// <remarks>This class is immutable and thread-safe.</remarks>
    public partial class BacktrackingSolver : ISolver
    {
        #region Dependencies
        private readonly Config _config;
        private readonly IFeedManager _feedManager;
        private readonly IStore _store;
        private readonly IPackageManager _packageManager;
        private readonly ITaskHandler _handler;

        /// <summary>
        /// Creates a new simple solver.
        /// </summary>
        /// <param name="config">User settings controlling network behaviour, solving, etc.</param>
        /// <param name="store">Used to check which <see cref="Implementation"/>s are already cached.</param>
        /// <param name="feedManager">Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.</param>
        /// <param name="packageManager">An external package manager that can install <see cref="PackageImplementation"/>s.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        public BacktrackingSolver([NotNull] Config config, [NotNull] IFeedManager feedManager, [NotNull] IStore store, [NotNull] IPackageManager packageManager, [NotNull] ITaskHandler handler)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (feedManager == null) throw new ArgumentNullException(nameof(feedManager));
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (packageManager == null) throw new ArgumentNullException(nameof(packageManager));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            _config = config;
            _store = store;
            _packageManager = packageManager;
            _feedManager = feedManager;
            _handler = handler;
        }
        #endregion

        /// <inheritdoc/>
        public Selections Solve(Requirements requirements)
        {
            #region Sanity checks
            if (requirements == null) throw new ArgumentNullException(nameof(requirements));
            if (requirements.InterfaceUri == null) throw new ArgumentException(Resources.MissingInterfaceUri, nameof(requirements));
            #endregion

            Log.Info("Running Backtracking Solver for: " + requirements);

            var effectiveRequirements = requirements.GetEffective();
            var candidateProvider = new SelectionCandidateProvider(_config, _feedManager, _store, _packageManager, requirements.Languages);
            var solverRuns = effectiveRequirements.Select(x => new Pass(x, _handler.CancellationToken, candidateProvider));

            var successfullSolverRun = solverRuns.FirstOrDefault(x => x.TryToSolve());
            if (successfullSolverRun == null) throw new SolverException("No solution found");

            successfullSolverRun.Selections.PurgeRestrictions();
            successfullSolverRun.Selections.Implementations.Sort();
            return successfullSolverRun.Selections;
        }
    }
}
