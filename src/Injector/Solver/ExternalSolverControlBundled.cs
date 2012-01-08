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

using System.Diagnostics;
using System.IO;
using Common.Cli;
using Common.Storage;
using Common.Utils;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Interacts with the external process used by <see cref="ExternalSolver"/> using a bundled Python distribution.
    /// </summary>
    internal sealed class ExternalSolverControlBundled : BundledCliAppControl, IExternalSolverControl
    {
        /// <inheritdoc/>
        protected override string AppBinary { get { return "0solve"; } }

        /// <inheritdoc/>
        protected override string AppDirName { get { return "Solver"; } }

        /// <inheritdoc/>
        protected override ProcessStartInfo GetStartInfo(string arguments)
        {
            var startInfo = base.GetStartInfo(arguments);

            // Supress unimportant warnings
            startInfo.EnvironmentVariables["PYTHONWARNINGS"] = "ignore::DeprecationWarning";

            // Pass-through portable mode
            if (Locations.IsPortable)
            {
                startInfo.EnvironmentVariables["ZEROINSTALL_PORTABLE_BASE"] = Locations.PortableBase;
                startInfo.EnvironmentVariables["GNUPGHOME"] = Locations.GetSaveConfigPath("GnuPG", false, "gnupg");
            }

            // Add bundled GnuPG to search path for the external solver to use on Windows
            string gnuPGDirectory = GetBundledDirectory("GnuPG");
            if (WindowsUtils.IsWindows && File.Exists(Path.Combine(gnuPGDirectory, "gpg.exe")))
                startInfo.EnvironmentVariables["PATH"] = gnuPGDirectory + Path.PathSeparator + startInfo.EnvironmentVariables["PATH"];

            return startInfo;
        }

        /// <inheritdoc/>
        public string ExecuteSolver(string arguments, IHandler handler)
        {
            var errorParser = new ExternalSolverErrorParser(handler);
            var result = Execute(arguments, null, errorParser.HandleStdErrorLine);
            errorParser.Flush(); // Handle any left-over error messages
            return result;
        }
    }
}
