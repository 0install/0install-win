/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.IO;
using Common.Utils;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;

namespace ZeroInstall.Central
{
    /// <summary>
    /// Helper methods for updating installed applications.
    /// </summary>
    public static class UpdateUtils
    {
        #region Self-update
        /// <summary>
        /// Checks if updates for Zero Install itself are available.
        /// </summary>
        /// <returns>The version number of the newest available update; <see langword="null"/> if no update is available.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the operation.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted or if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if an operation failed due to insufficient rights.</exception>
        /// <exception cref="SolverException">Thrown if the dependencies could not be solved.</exception>
        public static ImplementationVersion CheckSelfUpdate()
        {
            var policy = Policy.CreateDefault(new SilentHandler());
            if (policy.Config.EffectiveNetworkUse == NetworkLevel.Offline) return null;
            policy.FeedManager.Refresh = true;

            // Run solver
            bool staleFeeds;
            var requirements = new Requirements {InterfaceID = policy.Config.SelfUpdateID, CommandName = "update", Architecture = Architecture.CurrentSystem};
            var selections = policy.Solver.Solve(requirements, policy, out staleFeeds);

            // Report version of current update if it is newer than the already installed version
            var currentVersion = new ImplementationVersion(AppInfo.Version);
            var newVersion = selections.Implementations[0].Version;
            return (newVersion > currentVersion) ? newVersion : null;
        }
        #endregion
    }
}
