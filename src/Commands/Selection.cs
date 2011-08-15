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
using Common;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Select a version of the program identified by URI, and compatible versions of all of its dependencies.
    /// </summary>
    [CLSCompliant(false)]
    public class Selection : CommandBase
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "select";
        #endregion

        #region Variables
        /// <summary>Cached <see cref="ISolver"/> results.</summary>
        protected Selections Selections;

        /// <summary>Indicates the user provided a pre-created <see cref="Selections"/> XML document instead of using the <see cref="Policy.Solver"/>.</summary>
        protected bool SelectionsDocument;

        /// <summary>Indicates the user wants a UI to audit the selections.</summary>
        protected bool ShowSelectionsUI;

        /// <summary>Indicates the user wants a machine-readable output.</summary>
        protected bool ShowXml;

        /// <summary>Indicates that one or more of the <see cref="Model.Feed"/>s used by the <see cref="Policy.Solver"/> should be updated.</summary>
        protected bool StaleFeeds;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionSelect; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] URI"; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionSelection; } }

        private readonly Requirements _requirements = new Requirements() /*{Architecture = Architecture.CurrentSystem}*/;
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
        public Selection(Policy policy) : base(policy)
        {
            Options.Add("batch", Resources.OptionBatch, unused => Policy.Handler.Batch = true);
            Options.Add("g|gui", Resources.OptionGui, unused => Policy.FeedManager.Refresh = ShowSelectionsUI = true);

            Options.Add("o|offline", Resources.OptionOffline, unused => Policy.Config.NetworkUse = NetworkLevel.Offline);
            Options.Add("r|refresh", Resources.OptionRefresh, unused => Policy.FeedManager.Refresh = true);

            Options.Add("with-store=", Resources.OptionWithStore, delegate(string path)
            {
                if (string.IsNullOrEmpty(path)) throw new OptionException(string.Format(Resources.MissingOptionValue, "--with-store"), "with-store");
                Policy.Fetcher.Store = new CompositeStore(new DirectoryStore(path), Policy.Fetcher.Store);
            });

            Options.Add("command=", Resources.OptionCommand, command => _requirements.CommandName = command);
            Options.Add("before=", Resources.OptionBefore, delegate(string version)
            {
                if (string.IsNullOrEmpty(version)) throw new OptionException(string.Format(Resources.MissingOptionValue, "--before"), "before");
                try { _requirements.BeforeVersion = new ImplementationVersion(version); }
                #region Error handling
                catch (ArgumentException ex)
                {
                    throw new OptionException(ex.Message, "before", ex);
                }
                #endregion
            });
            Options.Add("not-before=", Resources.OptionNotBefore, delegate(string version)
            {
                if (string.IsNullOrEmpty(version)) throw new OptionException(string.Format(Resources.MissingOptionValue, "--not-before"), "not-before");
                try { _requirements.NotBeforeVersion = new ImplementationVersion(version); }
                #region Error handling
                catch (ArgumentException ex)
                {
                    throw new OptionException(ex.Message, "not-before", ex);
                }
                #endregion
            });
            Options.Add("s|source", Resources.OptionSource, unused => _requirements.Architecture = new Architecture(_requirements.Architecture.OS, Cpu.Source));
            Options.Add("os=", Resources.OptionOS + "\n" + string.Format(Resources.SupportedValues, StringUtils.Concatenate(Architecture.KnownOSStrings, ", ")), delegate(string os)
            {
                if (string.IsNullOrEmpty(os)) throw new OptionException(string.Format(Resources.MissingOptionValue, "--os"), "os");
                try { _requirements.Architecture = new Architecture(Architecture.ParseOS(os), _requirements.Architecture.Cpu); }
                #region Error handling
                catch (ArgumentException ex)
                {
                    throw new OptionException(ex.Message, "os", ex);
                }
                #endregion
            });
            Options.Add("cpu=", Resources.OptionCpu + "\n" + string.Format(Resources.SupportedValues, StringUtils.Concatenate(Architecture.KnownCpuStrings, ", ")), delegate(string cpu)
            {
                if (string.IsNullOrEmpty(cpu)) throw new OptionException(string.Format(Resources.MissingOptionValue, "--cpu"), "cpu");
                try { _requirements.Architecture = new Architecture(_requirements.Architecture.OS, Architecture.ParseCpu(cpu)); }
                #region Error handling
                catch (ArgumentException ex)
                {
                    throw new OptionException(ex.Message, "cpu", ex);
                }
                #endregion
            });

            Options.Add("xml", Resources.OptionXml, unused => ShowXml = true);
        }
        #endregion

        //--------------------//

        #region Parse
        /// <inheritdoc/>
        public override void Parse(IEnumerable<string> args)
        {
            base.Parse(args);

            if (AdditionalArgs.Count == 0 || string.IsNullOrEmpty(AdditionalArgs.First))
                throw new InvalidInterfaceIDException(Resources.NoInterfaceSpecified);

            // The first argument is the interface ID
            Requirements.InterfaceID = GetCanonicalID(AdditionalArgs.First);
            AdditionalArgs.RemoveFirst();

            if (File.Exists(Requirements.InterfaceID))
            {
                try
                { // Try to parse as selections document
                    Selections = Selections.Load(Requirements.InterfaceID);
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
            #region Sanity checks
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count != 0) throw new OptionException(Resources.TooManyArguments + "\n" + AdditionalArgs, "");
            #endregion

            Policy.Handler.ShowProgressUI(Cancel);
            Solve();
            SelectionsUI();

            // If any of the feeds are getting old rerun solver in refresh mode
            if (StaleFeeds && Policy.Config.NetworkUse != NetworkLevel.Offline)
            {
                Policy.FeedManager.Refresh = true;
                Solve();
            }

            Policy.Handler.Output(Resources.SelectedImplementations, GetSelectionsOutput());
            return 0;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Runs <see cref="ISolver.Solve"/> (unless <see cref="SelectionsDocument"/> is <see langword="true"/>) and stores the result in <see cref="Selections"/>.
        /// </summary>
        /// <returns>The same result as stored in <see cref="Selections"/>.</returns>
        /// <exception cref="UserCancelException">Thrown if the user canceled the process.</exception>
        /// <exception cref="IOException">Thrown if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="SolverException">Thrown if the dependencies could not be solved.</exception>
        protected virtual Selections Solve()
        {
            // Don't run the solver if the user provided an external selections document
            if (SelectionsDocument) return Selections;

            try
            {
                Selections = Policy.Solver.Solve(Requirements, Policy, out StaleFeeds);
            }
            catch
            {
                // Suppress any left-over errors if the user canceled anyway
                if (Canceled) throw new UserCancelException();
                throw;
            }

            if (Canceled) throw new UserCancelException();
            return Selections;
        }

        /// <summary>
        /// Allows the user to modify <see cref="Policy"/> and <see cref="Selections"/>.
        /// </summary>
        protected void SelectionsUI()
        {
            Policy.Handler.ShowSelections(Selections, Policy.FeedManager.Cache);

            // Allow the user to trigger a Solver rerun after modifying preferences
            if (ShowSelectionsUI && !SelectionsDocument)
                Policy.Handler.AuditSelections(Solve);

            if (Canceled) throw new UserCancelException();
        }

        /// <summary>
        /// Returns the content of <see cref="Selections"/> formated as the user requested it.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected string GetSelectionsOutput()
        {
            return ShowXml ? Selections.WriteToString() : Selections.GetHumanReadable(Policy.Fetcher.Store);
        }
        #endregion
    }
}
