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
using ZeroInstall.Launcher.Properties;
using ZeroInstall.Launcher.Solver;

namespace ZeroInstall.Launcher.Commands
{
    /// <summary>
    /// Select a version of the program identified by URI, and compatible versions of all of its dependencies.
    /// </summary>
    [CLSCompliant(false)]
    public class Selection : Cmd
    {
        #region Variables
        /// <summary>Cached <see cref="ISolver"/> results.</summary>
        protected Selections Selections;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return "select"; } }
        
        private readonly Requirements _requirements = new Requirements();
        /// <summary>
        /// A set of requirements/restrictions imposed by the user on the implementation selection process as parsed from the command-line arguments.
        /// </summary>
        public Requirements Requirements { get { return _requirements; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Selection(IHandler handler) : base(handler)
        {
            Options.Add("batch", Resources.OptionBatch, unused => handler.Batch = true);
            Options.Add("r|refresh", Resources.OptionRefresh, unused => Policy.FeedManager.Refresh = true);
            // ToDo: Add --xml
            _requirements.HookUpOptionParsing(Options);

            // Work-around to disable interspersed arguments (needed for passing arguments through to sub-processes)
            Options.Add("<>", value =>
            {
                if (string.IsNullOrEmpty(Requirements.InterfaceID))
                { // First unknown argument
                    // Must not be an option
                    // Note: Windows-style arguments beginning with a slash are interpreted as Unix paths here instead
                    if (value.StartsWith("-")) throw new ArgumentException(Resources.UnknownOption);

                    Requirements.InterfaceID = (File.Exists(value) ? Path.GetFullPath(value) : value);

                    // Stop using options parser, treat everything from here on as unknown
                    Options.Clear();
                }
                else
                { // All other unknown arguments
                    AdditionalArgs.Add(value);
                }
            });
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override void Execute()
        {
            base.Execute();

            if (AdditionalArgs.Count != 0) throw new ArgumentException(Resources.UnknownOption);

            // ToDo: Detect Selections documents

            Selections = SolverProvider.Default.Solve(_requirements, Policy, Handler);
        }
        #endregion
    }
}
