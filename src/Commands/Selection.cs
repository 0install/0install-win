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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Select a version of the program identified by URI, and compatible versions of all of its dependencies.
    /// </summary>
    [CLSCompliant(false)]
    public class Selection : CommandBase
    {
        #region Variables
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "select";

        /// <summary>The solver to use get <see cref="Selections"/> based on the <see cref="Requirements"/>.</summary>
        protected readonly ISolver Solver;

        /// <summary>Cached <see cref="ISolver"/> results.</summary>
        protected Selections Selections;

        /// <summary>Indicates the user provided a pre-created <see cref="Selections"/> XML document instead of using the <see cref="Solver"/>.</summary>
        protected bool SelectionsDocument;

        /// <summary>Indicates the user wants a machine-readable output.</summary>
        protected bool ShowXml;

        /// <summary>Indicates that one or more of the <see cref="Model.Feed"/>s used by the <see cref="Solver"/> should be updated.</summary>
        protected bool StaleFeeds;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionSelect; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] URI"; } }

        private readonly Requirements _requirements = new Requirements();
        /// <summary>
        /// A set of requirements/restrictions imposed by the user on the implementation selection process as parsed from the command-line arguments.
        /// </summary>
        public Requirements Requirements { get { return _requirements; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="policy">Combines UI access, preferences and resources used to solve dependencies and download implementations.</param>
        /// <param name="solver">The solver to use get <see cref="Selections"/> based on the <see cref="Requirements"/>.</param>
        public Selection(Policy policy, ISolver solver) : base(policy)
        {
            Solver = solver;

            Options.Add("batch", Resources.OptionBatch, unused => Policy.Handler.Batch = true);
            Options.Add("r|refresh", Resources.OptionRefresh, unused => Policy.FeedManager.Refresh = true);
            
            Options.Add("command=", Resources.OptionCommand, command => _requirements.CommandName = command);
            Options.Add("before=", Resources.OptionBefore, version => _requirements.BeforeVersion = new ImplementationVersion(version));
            Options.Add("not-before=", Resources.OptionNotBefore, version => _requirements.NotBeforeVersion = new ImplementationVersion(version));
            Options.Add("s|source", Resources.OptionSource, unused => _requirements.Architecture = new Architecture(_requirements.Architecture.OS, Cpu.Source));
            Options.Add("os=", Resources.OptionOS, os => _requirements.Architecture = new Architecture(Architecture.ParseOS(os), _requirements.Architecture.Cpu));
            Options.Add("cpu=", Resources.OptionCpu, cpu => _requirements.Architecture = new Architecture(_requirements.Architecture.OS, Architecture.ParseCpu(cpu)));

            Options.Add("xml", Resources.OptionXml, unused => ShowXml = true);
        }
        #endregion

        //--------------------//

        #region Helpers
        /// <summary>
        /// Runs <see cref="ISolver.Solve"/> (unless <see cref="SelectionsDocument"/> is <see langword="true"/>) and stores the result in <see cref="Selections"/>.
        /// </summary>
        protected void Solve()
        {
            // Run the solver unless the user provided a selections document
            if (!SelectionsDocument) Selections = Solver.Solve(Requirements, Policy, out StaleFeeds);
        }

        /// <summary>
        /// Returns the content of <see cref="Selections"/> formated as the user requested it.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected string GetSelectionsOutput()
        {
            return ShowXml ? Selections.WriteToString() : Selections.GetHumanReadable(Policy.SearchStore);
        }
        #endregion

        #region Parse
        /// <inheritdoc/>
        public override void Parse(IEnumerable<string> args)
        {
            base.Parse(args);

            if (AdditionalArgs.Count == 0) throw new InvalidInterfaceIDException(Resources.NoInterfaceSpecified);

            // The first argument is the interface ID
            var feedID = AdditionalArgs.First;
            AdditionalArgs.RemoveFirst();

            if (feedID.StartsWith("alias:"))
            {
                // ToDo: Handle alias lookup
            }
            else if (File.Exists(feedID))
            {
                try
                { // Try to parse as selections document
                    Selections = Selections.Load(feedID);
                    Requirements.InterfaceID = Selections.InterfaceID;
                    SelectionsDocument = true;
                }
                catch (InvalidOperationException)
                { // If that fails assume it is an interface
                    Requirements.InterfaceID = Path.GetFullPath(feedID);
                    SelectionsDocument = false;
                }
            }
            else
            { // Assume a normal URI
                Requirements.InterfaceID = feedID;
            }
        }
        #endregion

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            #region Sanity checks
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count != 0) throw new OptionException(Resources.TooManyArguments + "\n" + AdditionalArgs, "");
            #endregion

            Solve();

            // If any of the feeds are getting old rerun solver in refresh mode
            if (StaleFeeds)
            {
                Policy.FeedManager.Refresh = true;
                Solve();
            }

            Policy.Handler.Output(Resources.SelectedImplementations, GetSelectionsOutput());
            return 0;
        }
        #endregion
    }
}
