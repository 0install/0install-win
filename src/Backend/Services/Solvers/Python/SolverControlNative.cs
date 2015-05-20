/*
 * Copyright 2010-2015 Bastian Eicher
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
using NanoByte.Common.Cli;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;

namespace ZeroInstall.Services.Solvers.Python
{
    /// <summary>
    /// Interacts with the external process used by <see cref="PythonSolver"/> using a native Python distribution.
    /// </summary>
    internal sealed class SolverControlNative : CliAppControl, ISolverControl
    {
        private readonly ErrorParser _errorParser;

        public SolverControlNative(ITaskHandler handler)
        {
            _errorParser = new ErrorParser(handler);
        }

        /// <inheritdoc/>
        protected override string AppBinary { get { return "python"; } }

        /// <inheritdoc/>
        public new string Execute(params string[] arguments)
        {
            var result = base.Execute(arguments);
            _errorParser.Flush(); // Handle any left-over error messages
            return result;
        }

        /// <inheritdoc/>
        protected override ProcessStartInfo GetStartInfo(params string[] arguments)
        {
            string solverDirectory = BundledCliAppControl.GetBundledDirectory("Solver");

            // Launch solver script using Python
            var startInfo = base.GetStartInfo(arguments.Prepend(Path.Combine(solverDirectory, "0solve")));

            // Supress unimportant warnings
            startInfo.EnvironmentVariables["PYTHONWARNINGS"] = "ignore::DeprecationWarning";

            // Pass-through portable mode
            if (Locations.IsPortable)
            {
                startInfo.EnvironmentVariables["ZEROINSTALL_PORTABLE_BASE"] = Locations.PortableBase;
                startInfo.EnvironmentVariables["GNUPGHOME"] = Locations.GetSaveConfigPath("GnuPG", false, "gnupg");
            }

            return startInfo;
        }

        /// <inheritdoc/>
        protected override string HandleStderr(string line)
        {
            return _errorParser.HandleStdErrorLine(line);
        }
    }
}
