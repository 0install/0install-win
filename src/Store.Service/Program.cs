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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Windows.Forms;
using Common;
using Common.Storage;
using Common.Utils;
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
        public static int Main(string[] args)
        {
            // Encode installation path into mutex name to allow instance detection during updates
            string mutexName = "mutex-" + Locations.InstallBase.Hash(MD5.Create());
            if (AppMutex.Probe(mutexName + "-update")) return 1;

            if (args.Length == 0)
            {
                // NOTE: Do not block updater from starting because it will automatically stop service

                ServiceBase.Run(new ServiceBase[] {new StoreService()});
                return 0;
            }
            else
            {
                AppMutex.Create(mutexName);

                string command = args[0].ToLowerInvariant();
                bool silent = args.Contains("--silent", StringComparer.InvariantCultureIgnoreCase);
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
            var controller = new ServiceController("0store-service");
            switch (command)
            {
                case "install":
                {
                    if (Locations.IsPortable)
                    {
                        Msg.Inform(null, Resources.NoPortableMode, MsgSeverity.Error);
                        return 1;
                    }

                    var process = Process.Start(
                        new ProcessStartInfo(InstallUtilPath, Application.ExecutablePath.EscapeArgument())
                        {WindowStyle = (silent ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal)});
                    process.WaitForExit();

                    if (!silent)
                    {
                        if (process.ExitCode == 0) Msg.Inform(null, Resources.InstallSuccess, MsgSeverity.Info);
                        else Msg.Inform(null, Resources.InstallFail, MsgSeverity.Error);
                    }
                    return process.ExitCode;
                }

                case "uninstall":
                {
                    if (controller.Status == ServiceControllerStatus.Running) controller.Stop();

                    var process = Process.Start(
                        new ProcessStartInfo(InstallUtilPath, new[] {"/u", Application.ExecutablePath}.JoinEscapeArguments())
                        {WindowStyle = (silent ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal)});
                    process.WaitForExit();

                    if (!silent)
                    {
                        if (process.ExitCode == 0) Msg.Inform(null, Resources.UninstallSuccess, MsgSeverity.Info);
                        else Msg.Inform(null, Resources.UninstallFail, MsgSeverity.Error);
                    }
                    return process.ExitCode;
                }

                case "start":
                    controller.Start();
                    if (!silent) Msg.Inform(null, Resources.StartSuccess, MsgSeverity.Info);
                    return 0;

                case "stop":
                    controller.Stop();
                    if (!silent) Msg.Inform(null, Resources.StopSuccess, MsgSeverity.Info);
                    return 0;

                default:
                    Msg.Inform(null, string.Format(Resources.UnkownCommand, "0store-service (install|uninstall|start|stop) [--silent]"), MsgSeverity.Error);
                    return 1;
            }
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
