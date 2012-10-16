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
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Reintegrate all applications in the application list into the desktop environment.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class RepairApps : IntegrationCommand
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "repair-apps";
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionRepairApps; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[PATTERN]"; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public RepairApps(Policy policy) : base(policy)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count > 0) throw new OptionException(Resources.TooManyArguments, "");

            if (MachineWide && WindowsUtils.IsWindows && !WindowsUtils.IsAdministrator) return RerunAsAdmin();

            Policy.Handler.ShowProgressUI();
            using (var integrationManager = new IntegrationManager(MachineWide, Policy.Handler))
                integrationManager.Repair(feedID => Policy.FeedManager.GetFeed(feedID, Policy));
            return 0;
        }
        #endregion
    }
}
