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
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// List all current <see cref="AppEntry"/>s in the <see cref="AppList"/>.
    /// </summary>
    public sealed class ListApps : IntegrationCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "list-apps";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionListApps; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[PATTERN]"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 1; } }
        #endregion

        /// <inheritdoc/>
        public ListApps([NotNull] ICommandHandler handler) : base(handler)
        {}

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            IEnumerable<AppEntry> apps = AppList.LoadSafe(MachineWide).Entries;
            if (AdditionalArgs.Count > 0) apps = apps.Where(x => x.Name.ContainsIgnoreCase(AdditionalArgs[0]));

            Handler.Output(Resources.MyApps, apps);
            return ExitCode.OK;
        }
    }
}
