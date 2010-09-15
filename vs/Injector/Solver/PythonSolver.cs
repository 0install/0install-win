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
using Common;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Feed;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Uses the Python script 0solve to solve dependencies.
    /// </summary>
    /// <remarks>This class is immutable.</remarks>
    public sealed class PythonSolver : CliAppControl, ISolver
    {
        #region Properties
        /// <inheritdoc/>
        protected override string AppName { get { return "Python"; } }

        /// <inheritdoc/>
        protected override string AppBinaryName { get { return "python"; } }

        private string SolverScript
        {
            get { return Path.Combine(Path.Combine(AppDirectory, "Scripts"), "0solve"); }
        }

        private string GnuPGDirectory
        {
            get { return Path.Combine(PortableDirectory, "GnuPG"); }
        }
        #endregion

        //--------------------//

        /// <inheritdoc/>
        protected override ProcessStartInfo GetStartInfo(string arguments)
        {
            var startInfo = base.GetStartInfo(arguments);

            // Add helper applications to search path
            startInfo.EnvironmentVariables["PATH"] = GnuPGDirectory + Path.PathSeparator + startInfo.EnvironmentVariables["PATH"];

            return startInfo;
        }

        /// <inheritdoc />
        public Selections Solve(string interfaceID, Policy policy)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            // Asynchronously parse all StandardError data
            string pendingQuestion = null, pendingError = null;
            var errorParser = new PythonErrorParser(question => pendingQuestion = question, error => pendingError = error);

            // Launch the script and pass in solver arguments with the interface ID
            string result = Execute(BuildArguments(interfaceID, policy), errorParser.HandleStdErrorLine, delegate
            {
                if (pendingQuestion != null)
                {
                    string answer = policy.InterfaceCache.Handler.AcceptNewKey(pendingQuestion) ? "Y" : "N";
                    pendingQuestion = null;
                    return answer;
                }
                if (pendingError != null) throw new SolverException(pendingError);
                return null;
            });
            errorParser.Flush();
            if (pendingError != null) throw new SolverException(pendingError);

            // Parse StandardOutput data as XML
            return Selections.LoadFromString(result);
        }

        private string BuildArguments(string interfaceID, Policy policy)
        {
            return "-W ignore::DeprecationWarning \"" + SolverScript + "\" " + GetSolverArguments(policy) + interfaceID;
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
    }
}
