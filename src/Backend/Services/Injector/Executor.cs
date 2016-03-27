/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Diagnostics;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Store.Implementations;
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
            if (selections == null) throw new ArgumentNullException("selections");
            if (arguments == null) throw new ArgumentNullException("arguments");
            #endregion

            return GetStartInfo(selections, arguments).Start();
        }

        /// <inheritdoc/>
        public ProcessStartInfo GetStartInfo(Selections selections, params string[] arguments)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (arguments == null) throw new ArgumentNullException("arguments");
            #endregion

            if (string.IsNullOrEmpty(selections.Command)) throw new ExecutorException("The Selections document does not specify a start command.");
            if (selections.Implementations.Count == 0) throw new ExecutorException("The Selections document does not list any implementations.");
            Selections = selections;

            try
            {
                var startInfo = BuildStartInfoWithBindings();
                var commandLine = GetCommandLine(GetMainImplementation(), Selections.Command, startInfo);
                PrependWrapper(commandLine);
                AppendUserArgs(arguments, commandLine);
                ProcessRunEnvBindings(startInfo);
                ApplyCommandLine(commandLine, startInfo);
                return startInfo;
            }
                #region Error handling
            catch (KeyNotFoundException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new ExecutorException(ex.Message);
            }
            #endregion
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
