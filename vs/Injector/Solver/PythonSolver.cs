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
using System.Text;
using System.Threading;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Uses the Python implementation of 0launch to solve dependencies.
    /// </summary>
    /// <remarks>This class is immutable.</remarks>
    public sealed class PythonSolver : ISolver
    {
        #region Static properties
        private static string HelperDirectory
        {
            get
            {
#if DEBUG
                // Use the current directory since the launching application might be a test runner in another directory
                string searchBase = Environment.CurrentDirectory;
#else
                // Use the base directory of the launching application since the current directory may be arbitrary
                string searchBase = AppDomain.CurrentDomain.BaseDirectory;
#endif

                if (Directory.Exists(Path.Combine(searchBase, "Python"))) return searchBase;
                return Path.Combine(Path.Combine(Path.Combine(searchBase, ".."), ".."), "Portable");
            }
        }

        private static string PythonDirectory
        {
            get { return Path.Combine(HelperDirectory, "Python"); }
        }

        private static string PythonBinary
        {
            get { return Path.Combine(PythonDirectory, "python.exe"); }
        }

        private static string SolverScript
        {
            get { return Path.Combine(Path.Combine(PythonDirectory, "Scripts"), "0solve"); }
        }

        private static string GnuPGDirectory
        {
            get { return Path.Combine(HelperDirectory, "GnuPG"); }
        }
        #endregion

        //--------------------//

        #region Solve
        /// <inheritdoc />
        public Selections Solve(string feed, Policy policy)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feed)) throw new ArgumentNullException("feed");
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            var process = new Process {StartInfo = GetStartInfo(feed, policy)};
            process.Start();

            // Asynchronously buffer all StandardOutput data
            var stdOut = new StringBuilder();
            process.OutputDataReceived += (sender, e) => stdOut.AppendLine(e.Data);
            process.BeginOutputReadLine();

            // Asynchronously parse all StandardError data
            string pendingQuestion = null, pendingError = null;
            var errorParser = new PythonErrorParser(question => pendingQuestion = question, error => pendingError = error);
            process.ErrorDataReceived += (sender, e) => errorParser.HandleStdErrorLine(e.Data);
            process.BeginErrorReadLine();

            while (!process.HasExited)
            {
                if (pendingQuestion != null)
                {
                    char answer = policy.InterfaceCache.Handler.AcceptNewKey(pendingQuestion) ? 'Y' : 'N';
                    process.StandardInput.WriteLine(answer);
                    process.StandardInput.Flush();
                    pendingQuestion = null;
                }
                if (pendingError != null) throw new SolverException(pendingError);
                Thread.Sleep(100);
            }
            process.WaitForExit();
            errorParser.Flush();
            if (pendingError != null) throw new SolverException(pendingError);

            // Parse StandardOutput data as XML
            return Selections.LoadFromString(stdOut.ToString());
        }
        #endregion

        #region Python subprocess
        /// <summary>
        /// Prepares to launch a the Python solver code in a child process.
        /// </summary>
        /// <param name="feed">The URI or local path to the feed to solve the dependencies for.</param>
        /// <param name="policy">The user settings controlling the solving process.</param>
        /// <returns>The <see cref="ProcessStartInfo"/> that can be used to start the new <see cref="Process"/>.</returns>
        private static ProcessStartInfo GetStartInfo(string feed, Policy policy)
        {
            // Prepare to launch the Python interpreter (no window, redirect all output)
            var startInfo = new ProcessStartInfo
            {
                FileName = PythonBinary,
                Arguments = "-W ignore::DeprecationWarning \"" + SolverScript + "\" " + GetSolverArguments(policy) + feed,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            // Add helper applications to search path
            startInfo.EnvironmentVariables["PATH"] = PythonDirectory + Path.PathSeparator + GnuPGDirectory + Path.PathSeparator + startInfo.EnvironmentVariables["PATH"];

            return startInfo;
        }

        /// <summary>
        /// Generates a list of arguments to be passed on to the solver script.
        /// </summary>
        /// <param name="policy">The user settings controlling the solving process.</param>
        /// <returns>An empty string or a list of arguments terminated by a space.</returns>
        private static string GetSolverArguments(Policy policy)
        {
            string arguments = "";
            if (policy.InterfaceCache.NetworkLevel == NetworkLevel.Offline) arguments += "--offline ";
            if (policy.InterfaceCache.Refresh) arguments += "--refresh ";
            if (policy.Constraint.BeforeVersion != null) arguments += "--before=" + policy.Constraint.BeforeVersion + " ";
            if (policy.Constraint.NotBeforeVersion != null) arguments += "--not-before=" + policy.Constraint.NotBeforeVersion + " ";
            if (policy.Architecture.Cpu == Cpu.Source) arguments += "--source ";
            else
            {
                if (policy.Architecture.OS != OS.All) arguments += "--os=" + policy.Architecture.OS;
                if (policy.Architecture.Cpu != Cpu.All) arguments += "--cpu=" + policy.Architecture.Cpu;
            }
            var additionalStore = policy.AdditionalStore as DirectoryStore;
            if (additionalStore != null) arguments += "--store=" + additionalStore.DirectoryPath;

            return arguments;
        }
        #endregion
    }
}
