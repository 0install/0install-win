/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.IO;
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common.Info;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Services;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Commands.Utils
{
    /// <summary>
    /// Provides functionality for controlling Zero Install's self-update feature.
    /// </summary>
    public static class SelfUpdateUtils
    {
        /// <summary>
        /// The name of a file placed in <see cref="Locations.InstallBase"/> used to set <see cref="NoAutoCheck"/> to <c>true</c>.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
        public const string NoAutoCheckFlagName = "_no_self_update_check";

        private static string NoAutoCheckFlagFile { get { return Path.Combine(Locations.PortableBase, NoAutoCheckFlagName); } }

        /// <summary>
        /// <c>true</c> if automatic check for updates are disabled.
        /// </summary>
        public static bool NoAutoCheck
        {
            get { return File.Exists(NoAutoCheckFlagFile); }
            set
            {
                if (value) FileUtils.Touch(NoAutoCheckFlagFile);
                else File.Delete(NoAutoCheckFlagFile);
            }
        }

        /// <summary>
        /// Checks if updates for Zero Install itself are available without using the usual command infrastructure.
        /// </summary>
        /// <returns>The version number of the newest available update; <c>null</c> if no update is available.</returns>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="IOException">A problem occured while reading the feed file.</exception>
        /// <exception cref="WebException">A problem occured while fetching the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the cache is not permitted.</exception>
        /// <exception cref="SignatureException">The signature data of a remote feed file could not be verified.</exception>
        /// <exception cref="UriFormatException"><see cref="Config.SelfUpdateUri"/> is invalid.</exception>
        /// <exception cref="SolverException">The solver was unable to get information about the current version of Zero Install.</exception>
        /// <exception cref="InvalidDataException">A configuration file is damaged.</exception>
        [CanBeNull]
        public static ImplementationVersion SilentCheck()
        {
            var services = new ServiceLocator(new SilentTaskHandler()) {FeedManager = {Refresh = true}};
            if (services.Config.NetworkUse == NetworkLevel.Offline) return null;

            // Run solver
            var requirements = new Requirements(services.Config.SelfUpdateUri, command: "update");
            var selections = services.Solver.Solve(requirements);

            // Report version of current update if it is newer than the already installed version
            var currentVersion = new ImplementationVersion(AppInfo.Current.Version);
            var newVersion = selections.MainImplementation.Version;
            return (newVersion > currentVersion) ? newVersion : null;
        }
    }
}
