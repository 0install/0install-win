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
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Removes all applications from the <see cref="AppList"/> and undoes any desktop environment integration.
    /// </summary>
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
        protected override int AdditionalArgsMax { get { return 0; } }
        #endregion

        /// <inheritdoc/>
        public RemoveAllApps([NotNull] ICommandHandler handler) : base(handler)
        {}

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            using (var integrationManager = new IntegrationManager(Handler, MachineWide))
            {
                if (integrationManager.AppList.Entries.Count == 0) return ExitCode.OK;

                if (Handler.Ask(Resources.ConfirmRemoveAll, defaultAnswer: true))
                {
                    Handler.RunTask(ForEachTask.Create(Resources.RemovingApplications, integrationManager.AppList.Entries.ToList(), integrationManager.RemoveApp));

                    // Purge sync status, otherwise next sync would remove everything from server as well instead of restoring from there
                    File.Delete(AppList.GetDefaultPath(MachineWide) + SyncIntegrationManager.AppListLastSyncSuffix);
                }
                else throw new OperationCanceledException();
            }

            return ExitCode.OK;
        }
    }
}
