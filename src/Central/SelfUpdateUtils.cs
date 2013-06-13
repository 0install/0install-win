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
using System.IO;
using Common.Info;
using Common.Storage;
using ZeroInstall.Backend;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Central
{
    /// <summary>
    /// Provides methods for updating Zero Install itself.
    /// </summary>
    public static class SelfUpdateUtils
    {
        /// <summary>
        /// <see langword="true"/> if <see cref="Check"/> should be called automatically.
        /// </summary>
        public static bool AutoActive
        {
            get
            {
                // Do not check for updates if Zero Install itself was launched as a Zero Install implementation
                if (StoreUtils.PathInAStore(Locations.InstallBase)) return false;

                // Flag file to supress check
                return !File.Exists(Path.Combine(Locations.PortableBase, "_no_self_update_check"));
            }
        }

        /// <summary>
        /// Checks if updates for Zero Install itself are available.
        /// </summary>
        /// <returns>The version number of the newest available update; <see langword="null"/> if no update is available.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the operation.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted or if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if an operation failed due to insufficient rights.</exception>
        /// <exception cref="InvalidDataException">Thrown if a configuration file is damaged.</exception>
        /// <exception cref="SolverException">Thrown if the dependencies could not be solved.</exception>
        public static ImplementationVersion Check()
        {
            var resolver = new Resolver(new SilentHandler());
            if (resolver.Config.EffectiveNetworkUse == NetworkLevel.Offline) return null;
            resolver.FeedManager.Refresh = true;

            // Run solver
            var requirements = new Requirements {InterfaceID = resolver.Config.SelfUpdateID, CommandName = "update"};
            var selections = resolver.Solver.Solve(requirements);

            // Report version of current update if it is newer than the already installed version
            var currentVersion = new ImplementationVersion(AppInfo.Current.Version);
            var newVersion = selections.Implementations[0].Version;
            return (newVersion > currentVersion) ? newVersion : null;
        }
    }
}
