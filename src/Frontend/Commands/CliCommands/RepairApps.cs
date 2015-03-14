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
using JetBrains.Annotations;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services.Feeds;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Reintegrate all applications in the <see cref="AppList"/> into the desktop environment.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class RepairApps : IntegrationCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "repair-all";

        /// <summary>The alternative name of this command as used in command-line arguments in lower-case.</summary>
        public const string AltName = "repair-apps";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionRepairApps; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS]"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 0; } }
        #endregion

        /// <inheritdoc/>
        public RepairApps([NotNull] ICommandHandler handler) : base(handler)
        {}

        /// <inheritdoc/>
        public override int Execute()
        {
            using (var integrationManager = new IntegrationManager(Handler, MachineWide))
                integrationManager.Repair(FeedManager.GetFeedFresh);

            return 0;
        }
    }
}
