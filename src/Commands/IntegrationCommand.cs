using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Common.Storage;
using Common.Utils;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Common base class for commands that manage <see cref="ZeroInstall.DesktopIntegration"/>.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class IntegrationCommand : CommandBase
    {
        #region Variables
        /// <summary>Apply the operation system-wide instead of just for the current user.</summary>
        protected bool SystemWide;
        #endregion

        #region Constructor
        /// <inheritdoc/>
        protected IntegrationCommand(Policy policy) : base(policy)
        {
            Options.Add("batch", Resources.OptionBatch, unused => Policy.Handler.Batch = true);

            Options.Add("o|offline", Resources.OptionOffline, unused => Policy.Config.NetworkUse = NetworkLevel.Offline);
            Options.Add("r|refresh", Resources.OptionRefresh, unused => Policy.FeedManager.Refresh = true);

            Options.Add("g|global", Resources.OptionGlobal, unused => SystemWide = true);
        }
        #endregion

        #region Admin
        /// <summary>
        /// Reruns the current command as an administrator.
        /// </summary>
        /// <returns>The exit code of the rerun command.</returns>
        /// <remarks>The command-line arguments are pulled from the current process.</remarks>
        /// <exception cref="PlatformNotSupportedException">When called on a non-Windows platform.</exception>
        protected static int RerunAsAdmin()
        {
            if (!WindowsUtils.IsWindows) throw new PlatformNotSupportedException();

            var commandLine = new LinkedList<string>(Environment.GetCommandLineArgs());
            string executable = commandLine.First.Value;
            commandLine.RemoveFirst();
            if (executable.StartsWith("0alias")) commandLine.AddFirst(AddAlias.Name);

            var startInfo = new ProcessStartInfo(Path.Combine(Locations.InstallBase, "0install-win.exe"), StringUtils.ConcatenateEscapeArgument(commandLine)) {Verb = "runas"};
            var process = Process.Start(startInfo);
            process.WaitForExit();
            return process.ExitCode;
        }
        #endregion
    }
}