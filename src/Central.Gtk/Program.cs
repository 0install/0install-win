/*
 * Copyright 2010-2013 Bastian Eicher
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

using System.Linq;
using System.Security;
using System.Security.Cryptography;
using Common;
using Common.Storage;
using Common.Utils;
using Gtk;
using ZeroInstall.DesktopIntegration;

namespace ZeroInstall.Central.Gtk
{
    /// <summary>
    /// Launches a GTK# tool for managing caches of Zero Install implementations.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The canonical EXE name (without the file ending) for this binary.
        /// </summary>
        public const string ExeName = "ZeroInstall-gtk";

        /// <summary>
        /// The application user model ID used by the Windows 7 taskbar. Encodes <see cref="Locations.InstallBase"/> and the name of this sub-app.
        /// </summary>
        public static readonly string AppUserModelID = "ZeroInstall." + Locations.InstallBase.Hash(MD5.Create()) + ".Central";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>s
        private static int Main(string[] args)
        {
            WindowsUtils.SetCurrentProcessAppID(AppUserModelID);

            // Encode installation path into mutex name to allow instance detection during updates
            string mutexName = "mutex-" + Locations.InstallBase.Hash(MD5.Create());
            if (AppMutex.Probe(mutexName + "-update")) return 99;
            AppMutex.Create(mutexName);

            // Allow setup to detect Zero Install instances
#if !DEBUG
            AppMutex.Create("Zero Install");
#endif

            NetUtils.TrustCertificates(SyncIntegrationManager.DefaultServerPublicKey);
            return Run(args);
        }

        /// <summary>
        /// Runs the application (called by main method or by embedding process).
        /// </summary>
        public static int Run(string[] args)
        {
            bool machineWide = args.Any(arg => arg == "-m" || arg == "--machine");
            if (machineWide && WindowsUtils.IsWindowsNT && !WindowsUtils.IsAdministrator) return ProcessUtils.RunAssemblyAsAdmin("ZeroInstall", args.JoinEscapeArguments());
            
            Application.Init();
            var window = new MainWindow();
            window.DeleteEvent += delegate { Application.Quit(); };
            window.Show();
            Application.Run();
            return 0;
        }
    }
}
