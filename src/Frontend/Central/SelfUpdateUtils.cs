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
using System.IO;
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Info;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Services;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Central
{
    /// <summary>
    /// Provides methods for updating Zero Install itself.
    /// </summary>
    public static class SelfUpdateUtils
    {
        /// <summary>
        /// <see langword="true"/> if automatic check for updates is enabled.
        /// </summary>
        public static bool AutoCheckEnabled
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
        /// <exception cref="OperationCanceledException">The user canceled the process.</exception>
        /// <exception cref="IOException">A problem occured while reading the feed file.</exception>
        /// <exception cref="WebException">A problem occured while fetching the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the cache is not permitted.</exception>
        /// <exception cref="SignatureException">The signature data of a remote feed file could not be verified.</exception>
        /// <exception cref="UriFormatException"><see cref="Config.SelfUpdateUri"/> is invalid.</exception>
        /// <exception cref="SolverException">The dependencies could not be solved.</exception>
        /// <exception cref="InvalidDataException">A configuration file is damaged.</exception>
        [CanBeNull]
        public static ImplementationVersion Check()
        {
            var services = new ServiceLocator(new SilentTaskHandler()) {FeedManager = {Refresh = true}};
            if (services.Config.NetworkUse == NetworkLevel.Offline) return null;

            // Run solver
            var requirements = new Requirements(services.Config.SelfUpdateUri, command: "update");
            var selections = services.Solver.Solve(requirements);

            // Report version of current update if it is newer than the already installed version
            var currentVersion = new ImplementationVersion(AppInfo.Current.Version);
            var newVersion = selections.Implementations[0].Version;
            return (newVersion > currentVersion) ? newVersion : null;
        }

        /// <summary>
        /// Starts the self-update process.
        /// </summary>
        /// <exception cref="NotSupportedException">Called on a non-Windows NT-based operating system.</exception>
        public static void Run()
        {
            if (WindowsUtils.IsWindowsNT) ProcessUtils.LaunchAssembly("0install-win", "self-update --restart-central");
            else throw new NotSupportedException();
        }
    }
}
