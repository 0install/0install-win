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
using System.ComponentModel;
using System.IO;
using Common;
using Common.Storage;
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
        /// Checks for updates to Zero Install itself.
        /// </summary>
        /// <param name="policy">Combines UI access, preferences and resources used to solve dependencies and download implementations.</param>
        /// <returns>The version number of the newest available update; <see langword="null"/> if no update is available.</returns>
        /// <exception cref="UserCancelException">Thrown if the user cancelled the operation.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted or if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if an operation failed due to insufficient rights.</exception>
        /// <exception cref="SolverException">Thrown if the dependencies could not be solved.</exception>
        public static ImplementationVersion CheckSelfUpdate(Policy policy)
        {
            #region Sanity checks
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            if (policy.Config.NetworkUse == NetworkLevel.Offline) return null;

            policy.FeedManager.Refresh = true;
            var requirements = new Requirements {InterfaceID = policy.Config.SelfUpdateID, CommandName = "update"};

            // Run solver
            var currentVersion = new ImplementationVersion(AppInfo.Version);
            bool staleFeeds;
            var selections = policy.Solver.Solve(requirements, policy, out staleFeeds);

            // Report version of current update if it is newer than the already installed version
            var newVersion = selections.Implementations[0].Version;
            return (newVersion > currentVersion) ? newVersion : null;
        }

        /// <summary>
        /// Downloads and installs updates to Zero Install itself.
        /// </summary>
        /// <param name="policy">Combines UI access, preferences and resources used to solve dependencies and download implementations.</param>
        /// <remarks>Application should exit itself after calling this.</remarks>
        /// <exception cref="FileNotFoundException">Thrown if the assembly could not be located.</exception>
        /// <exception cref="Win32Exception">Thrown if there was a problem launching the assembly.</exception>
        public static void RunSelfUpdate(Policy policy)
        {
            #region Sanity checks
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            var requirements = new Requirements {InterfaceID = policy.Config.SelfUpdateID, CommandName = "update"};

            // ToDo: Perform download in-process
            ProcessUtils.LaunchHelperAssembly("0install-win", "run --no-wait " + requirements.ToCommandLineArgs() + " \"" + Locations.InstallBase + "\"");
        }
        #endregion
    }
}
