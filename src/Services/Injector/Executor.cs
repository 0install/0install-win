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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Common;
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;
using ZeroInstall.Properties;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector
{
    /// <summary>
    /// Executes a set of <see cref="Selections"/> as a program using dependency injection.
    /// </summary>
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
        ///   The specific <see cref="ImplementationSelection"/>s chosen for the <see cref="Dependency"/>s.
        ///   This object must _not_ be modified once it has been passed into this constructor!
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

            try
            {
                return Process.Start(GetStartInfo(arguments));
            }
                #region Error handling
            catch (Win32Exception ex)
            {
                const int requestedOperationRequiresElevation = 740;
                if (ex.NativeErrorCode == requestedOperationRequiresElevation) throw new NotAdminException(ex.Message);
                else throw;
            }
            #endregion
        }

        /// <summary>
        /// Prepares a <see cref="ProcessStartInfo"/> for executing the program as specified by the <see cref="Selections"/>.
        /// </summary>
        /// <param name="arguments">Arguments to be passed to the launched programs.</param>
        /// <returns>The <see cref="ProcessStartInfo"/> that can be used to start the new <see cref="Process"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if <see cref="Selections"/> points to missing <see cref="Dependency"/>s.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="Model.Implementation"/>s is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if there was a problem locating the implementation executable.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to a file is not permitted.</exception>
        /// <exception cref="Win32Exception">Thrown if a problem occurred while creating a hard link.</exception>
        internal ProcessStartInfo GetStartInfo(params string[] arguments)
        {
            #region Sanity checks
            if (arguments == null) throw new ArgumentNullException("arguments");
            #endregion

            var startInfo = BuildStartInfoWithBindings();
            var commandLine = GetCommandLine(GetMainImplementation(), Selections.Command, startInfo);
            PrependWrapper(commandLine);
            AppendUserArgs(arguments, commandLine);
            ProcessRunEnvBindings(startInfo);
            ApplyCommandLine(commandLine, startInfo);
            return startInfo;
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

            if (string.IsNullOrEmpty(implementation.LocalPath))
            {
                string path = Store.GetPath(implementation.ManifestDigest);
                if (path == null) throw new ImplementationNotFoundException(implementation.ManifestDigest);
                return path;
            }
            else return implementation.LocalPath;
        }
        #endregion
    }
}
