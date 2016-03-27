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

using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using ZeroInstall.Commands.Properties;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Opens the central graphical user interface for launching and managing applications.
    /// </summary>
    public sealed class Central : CliCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "central";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionCentral; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS]"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 0; } }
        #endregion

        #region State
        private bool _machineWide;

        /// <inheritdoc/>
        public Central([NotNull] ICommandHandler handler) : base(handler)
        {
            Options.Add("m|machine", () => Resources.OptionMachine, _ => _machineWide = true);
        }
        #endregion

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            if (_machineWide && !WindowsUtils.IsAdministrator)
                throw new NotAdminException(Resources.MustBeAdminForMachineWide);

            return (ExitCode)ProcessUtils.Assembly(WindowsUtils.IsWindows ? "ZeroInstall" : "ZeroInstall-gtk").Run();
        }
    }
}
