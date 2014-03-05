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
using Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Removes all applications from the application list and undoes any desktop environment integration.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class RemoveAllApps : IntegrationCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "remove-all";

        /// <summary>The alternative name of this command as used in command-line arguments in lower-case.</summary>
        public const string AltName = "remove-all-apps";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionRemoveAllApps; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS]"; } }

        /// <inheritdoc/>
        public RemoveAllApps(ICommandHandler handler) : base(handler)
        {}
        #endregion

        /// <inheritdoc/>
        public override int Execute()
        {
            using (var integrationManager = new IntegrationManager(Handler, MachineWide))
            {
                if (AppList.Entries.Count == 0) return 0;

                Handler.ShowProgressUI();
                if (Handler.Batch || Handler.AskQuestion(Resources.ConfirmRemoveAll))
                {
                    Handler.RunTask(new ForEachTask<AppEntry>(Resources.RemovingApplications, AppList.Entries, integrationManager.RemoveApp));

                    // Purge sync status, otherwise next sync would remove everything from server as well instead of restoring from there
                    File.Delete(AppList.GetDefaultPath(MachineWide) + SyncIntegrationManager.AppListLastSyncSuffix);
                }
                else throw new OperationCanceledException();
            }

            return 0;
        }
    }
}
