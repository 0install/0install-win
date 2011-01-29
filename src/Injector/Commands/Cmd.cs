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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using Common;
using NDesk.Options;
using ZeroInstall.Fetchers;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector.Commands
{
    /// <summary>
    /// Represents a command issued via command-line arguments.
    /// </summary>
    /// <remarks>Specific sub-classes of this class are used to handle a commands like "0install COMMAND-NAME OPTIONS".</remarks>
    [CLSCompliant(false)]
    public abstract class Cmd
    {
        #region Variables
        private bool _parsed;

        /// <summary>The command-line argument parser used to evaluate user input.</summary>
        protected readonly OptionSet Options = new OptionSet();

        /// <summary>Combines configuration and resources used to solve dependencies and download implementations.</summary>
        protected readonly Policy Policy = Policy.CreateDefault();

        /// <summary>A callback object used when the the user needs to be asked questions or is to be informed about progress.</summary>
        protected readonly IHandler Handler;

        /// <summary>The detail level of messages printed to the console. 0 = normal, 1 = verbose, 2 = very verbose</summary>
        protected int Verbosity;

        /// <summary>Feeds to add, terms to search for, etc.</summary>
        protected IList<string> AdditionalArgs;
        #endregion

        #region Properties
        /// <summary>
        /// The name of the command as used in command-line arguments in lower-case.
        /// </summary>
        public abstract string Name { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or is to be informed about progress.</param>
        protected Cmd(IHandler handler)
        {
            Handler = handler;

            Options.Add("with-store=", Resources.OptionWithStore, path => Policy.AdditionalStore = new DirectoryStore(path));
            Policy.Preferences.HookUpOptionParsing(Options);
            Options.Add("v|verbose", Resources.OptionVerbose, unused => Verbosity++);
        }
        #endregion

        //--------------------//

        #region Parse
        /// <summary>
        /// Parses command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to be parsed.</param>
        /// <returns>The options detected by the parsing process.</returns>
        /// <exception cref="ArgumentException">Throw if <paramref name="args"/> contains unknown options.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public void Parse(IEnumerable<string> args)
        {
            _parsed = true;

            AdditionalArgs = Options.Parse(args);
        }
        #endregion

        #region Execute
        /// <summary>
        /// Executes the commands specified by the command-line arguments.
        /// </summary>
        /// <returns>The error code to end the process with.</returns>
        /// <exception cref="UserCancelException">Thrown if a download or IO task was cancelled.</exception>
        /// <exception cref="ArgumentException">Thrown if the number of arguments passed in on the command-line is incorrect.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted or if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to an <see cref="IStore"/> is not permitted.</exception>
        /// <exception cref="SolverException">Thrown if the <see cref="ISolver"/> was unable to solve all dependencies.</exception>
        /// <exception cref="FetcherException">Thrown if an <see cref="Model.Implementation"/> could not be downloaded.</exception>
        /// <exception cref="DigestMismatchException">Thrown if an <see cref="Model.Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        /// <exception cref="ImplementationNotFoundException">Thrown if one of the <see cref="ImplementationBase"/>s is not cached yet.</exception>
        /// <exception cref="CommandException">Thrown if there was a problem locating the implementation executable.</exception>
        /// <exception cref="Win32Exception">Thrown if the main executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the main executable could not be launched.</exception>
        /// <exception cref="InvalidOperationException">Thron if this method is called before calling <see cref="Parse"/>.</exception>
        public virtual void Execute()
        {
            if (!_parsed) throw new InvalidOperationException(Resources.NotParsed);
        }
        #endregion
    }
}
