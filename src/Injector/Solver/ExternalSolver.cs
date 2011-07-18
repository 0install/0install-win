/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.IO;
using Common;
using Common.Utils;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Uses an external process to solve dependencies.
    /// </summary>
    /// <remarks>This class is immutable.</remarks>
    public sealed class ExternalSolver : ISolver
    {
        /// <inheritdoc />
        public Selections Solve(Requirements requirements, Policy policy, out bool staleFeeds)
        {
            #region Sanity checks
            if (requirements == null) throw new ArgumentNullException("requirements");
            if (policy == null) throw new ArgumentNullException("policy");
            if (string.IsNullOrEmpty(requirements.InterfaceID)) throw new ArgumentException(Resources.MissingInterfaceID, "requirements");
            #endregion

            // Sanitize interface ID (support both URIs and local paths)
            string interfaceID = requirements.InterfaceID.Replace("\"", "");
            if (interfaceID.Contains(" ")) interfaceID = "\"" + interfaceID + "\"";

            // Execute the external solver
            IExternalSolverControl control;
            if (WindowsUtils.IsWindows) control = new ExternalSolverControlBundled(); // Use bundled Python on Windows
            else control = new ExternalSolverControlNative(); // Use native Python everywhere else
            string arguments = GetSolverArguments(requirements, policy) + interfaceID;
            string result = control.ExecuteSolver(arguments, policy.Handler);

            // Detect when feeds get out-of-date
            staleFeeds = result.Contains("<!-- STALE_FEEDS -->");

            // Parse StandardOutput data as XML
            try { return Selections.LoadFromString(result); }
            #region Error handling
            catch (InvalidDataException ex)
            {
                Log.Warn("Solver result:\n" + result);
                throw new SolverException(Resources.ExternalSolverOutputErrror + "\n" + ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), ex);
            }
            #endregion
        }

        /// <summary>
        /// Generates a list of arguments to be passed on to the solver script.
        /// </summary>
        /// <param name="requirements">A set of requirements/restrictions imposed by the user on the implementation selection process.</param>
        /// <param name="policy">Combines UI access, preferences and resources used to solve dependencies and download implementations.</param>
        /// <returns>An empty string or a list of arguments terminated by a space.</returns>
        private static string GetSolverArguments(Requirements requirements, Policy policy)
        {
            string arguments = "";
            if (policy.Config.NetworkUse == NetworkLevel.Offline) arguments += "--offline ";
            if (policy.FeedManager.Refresh) arguments += "--refresh ";
            if (requirements.CommandName != null) arguments += "--command=\"" + requirements.CommandName + "\" ";
            if (requirements.BeforeVersion != null) arguments += "--before=" + requirements.BeforeVersion + " ";
            if (requirements.NotBeforeVersion != null) arguments += "--not-before=" + requirements.NotBeforeVersion + " ";
            if (requirements.Architecture.Cpu == Cpu.Source) arguments += "--source ";
            else
            {
                if (requirements.Architecture.OS != OS.All) arguments += "--os=" + requirements.Architecture.OSString + " ";
                if (requirements.Architecture.Cpu != Cpu.All) arguments += "--cpu=" + requirements.Architecture.CpuString + " ";
            }
            //if (additionalStore != null) arguments += "--store=" + additionalStore.DirectoryPath + " ";
            for (int i = 0; i < policy.Verbosity; i++)
                arguments += "--verbose ";

            return arguments;
        }
    }
}
