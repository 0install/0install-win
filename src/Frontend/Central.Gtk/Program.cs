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

using System.Linq;
using Gtk;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using ZeroInstall.Commands;

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
        /// The main entry point for the application.
        /// </summary>s
        private static int Main(string[] args)
        {
            ProgramUtils.Init();
            Application.Init();
            return Run(args);
        }

        /// <summary>
        /// Runs the application (called by main method or by embedding process).
        /// </summary>
        public static int Run(string[] args)
        {
            bool machineWide = args.Any(arg => arg == "-m" || arg == "--machine");
            if (machineWide && WindowsUtils.IsWindowsNT && !WindowsUtils.IsAdministrator) return ProcessUtils.RunAssemblyAsAdmin("ZeroInstall", args.JoinEscapeArguments());

            var window = new MainWindow();
            window.DeleteEvent += delegate { Application.Quit(); };
            window.Show();
            Application.Run();
            return 0;
        }
    }
}
