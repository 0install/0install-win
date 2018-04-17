// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

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
using ZeroInstall.Store.Implementations;

namespace ZeroInstall.Commands.CliCommands
{
    partial class MaintenanceMan
    {
        internal abstract class RemoveSubCommandBase : MaintenanceSubCommand
        {
            protected RemoveSubCommandBase([NotNull] ICommandHandler handler)
                : base(handler)
            {}

            [NotNull]
            protected abstract string TargetDir { get; }

            // Auto-detect portable targets by looking for flag file
            protected bool Portable => File.Exists(Path.Combine(TargetDir, Locations.PortableFlagName));

            // Auto-detect machine-wide targets by comparing path with registry entry
            protected bool MachineWide => !Portable && (TargetDir == FindExistingInstance(machineWide: true));

            protected void PerformRemove()
            {
                using (var manager = new MaintenanceManager(TargetDir, Handler, MachineWide, Portable))
                {
                    Log.Info($"Using Zero Install instance at '{Locations.InstallBase}' to remove '{TargetDir}'");
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

            public override string Description => Resources.DescriptionMaintenanceRemove;

            public override string Usage => "";

            protected override int AdditionalArgsMax => 0;

            public Remove([NotNull] ICommandHandler handler)
                : base(handler)
            {}
            #endregion

            protected override string TargetDir => Locations.InstallBase;

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
                => AppList.LoadSafe(machineWide).Entries.Any(x => x.AccessPoints != null);

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

            public override string Description => "Internal helper for '0install maintenance remove' used to support self-removal on Windows.";

            public override string Usage => "TARGET";

            protected override int AdditionalArgsMin => 1;

            protected override int AdditionalArgsMax => 1;

            public RemoveHelper([NotNull] ICommandHandler handler)
                : base(handler)
            {}
            #endregion

            protected override string TargetDir => AdditionalArgs[0];

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
            => new ProcessStartInfo("cmd.exe", "/c (ping 127.0.0.1 -n 8 || ping ::1 -n 8) & rd /s /q " + Locations.InstallBase.EscapeArgument())
            {
                UseShellExecute = false,
                CreateNoWindow = true
            }.Start();
    }
}
