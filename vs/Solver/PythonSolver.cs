/*
 * Copyright 2010 Bastian Eicher
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
using System.Windows.Forms;
using Common;
using ZeroInstall.Model;

namespace ZeroInstall.Solver
{
    /// <summary>
    /// Uses the Python implementation of 0launch to solve dependencies.
    /// </summary>
    public sealed class PythonSolver : ISolver
    {
        #region Properties
        private static string HelperDirectory
        {
            get
            {
                //return Environment.GetEnvironmentVariable("apps");
                return Path.GetDirectoryName(Application.ExecutablePath);
            }
        }

        /// <summary>
        /// The complete path to the Python interpreter binary.
        /// </summary>
        private static string PythonBinary
        {
            get { return Path.Combine(Path.Combine(HelperDirectory, "python"), @"python.exe"); }
        }

        /// <summary>
        /// The complete path to the Python solver script.
        /// </summary>
        private static string SolverScript
        {
            get { return Path.Combine(Path.Combine(Path.Combine(HelperDirectory, "python"), @"Scripts"), @"0launch"); }
        }

        /// <summary>
        /// Try to avoid network usage.
        /// </summary>
        public bool Offline { get; set; }

        /// <summary>
        /// Fetch a fresh copy of all used interfaces.
        /// </summary>
        public bool Refresh { get; set; }

        /// <summary>
        /// An additional directory to search for cached <see cref="Interface"/>s.
        /// </summary>
        public string AdditionalStore { get; set; }
        #endregion
        
        //--------------------//

        #region Solver access
        /// <summary>
        /// Solves the dependencies for a specific feed.
        /// </summary>
        /// <param name="feed">The feed to solve the dependencies for.</param>
        /// <returns>The <see cref="Implementation"/>s chosen for the feed.</returns>
        /// <remarks>Interface files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        public Selections Solve(Uri feed)
        {
            return Solve(feed, null);
        }
        
        /// <summary>
        /// Solves the dependencies for a specific feed.
        /// </summary>
        /// <param name="feed">The feed to solve the dependencies for.</param>
        /// <param name="notBefore">The minimum version of the main <see cref="Implementation"/> to choose.</param>
        /// <returns>The <see cref="Implementation"/>s chosen for the feed.</returns>
        /// <remarks>Interface files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        // This method uses an external Python script to perform the actual solving
        public Selections Solve(Uri feed, ImplementationVersion notBefore)
        {
            // Build the arguments list for the solver script
            string arguments = "--console --get-selections --select-only ";
            if (Offline) arguments += "--offline ";
            if (Refresh) arguments += "--refresh ";
            if (!string.IsNullOrEmpty(AdditionalStore)) arguments += "--with-store=" + AdditionalStore + " ";
            if (notBefore != null) arguments += "--not-before=" + notBefore + " ";
            arguments += feed;

            // Prepare to launch the Python interpreter (no window, redirect all output)
            var python = new ProcessStartInfo
            {
                FileName = PythonBinary,
                Arguments = SolverScript + " " + arguments,
                CreateNoWindow = true, UseShellExecute = false,
                RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
            };
            python.EnvironmentVariables["PATH"] = HelperDirectory + Path.PathSeparator + python.EnvironmentVariables["PATH"];

            // Start the Python process
            var process = Process.Start(python);
            if (process == null) throw new IOException("Unable to launch Python interpreter.");

            // ToDo: Parse stderr immediatley

            // Parse stdout after the process has completed
            process.WaitForExit();
            return Selections.Load(process.StandardOutput.BaseStream);
        }
        #endregion
    }
}
