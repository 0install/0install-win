/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common.Storage;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Backend;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Select a version of the program identified by URI, and compatible versions of all of its dependencies.
    /// </summary>
    [CLSCompliant(false)]
    public class Selection : FrontendCommand
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "select";
        #endregion

        #region Variables
        /// <summary>Cached <see cref="ISolver"/> results.</summary>
        protected Selections Selections;

        /// <summary>Indicates the user provided a pre-created <see cref="Selections"/> XML document instead of using the <see cref="Resolver.Solver"/>.</summary>
        protected bool SelectionsDocument;

        /// <summary>Indicates the user wants a UI to audit the selections.</summary>
        protected bool ShowSelectionsUI;

        /// <summary>Indicates the user wants a machine-readable output.</summary>
        protected bool ShowXml;

        /// <summary>Indicates that one or more of the <see cref="Model.Feed"/>s used by the <see cref="Resolver.Solver"/> should be updated.</summary>
        protected bool StaleFeeds;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionSelect; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] URI"; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionSelection; } }

        private readonly Requirements _requirements = new Requirements();

        /// <summary>
        /// A set of requirements/restrictions imposed by the user on the implementation selection process as parsed from the command-line arguments.
        /// </summary>
        public Requirements Requirements { get { return _requirements; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Selection(Resolver resolver) : base(resolver)
        {
            Options.Add("batch", Resources.OptionBatch, unused => Resolver.Handler.Batch = true);
            Options.Add("g|gui", Resources.OptionGui, unused => ShowSelectionsUI = true);

            Options.Add("o|offline", Resources.OptionOffline, unused => Resolver.Config.NetworkUse = NetworkLevel.Offline);
            Options.Add("r|refresh", Resources.OptionRefresh, unused => Resolver.FeedManager.Refresh = true);

            Options.Add("with-store=", Resources.OptionWithStore, delegate(string path)
            {
                if (string.IsNullOrEmpty(path)) throw new OptionException(string.Format(Resources.MissingOptionValue, "--with-store"), "with-store");
                Resolver.Store = new CompositeStore(new DirectoryStore(path), Resolver.Store);
            });

            Requirements.FromCommandLine(Options);

            Options.Add("xml", Resources.OptionXml, unused => ShowXml = true);
        }
        #endregion

        //--------------------//

        #region Parse
        /// <inheritdoc/>
        public override void Parse(IEnumerable<string> args)
        {
            base.Parse(args);

            if (AdditionalArgs.Count == 0 || string.IsNullOrEmpty(AdditionalArgs[0]))
                throw new InvalidInterfaceIDException(Resources.NoInterfaceSpecified);

            // The first argument is the interface ID
            Requirements.InterfaceID = GetCanonicalID(AdditionalArgs[0]);
            AdditionalArgs.RemoveAt(0);

            if (File.Exists(Requirements.InterfaceID))
            {
                try
                { // Try to parse as selections document
                    Selections = XmlStorage.LoadXml<Selections>(Requirements.InterfaceID);
                    Requirements.InterfaceID = Selections.InterfaceID;
                    SelectionsDocument = true;
                }
                catch (InvalidDataException)
                { // If that fails assume it is an interface
                }
            }
        }
        #endregion

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count != 0) throw new OptionException(Resources.TooManyArguments + "\n" + AdditionalArgs.JoinEscapeArguments(), "");

            Resolver.Handler.ShowProgressUI();

            Solve();
            SelectionsUI();

            // If any of the feeds are getting old rerun solver in refresh mode
            if (StaleFeeds && Resolver.Config.EffectiveNetworkUse == NetworkLevel.Full)
            {
                Resolver.FeedManager.Refresh = true;
                Solve();
            }

            Resolver.Handler.Output(Resources.SelectedImplementations, GetSelectionsOutput());
            return 0;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Runs <see cref="ISolver.Solve(ZeroInstall.Model.Requirements,out bool)"/> (unless <see cref="SelectionsDocument"/> is <see langword="true"/>) and stores the result in <see cref="Selections"/>.
        /// </summary>
        /// <returns>The same result as stored in <see cref="Selections"/>.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the process.</exception>
        /// <exception cref="IOException">Thrown if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="SolverException">Thrown if the dependencies could not be solved.</exception>
        protected virtual Selections Solve()
        {
            // TODO: Handle named apps

            // Don't run the solver if the user provided an external selections document
            if (SelectionsDocument) return Selections;

            try
            {
                Selections = Resolver.Solver.Solve(Requirements, out StaleFeeds);
            }
                #region Error handling
            catch
            {
                // Suppress any left-over errors if the user canceled anyway
                Resolver.Handler.CancellationToken.ThrowIfCancellationRequested();
                throw;
            }
            #endregion

            Resolver.Handler.CancellationToken.ThrowIfCancellationRequested();
            return Selections;
        }

        /// <summary>
        /// Allows the user to modify <see cref="Selections"/>.
        /// </summary>
        protected void SelectionsUI()
        {
            Resolver.Handler.ShowSelections(Selections, Resolver.FeedCache);

            // Allow the user to trigger a Solver rerun after modifying preferences
            if (ShowSelectionsUI && !SelectionsDocument)
                Resolver.Handler.AuditSelections(Solve);

            Resolver.Handler.CancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Returns the content of <see cref="Selections"/> formated as the user requested it.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected string GetSelectionsOutput()
        {
            return ShowXml ? Selections.ToXmlString() : Selections.GetHumanReadable(Resolver.Store);
        }
        #endregion
    }
}
