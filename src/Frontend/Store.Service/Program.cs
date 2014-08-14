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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Utils;
using ZeroInstall.Store.Service.Properties;

namespace ZeroInstall.Store.Service
{
    /// <summary>
    /// Launches a Windows service for managing the secure shared cache of Zero Install implementations.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        // NOTE: No [STAThread] here, because it could block .NET remoting
        private static int Main(string[] args)
        {
            // Encode installation path into mutex name to allow instance detection during updates
            string mutexName = "mutex-" + Locations.InstallBase.GetHashCode();
            if (AppMutex.Probe(mutexName + "-update")) return 99;

            if (args == null || args.Length == 0)
            {
                // NOTE: Do not block updater from starting because it will automatically stop service

                ServiceBase.Run(new ServiceBase[] {new StoreService()});
                return 0;
            }
            else
            {
                AppMutex.Create(mutexName);
                if (Locations.IsPortable)
                {
                    Msg.Inform(null, Resources.NoPortableMode, MsgSeverity.Error);
                    return 1;
                }

                string command = args[0].ToLowerInvariant();
                bool silent = args.Contains("--silent", StringComparer.OrdinalIgnoreCase);
                try
                {
                    return HandleCommand(command, silent);
                }
                    #region Error handling
                catch (Win32Exception ex)
                {
                    if (!silent) Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return 1;
                }
                catch (InvalidOperationException ex)
                {
                    if (!silent) Msg.Inform(null, ex.Message, MsgSeverity.Error);
                    return 1;
                }
                #endregion
            }
        }

        #region Commands
        /// <summary>
        /// Handles command-line arguments.
        /// </summary>
        /// <param name="command">The primary command to execute.</param>
        /// <param name="silent"><see langword="true"/> if the command is to be executed without any visible user interface.</param>
        /// <returns>The process exit code.</returns>
        private static int HandleCommand(string command, bool silent)
        {
            switch (command)
            {
                case "install":
                    return Install(silent);
                case "uninstall":
                    return Uninstall(silent);
                case "start":
                    return Start(silent);
                case "stop":
                    Stop(silent);
                    return 0;
                case "status":
                    Status();
                    return 0;
                default:
                    Msg.Inform(null, string.Format(Resources.UnknownCommand, "0store-service (install|uninstall|start|stop|status) [--silent]"), MsgSeverity.Error);
                    return 1;
            }
        }

        private static int Install(bool silent)
        {
            using (var process = Process.Start(
                new ProcessStartInfo(InstallUtilPath, Application.ExecutablePath.EscapeArgument())
                {WindowStyle = (silent ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal)}))
            {
                process.WaitForExit();

                if (!silent)
                {
                    if (process.ExitCode == 0) Msg.Inform(null, Resources.InstallSuccess, MsgSeverity.Info);
                    else Msg.Inform(null, Resources.InstallFail, MsgSeverity.Error);
                }
                return process.ExitCode;
            }
        }

        private static int Uninstall(bool silent)
        {
            var controller = new ServiceController("0store-service");

            if (controller.Status == ServiceControllerStatus.Running) controller.Stop();

            using (var process = Process.Start(
                new ProcessStartInfo(InstallUtilPath, new[] {"/u", Application.ExecutablePath}.JoinEscapeArguments()) {WindowStyle = (silent ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal)}))
            {
                process.WaitForExit();

                if (!silent)
                {
                    if (process.ExitCode == 0) Msg.Inform(null, Resources.UninstallSuccess, MsgSeverity.Info);
                    else Msg.Inform(null, Resources.UninstallFail, MsgSeverity.Error);
                }
                return process.ExitCode;
            }
        }

        private static int Start(bool silent)
        {
            var controller = new ServiceController("0store-service");

            controller.Start();
            if (!silent) Msg.Inform(null, Resources.StartSuccess, MsgSeverity.Info);
            return 0;
        }

        private static void Stop(bool silent)
        {
            var controller = new ServiceController("0store-service");

            controller.Stop();
            if (!silent) Msg.Inform(null, Resources.StopSuccess, MsgSeverity.Info);
        }

        private static void Status()
        {
            var controller = new ServiceController("0store-service");

            Msg.Inform(null, (controller.Status == ServiceControllerStatus.Running ? Resources.StatusRunning : Resources.StatusStopped), MsgSeverity.Info);
        }

        /// <summary>
        /// The path to the .NET service installation utility.
        /// </summary>
        private static string InstallUtilPath
        {
            get
            {
                // Use .NET 4.0 if possible, otherwise 2.0
                string netFxDir = WindowsUtils.GetNetFxDirectory(
                    WindowsUtils.HasNetFxVersion(WindowsUtils.NetFx40) ? WindowsUtils.NetFx40 : WindowsUtils.NetFx20);

                return Path.Combine(netFxDir, "InstallUtil.exe");
            }
        }
        #endregion
    }
}
