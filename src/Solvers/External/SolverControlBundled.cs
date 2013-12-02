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

using System.Diagnostics;
using System.IO;
using Common.Cli;
using Common.Storage;
using Common.Utils;
using ZeroInstall.Store;

namespace ZeroInstall.Solvers.External
{
    /// <summary>
    /// Interacts with the external process used by <see cref="ExternalSolver"/> using a bundled Python distribution.
    /// </summary>
    internal sealed class SolverControlBundled : BundledCliAppControl, ISolverControl
    {
        private readonly IHandler _handler;

        public SolverControlBundled(IHandler handler)
        {
            _handler = handler;
        }

        /// <inheritdoc/>
        protected override string AppBinary { get { return "0solve"; } }

        /// <inheritdoc/>
        protected override string AppDirName { get { return "Solver"; } }

        /// <inheritdoc/>
        protected override ProcessStartInfo GetStartInfo(string arguments, bool hidden = false)
        {
            var startInfo = base.GetStartInfo(arguments, hidden);

            // Supress unimportant warnings
            startInfo.EnvironmentVariables["PYTHONWARNINGS"] = "ignore::DeprecationWarning";

            // Pass-through portable mode
            if (Locations.IsPortable)
            {
                startInfo.EnvironmentVariables["ZEROINSTALL_PORTABLE_BASE"] = Locations.PortableBase;
                startInfo.EnvironmentVariables["GNUPGHOME"] = Locations.GetSaveConfigPath("GnuPG", false, "gnupg");
            }

            // Add bundled GnuPG to environment variable for the external solver to use on Windows
            string gpgPath = Path.Combine(GetBundledDirectory("GnuPG"), "gpg.exe");
            if (WindowsUtils.IsWindows && File.Exists(gpgPath)) startInfo.EnvironmentVariables["ZEROINSTALL_GPG"] = gpgPath;

            return startInfo;
        }

        /// <inheritdoc/>
        public string ExecuteSolver(string arguments)
        {
            var errorParser = new ErrorParser(_handler);
            var result = Execute(arguments, null, errorParser.HandleStdErrorLine);
            errorParser.Flush(); // Handle any left-over error messages
            return result;
        }
    }
}
