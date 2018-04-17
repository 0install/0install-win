// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

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
