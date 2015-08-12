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
using System.IO;
using System.Linq;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;

namespace ZeroInstall.Commands.Utils
{
    /// <summary>
    /// Provides utility methods for operating on <see cref="AppList"/>s and <see cref="IIntegrationManager"/>s.
    /// </summary>
    public static class AppUtils
    {
        /// <summary>
        /// Removes all applications from the <see cref="AppList"/> and undoes any desktop environment integration.
        /// </summary>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="machineWide">Apply the operation machine-wide instead of just for the current user.</param>
        public static void RemoveAllApps(ITaskHandler handler, bool machineWide)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            using (var integrationManager = new IntegrationManager(handler, machineWide))
            {
                handler.RunTask(ForEachTask.Create(Resources.RemovingApplications, integrationManager.AppList.Entries.ToList(), integrationManager.RemoveApp));

                // Purge sync status, otherwise next sync would remove everything from server as well instead of restoring from there
                File.Delete(AppList.GetDefaultPath(machineWide) + SyncIntegrationManager.AppListLastSyncSuffix);
            }
        }
    }
}