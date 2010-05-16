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
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Interface;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Uses the Python implementation of 0launch to solve dependencies.
    /// </summary>
    public sealed class PythonSolver : ISolver
    {
        #region Static properties
        private static string HelperDirectory
        {
            get
            {
                // ToDo: Remove hack
                return Environment.GetEnvironmentVariable("apps");
                //return Path.GetDirectoryName(Application.ExecutablePath);
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
        #endregion

        #region Properties
        /// <summary>
        /// The source used to request <see cref="Interface"/>s.
        /// </summary>
        public InterfaceProvider InterfaceProvider { get; private set; }

        /// <summary>
        /// The location to search for cached <see cref="Implementation"/>s.
        /// </summary>
        public IStore Store { get; private set; }

        /// <summary>
        /// The architecture to to find <see cref="Implementation"/>s for.
        /// </summary>
        public Architecture Architecture { get; set; }
        
        /// <summary>
        /// Only choose <see cref="Implementation"/>s with a version number older than this.
        /// </summary>
        public ImplementationVersion Before { get; set; }

        /// <summary>
        /// Only choose <see cref="Implementation"/>s with a version number at least this new or newer.
        /// </summary>
        public ImplementationVersion NotBefore { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new Python-based solver.
        /// </summary>
        /// <param name="interfaceProvider">The source used to request <see cref="Interface"/>s.</param>
        /// <param name="store">The location to search for cached <see cref="Implementation"/>s.</param>
        public PythonSolver(InterfaceProvider interfaceProvider, IStore store)
        {
            #region Sanity checks
            if (interfaceProvider == null) throw new ArgumentNullException("interfaceProvider");
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            InterfaceProvider = interfaceProvider;
            Store = store;
        }
        #endregion

        //--------------------//

        #region Solve
        /// <summary>
        /// Solves the dependencies for a specific feed.
        /// </summary>
        /// <param name="feed">The URI or local path to the feed to solve the dependencies for.</param>
        /// <returns>The <see cref="ImplementationSelection"/>s chosen for the feed.</returns>
        /// <remarks>Interface files may be downloaded, signature validation is performed, implementations are not downloaded.</remarks>
        // ToDo: Add exceptions (feed problem, dependency problem)
        public Selections Solve(string feed)
        {
            // Build the arguments list for the solver script
            string arguments = "--console --get-selections --select-only ";
            if (InterfaceProvider.NetworkLevel == NetworkLevel.Offline) arguments += "--offline ";
            if (InterfaceProvider.Refresh) arguments += "--refresh ";
            if (Architecture.Cpu == Cpu.Source) arguments += "--source ";
            if (Before != null) arguments += "--before=" + NotBefore + " ";
            if (NotBefore != null) arguments += "--not-before=" + NotBefore + " ";
            arguments += feed;

            // Prepare to launch the Python interpreter (no window, redirect all output)
            var python = new ProcessStartInfo
            {
                FileName = PythonBinary,
                Arguments = SolverScript + " " + arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
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
