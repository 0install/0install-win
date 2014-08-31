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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using NanoByte.Common.Storage;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Select a version of the program identified by URI, and compatible versions of all of its dependencies.
    /// </summary>
    [CLSCompliant(false)]
    public class Selection : FrontendCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "select";

        /// <summary>Cached <see cref="ISolver"/> results.</summary>
        protected Selections Selections;

        /// <summary>Indicates the user provided a pre-computed <see cref="Selections"/> XML document instead of using the <see cref="ISolver"/>.</summary>
        protected bool SelectionsDocument;

        /// <summary>Indicates the user wants a UI to modify the <see cref="Selections"/>.</summary>
        protected bool ShowModifySelections;

        /// <summary>Indicates the user wants a machine-readable output.</summary>
        protected bool ShowXml;

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionSelect; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] URI"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMin { get { return 1; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 1; } }

        /// <inheritdoc/>
        public Selection(ICommandHandler handler) : base(handler)
        {
            Options.Add("batch", () => Resources.OptionBatch, _ => Handler.Batch = true);
            Options.Add("g|gui", () => Resources.OptionGui, _ => ShowModifySelections = true);

            Options.Add("o|offline", () => Resources.OptionOffline, _ => Config.NetworkUse = NetworkLevel.Offline);
            Options.Add("r|refresh", () => Resources.OptionRefresh, _ => FeedManager.Refresh = true);

            Options.Add("with-store=", () => Resources.OptionWithStore, delegate(string path)
            {
                if (string.IsNullOrEmpty(path)) throw new OptionException(string.Format(Resources.MissingOptionValue, "--with-store"), "with-store");
                Store = new CompositeStore(new[] {new DirectoryStore(path), Store});
            });

            Options.Add("command=", () => Resources.OptionCommand,
                command => _requirements.Command = command);
            Options.Add("before=", () => Resources.OptionBefore,
                (ImplementationVersion version) => _before = version);
            Options.Add("not-before=", () => Resources.OptionNotBefore,
                (ImplementationVersion version) => _notBefore = version);
            Options.Add("version=", () => Resources.OptionVersionRange,
                (VersionRange range) => _version = range);
            Options.Add("version-for==", () => Resources.OptionVersionRangeFor,
                (string interfaceID, VersionRange range) => _requirements.ExtraRestrictions[interfaceID] = range);
            Options.Add("s|source", () => Resources.OptionSource,
                _ => _requirements.Architecture = new Architecture(_requirements.Architecture.OS, Cpu.Source));
            Options.Add("os=", () => Resources.OptionOS + "\n" + SupportedValues(Architecture.KnownOS),
                (OS os) => _requirements.Architecture = new Architecture(os, _requirements.Architecture.Cpu));
            Options.Add("cpu=", () => Resources.OptionCpu + "\n" + SupportedValues(Architecture.KnownCpu),
                (Cpu cpu) => _requirements.Architecture = new Architecture(_requirements.Architecture.OS, cpu));

            Options.Add("xml", () => Resources.OptionXml, _ => ShowXml = true);
        }
        #endregion

        #region State
        private readonly Requirements _requirements = new Requirements();

        /// <summary>
        /// A set of requirements/restrictions imposed by the user on the implementation selection process as parsed from the command-line arguments.
        /// </summary>
        public Requirements Requirements { get { return _requirements; } }

        // Intermediate variables, transferred to Requirements after parsing
        private VersionRange _version;
        private ImplementationVersion _before, _notBefore;
        #endregion

        /// <inheritdoc/>
        public override void Parse(IEnumerable<string> args)
        {
            base.Parse(args);

            _requirements.InterfaceID = GetCanonicalID(AdditionalArgs[0]);
            AdditionalArgs.RemoveAt(0);

            if (_version != null) _requirements.ExtraRestrictions[_requirements.InterfaceID] = _version;
            else if (_before != null || _notBefore != null) _requirements.ExtraRestrictions[_requirements.InterfaceID] = new VersionRange(_notBefore, _before);

            if (File.Exists(_requirements.InterfaceID)) TryParseSelectionsDocument();
        }

        /// <inheritdoc/>
        public override int Execute()
        {
            Solve();
            if (FeedManager.Stale) RefreshSolve();

            ShowSelections();

            Handler.CancellationToken.ThrowIfCancellationRequested();
            return ShowOutput();
        }

        #region Helpers
        /// <summary>
        /// Trys to parse <see cref="Store.Model.Requirements.InterfaceID"/> as a pre-computed <see cref="Store.Model.Selection.Selections"/> document.
        /// </summary>
        /// <seealso cref="SelectionsDocument"/>
        private void TryParseSelectionsDocument()
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

        /// <summary>
        /// Runs <see cref="ISolver.Solve"/> (unless <see cref="SelectionsDocument"/> is <see langword="true"/>) and stores the result in <see cref="Selections"/>.
        /// </summary>
        /// <returns>The same result as stored in <see cref="Selections"/>.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the process.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">Thrown if an external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="SolverException">Thrown if the dependencies could not be solved.</exception>
        protected virtual void Solve()
        {
            // TODO: Handle named apps

            // Don't run the solver if the user provided an external selections document
            if (SelectionsDocument) return;

            try
            {
                Selections = Solver.Solve(Requirements);
            }
                #region Error handling
            catch
            {
                // Suppress any left-over errors if the user canceled anyway
                Handler.CancellationToken.ThrowIfCancellationRequested();
                throw;
            }
            #endregion

            Handler.CancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Run <see cref="Solve"/> with <see cref="IFeedManager.Refresh"/> set to <see langword="true"/>.
        /// </summary>
        protected void RefreshSolve()
        {
            // No need if initial solver run already had refresh turned on
            if (FeedManager.Refresh) return;

            FeedManager.Refresh = true;
            Solve();
        }

        /// <summary>
        /// Displays the <see cref="Selections"/> to the user.
        /// </summary>
        protected void ShowSelections()
        {
            Handler.ShowSelections(Selections, FeedCache);
            if (ShowModifySelections && !SelectionsDocument) Handler.ModifySelections(SolveCallback);
            Handler.CancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Run <see cref="Solve"/> and inform the caller as well as the <see cref="ICommandHandler"/> of any changes.
        /// </summary>
        private Selections SolveCallback()
        {
            Solve();
            Handler.ShowSelections(Selections, FeedCache);
            return Selections;
        }

        private int ShowOutput()
        {
            Handler.Output(Resources.SelectedImplementations, GetSelectionsOutput());
            return 0;
        }

        /// <summary>
        /// Returns the content of <see cref="Selections"/> formated as the user requested it.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected string GetSelectionsOutput()
        {
            return ShowXml ? Selections.ToXmlString() : Selections.GetHumanReadable(Store);
        }
        #endregion
    }
}
