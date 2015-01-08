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
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Services.Properties;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Services.Injector
{
    /// <summary>
    /// Executes a set of <see cref="Selections"/> as a program using dependency injection.
    /// </summary>
    public partial class Executor : IExecutor
    {
        #region Dependencies
        /// <summary>
        /// Used to locate the selected <see cref="Store.Model.Implementation"/>s.
        /// </summary>
        private readonly IStore _store;

        /// <summary>
        /// Creates a new executor.
        /// </summary>
        /// <param name="store">Used to locate the selected <see cref="Store.Model.Implementation"/>s.</param>
        public Executor([NotNull] IStore store)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            _store = store;
        }
        #endregion

        /// <inheritdoc/>
        public Selections Selections { get; private set; }

        /// <inheritdoc/>
        public string Main { get; set; }

        /// <inheritdoc/>
        public string Wrapper { get; set; }

        /// <inheritdoc/>
        public Process Start(Selections selections, params string[] arguments)
        {
            #region Sanity checks
            if (arguments == null) throw new ArgumentNullException("arguments");
            #endregion

            try
            {
                return Process.Start(GetStartInfo(selections, arguments));
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
        /// <param name="selections">The specific <see cref="ImplementationSelection"/>s chosen by the solver.</param>
        /// <param name="arguments">Arguments to be passed to the launched programs.</param>
        /// <returns>The <see cref="ProcessStartInfo"/> that can be used to start the new <see cref="Process"/>.</returns>
        /// <exception cref="KeyNotFoundException"><see cref="Selections"/> points to missing <see cref="Dependency"/>s.</exception>
        /// <exception cref="ImplementationNotFoundException">One of the <see cref="Store.Model.Implementation"/>s is not cached yet.</exception>
        /// <exception cref="ExecutorException">The <see cref="IExecutor"/> was unable to process the <see cref="Selections"/>.</exception>
        /// <exception cref="IOException">A problem occurred while writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file is not permitted.</exception>
        /// <exception cref="Win32Exception">A problem occurred while creating a hard link.</exception>
        [NotNull]
        internal ProcessStartInfo GetStartInfo([NotNull] Selections selections, [NotNull, ItemNotNull] params string[] arguments)
        {
            #region Sanity checks
            if (arguments == null) throw new ArgumentNullException("arguments");
            #endregion

            if (selections.Implementations.Count == 0) throw new ArgumentException(Resources.NoImplementationsPassed, "selections");
            Selections = selections;

            var startInfo = BuildStartInfoWithBindings();
            var commandLine = GetCommandLine(GetMainImplementation(), Selections.Command, startInfo);
            PrependWrapper(commandLine);
            AppendUserArgs(arguments, commandLine);
            ProcessRunEnvBindings(startInfo);
            ApplyCommandLine(commandLine, startInfo);
            return startInfo;
        }

        /// <inheritdoc/>
        public string GetImplementationPath(ImplementationSelection implementation)
        {
            #region Sanity checks
            if (implementation == null) throw new ArgumentNullException("implementation");
            #endregion

            if (string.IsNullOrEmpty(implementation.LocalPath))
            {
                string path = _store.GetPath(implementation.ManifestDigest);
                if (path == null) throw new ImplementationNotFoundException(implementation.ManifestDigest);
                return path;
            }
            else return implementation.LocalPath;
        }
    }
}
