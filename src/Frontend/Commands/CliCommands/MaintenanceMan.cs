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
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NDesk.Options;
using ZeroInstall.Commands.Properties;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Manages the integration of Zero Install itself in the operating system (deployment and removal).
    /// </summary>
    public sealed partial class MaintenanceMan : MultiCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "maintenance";

        /// <inheritdoc/>
        public MaintenanceMan([NotNull] ICommandHandler handler) : base(handler)
        {}
        #endregion

        /// <inheritdoc/>
        protected override IEnumerable<string> SubCommandNames { get { return new[] {Deploy.Name, Remove.Name}; } }

        /// <inheritdoc/>
        protected override SubCommand GetCommand(string commandName)
        {
            #region Sanity checks
            if (commandName == null) throw new ArgumentNullException("commandName");
            #endregion

            switch (commandName)
            {
                case Deploy.Name:
                    return new Deploy(Handler);
                case Remove.Name:
                    return new Remove(Handler);
                case RemoveHelper.Name:
                    return new RemoveHelper(Handler);
                default:
                    throw new OptionException(string.Format(Resources.UnknownCommand, commandName), commandName);
            }
        }

        internal abstract class MaintenanceSubCommand : SubCommand
        {
            protected override string ParentName { get { return MaintenanceMan.Name; } }

            protected MaintenanceSubCommand([NotNull] ICommandHandler handler) : base(handler)
            {}

            /// <summary>
            /// Tries to find an existing instance of Zero Install deployed on this system.
            /// </summary>
            /// <param name="machineWide"><c>true</c> to look only for machine-wide instances; <c>false</c> to look only for instances in the current user profile.</param>
            /// <returns>The installation directory of an instance of Zero Install; <c>null</c> if none was found.</returns>
            [CanBeNull]
            protected static string FindExistingInstance(bool machineWide)
            {
                if (!WindowsUtils.IsWindows) return null;

                string installLocation = RegistryUtils.GetSoftwareString("Zero Install", "InstallLocation", machineWide);
                if (string.IsNullOrEmpty(installLocation)) return null;
                if (!File.Exists(Path.Combine(installLocation, "0install.exe"))) return null;
                return installLocation;
            }
        }
    }
}
