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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common.Utils;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Model;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Executes a set of <see cref="ImplementationSelection"/>s as a program using dependency injection.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public partial class Executor
    {
        #region Properties
        /// <summary>
        /// The specific <see cref="Model.Implementation"/>s chosen for the <see cref="Dependency"/>s.
        /// </summary>
        public Selections Selections { get; private set; }

        /// <summary>
        /// Used to locate the selected <see cref="Model.Implementation"/>s.
        /// </summary>
        public IStore Store { get; private set; }

        /// <summary>
        /// An alternative executable to to run from the main <see cref="Model.Implementation"/> instead of <see cref="Element.Main"/>. May not contain command-line arguments! Whitespaces do not need to be escaped.
        /// </summary>
        public string Main { get; set; }

        /// <summary>
        /// Instead of executing the selected program directly, pass it as an argument to this program. Useful for debuggers. May contain command-line arguments. Whitespaces must be escaped!
        /// </summary>
        public string Wrapper { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new launcher from <see cref="Selections"/>.
        /// </summary>
        /// <param name="selections">
        /// The specific <see cref="ImplementationSelection"/>s chosen for the <see cref="Dependency"/>s.
        /// This object must _not_ be modified once it has been passed into this constructor!
        /// </param>
        /// <param name="store">Used to locate the selected <see cref="Model.Implementation"/>s.</param>
        public Executor(Selections selections, IStore store)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (store == null) throw new ArgumentNullException("store");
            if (selections.Implementations.Count == 0) throw new ArgumentException(Resources.NoImplementationsPassed, "selections");
            #endregion

            Selections = selections;
            Store = store;
        }
        #endregion

        //--------------------//

        #region Start process
        /// <summary>
        /// Prepares a <see cref="ProcessStartInfo"/> for executing the program as specified by the <see cref="Selections"/>.
        /// </summary>
        /// <param name="arguments">Arguments to be passed to the launched programs.</param>
        /// <returns>The <see cref="ProcessStartInfo"/> that can be used to start the new <see cref="Process"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if <see cref="Selections"/> contains <see cref="Dependency"/>s pointing to interfaces without selections.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="Model.Implementation"/>s is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if there was a problem locating the implementation executable.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to a file is not permitted.</exception>
        /// <exception cref="Win32Exception">Thrown if a problem occurred while creating a hard link.</exception>
        public ProcessStartInfo GetStartInfo(params string[] arguments)
        {
            #region Sanity checks
            if (arguments == null) throw new ArgumentNullException("arguments");
            #endregion

            // Apply implementation bindings
            var startInfo = BuildStartInfo();

            // Get the actual implementation to be started (and replace its binary if the user wanted that)
            var mainImplementation = Selections.MainImplementation;
            if (!string.IsNullOrEmpty(Main)) ApplyMain(ref mainImplementation);

            // Recursivley build command-line (applying additional bindings)
            var commandLine = GetCommandLine(mainImplementation, Selections.CommandName, startInfo);
            if (!string.IsNullOrEmpty(Wrapper)) commandLine.InsertRange(0, WindowsUtils.SplitArgs(Wrapper)); // Add wrapper in front
            commandLine.AddRange(arguments); // Append user arguments

            // Split and apply command-lines for executable bindings (delayed until here because there may be variable expanding)
            foreach (var pending in _runEnvPendings)
            {
                var split = SplitCommandLine(pending.CommandLine, startInfo.EnvironmentVariables);
                startInfo.EnvironmentVariables.Add("0install-runenv-file-" + pending.Name, split.FileName);
                startInfo.EnvironmentVariables.Add("0install-runenv-args-" + pending.Name, split.Arguments);
            }
            _runEnvPendings.Clear();

            // Split and apply main command-line
            {
                var split = SplitCommandLine(commandLine, startInfo.EnvironmentVariables);
                startInfo.FileName = split.FileName;
                startInfo.Arguments = split.Arguments;
            }
            return startInfo;
        }

        /// <summary>
        /// Starts the program as specified by the <see cref="Selections"/>.
        /// </summary>
        /// <param name="arguments">Arguments to be passed to the launched programs.</param>
        /// <returns>The newly created <see cref="Process"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if <see cref="Selections"/> contains <see cref="Dependency"/>s pointing to interfaces without selections.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="Model.Implementation"/>s is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if there was a problem locating the implementation executable.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to a file is not permitted.</exception>
        /// <exception cref="Win32Exception">Thrown if the main executable could not be launched or if a problem occurred while creating a hard link.</exception>
        public Process Start(params string[] arguments)
        {
            #region Sanity checks
            if (arguments == null) throw new ArgumentNullException("arguments");
            #endregion

            return Process.Start(GetStartInfo(arguments));
        }
        #endregion

        #region Path
        /// <summary>
        /// Locates an implementation on the disk (usually in an <see cref="IStore"/>).
        /// </summary>
        /// <param name="implementation">The <see cref="ImplementationBase"/> to be located.</param>
        /// <returns>A fully qualified path pointing to the implementation's location on the local disk.</returns>
        /// <exception cref="ImplementationNotFoundException">Thrown if the <paramref name="implementation"/> is not cached yet.</exception>
        public string GetImplementationPath(ImplementationBase implementation)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException("implementation");
            #endregion

            return (string.IsNullOrEmpty(implementation.LocalPath) ? Store.GetPath(implementation.ManifestDigest) : implementation.LocalPath);
        }
        #endregion
    }
}
