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
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Commands.Utils;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store;

namespace ZeroInstall.Commands.CliCommands
{
    partial class MaintenanceMan
    {
        internal abstract class RemoveSubCommandBase : MaintenanceSubCommand
        {
            protected RemoveSubCommandBase([NotNull] ICommandHandler handler) : base(handler)
            {}

            [NotNull]
            protected abstract string TargetDir { get; }

            protected bool Portable
            {
                get
                {
                    // Auto-detect portable targets by looking for flag file
                    return File.Exists(Path.Combine(TargetDir, Locations.PortableFlagName));
                }
            }

            protected bool MachineWide
            {
                get
                {
                    // Auto-detect machine-wide targets by comparing path with registry entry
                    return !Portable && (TargetDir == FindExistingInstance(machineWide: true));
                }
            }

            protected void PerformRemove()
            {
                using (var manager = new MaintenanceManager(TargetDir, Handler, MachineWide, Portable))
                {
                    Log.Info(string.Format("Using Zero Install instance at '{0}' to remove '{1}'", Locations.InstallBase, TargetDir));
                    manager.Remove();
                }
            }
        }

        /// <summary>
        /// Removes the current instance of Zero Install from the system.
        /// </summary>
        internal class Remove : RemoveSubCommandBase
        {
            #region Metadata
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new const string Name = "remove";

            protected override string Description { get { return Resources.DescriptionMaintenanceRemove; } }

            protected override string Usage { get { return ""; } }

            protected override int AdditionalArgsMax { get { return 0; } }

            public Remove([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            protected override string TargetDir { get { return Locations.InstallBase; } }

            public override ExitCode Execute()
            {
                if (ProgramUtils.IsRunningFromCache)
                {
                    Log.Error("This instance of Zero Install is running from a cache. There is nothing to uninstall.");
                    return ExitCode.NoChanges;
                }

                if (MachineWide && !WindowsUtils.IsAdministrator)
                    throw new NotAdminException(Resources.MustBeAdminForMachineWide);

                if (!Handler.Ask(Resources.AskRemoveZeroInstall, defaultAnswer: true))
                    return ExitCode.UserCanceled;

                if (ExistingDesktopIntegration(MachineWide) || ExistingDesktopIntegration(machineWide: false))
                {
                    if (Handler.Ask(Resources.ConfirmRemoveAll, defaultAnswer: true))
                    {
                        AppUtils.RemoveAllApps(Handler, MachineWide);
                        if (MachineWide) AppUtils.RemoveAllApps(Handler, machineWide: false);
                    }
                }

                if (Handler.Ask(Resources.ConfirmPurge, defaultAnswer: false))
                    Store.Purge(Handler);

                if (WindowsUtils.IsWindows) DelegateToTempCopy();
                else PerformRemove();

                return ExitCode.OK;
            }

            /// <summary>
            /// Indicates whether any desktop integration for apps has been performed yet.
            /// </summary>
            private static bool ExistingDesktopIntegration(bool machineWide)
            {
                return AppList.LoadSafe(machineWide).Entries.Any(x => x.AccessPoints != null);
            }

            /// <summary>
            /// Deploys a portable copy of Zero Install to a temp directory and delegates the actual removal of the current instance to this copy.
            /// </summary>
            private void DelegateToTempCopy()
            {
                string tempDir = FileUtils.GetTempDirectory("0install-remove");
                using (var manager = new MaintenanceManager(tempDir, Handler, machineWide: false, portable: true))
                    manager.Deploy();

                string assembly = Path.Combine(tempDir, ProgramUtils.GuiAssemblyName ?? "0install");

                var args = new[] {MaintenanceMan.Name, RemoveHelper.Name, Locations.InstallBase};
                if (Handler.Verbosity == Verbosity.Batch) args = args.Append("--batch");
                if (Handler.Background) args = args.Append("--background");

                ProcessUtils.Assembly(assembly, args).Start();
            }
        }

        /// <summary>
        /// Internal helper for <see cref="Remove"/> used to support self-removal on Windows.
        /// </summary>
        private class RemoveHelper : RemoveSubCommandBase
        {
            #region Metadata
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new const string Name = "remove-helper";

            protected override string Description { get { return "Internal helper for '0install maintenance remove' used to support self-removal on Windows."; } }

            protected override string Usage { get { return "TARGET"; } }

            protected override int AdditionalArgsMin { get { return 1; } }

            protected override int AdditionalArgsMax { get { return 1; } }

            public RemoveHelper([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            protected override string TargetDir { get { return AdditionalArgs[0]; } }

            public override ExitCode Execute()
            {
                try
                {
                    if (!Locations.IsPortable || !WindowsUtils.IsWindows)
                        throw new NotSupportedException("This command is used as an internal helper and should not be called manually.");

                    PerformRemove();

                    return ExitCode.OK;
                }
                finally
                {
                    WindowsSelfDelete();
                }
            }
        }

        /// <summary>
        /// Use cmd.exe to delete own installation directory after 8s delay
        /// </summary>
        private static void WindowsSelfDelete()
        {
            // Abuse ping to acheive delay without any 3rd party tools
            new ProcessStartInfo("cmd.exe", "/c (ping 127.0.0.1 -n 8 || ping ::1 -n 8) & rd /s /q " + Locations.InstallBase.EscapeArgument())
            {
                UseShellExecute = false,
                CreateNoWindow = true
            }.Start();
        }
    }
}
