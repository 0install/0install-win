/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Globalization;
using System.IO;
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common;
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

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Select a version of the program identified by URI, and compatible versions of all of its dependencies.
    /// </summary>
    [CLSCompliant(false)]
    public class Selection : CliCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "select";

        /// <summary>Cached <see cref="ISolver"/> results.</summary>
        protected Selections Selections;

        /// <summary>Indicates the user provided a pre-computed <see cref="Selections"/> XML document instead of using the <see cref="ISolver"/>.</summary>
        protected bool SelectionsDocument;

        /// <summary>Indicates the user wants a UI to modify the <see cref="Selections"/>.</summary>
        protected bool CustomizeSelections;

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
        #endregion

        #region State
        /// <summary>
        /// A set of requirements/restrictions imposed by the user on the implementation selection process as parsed from the command-line arguments.
        /// </summary>
        protected Requirements Requirements { get; private set; }

        // Intermediate variables, transferred to Requirements after parsing
        private VersionRange _version;
        private ImplementationVersion _before, _notBefore;

        /// <inheritdoc/>
        public Selection([NotNull] ICommandHandler handler) : base(handler)
        {
            Requirements = new Requirements();
            Options.Add("customize", () => Resources.OptionCustomize, _ => CustomizeSelections = true);

            Options.Add("o|offline", () => Resources.OptionOffline, _ => Config.NetworkUse = NetworkLevel.Offline);
            Options.Add("r|refresh", () => Resources.OptionRefresh, _ => FeedManager.Refresh = true);

            Options.Add("with-store=", () => Resources.OptionWithStore, delegate(string path)
            {
                if (string.IsNullOrEmpty(path)) throw new OptionException(string.Format(Resources.MissingOptionValue, "--with-store"), "with-store");
                Store = new CompositeStore(new[] {new DirectoryStore(path), Store});
            });

            Options.Add("command=", () => Resources.OptionCommand,
                command => Requirements.Command = command);
            Options.Add("before=", () => Resources.OptionBefore,
                (ImplementationVersion version) => _before = version);
            Options.Add("not-before=", () => Resources.OptionNotBefore,
                (ImplementationVersion version) => _notBefore = version);
            Options.Add("version=", () => Resources.OptionVersionRange,
                (VersionRange range) => _version = range);
            Options.Add("version-for==", () => Resources.OptionVersionRangeFor,
                (FeedUri interfaceUri, VersionRange range) => Requirements.ExtraRestrictions[interfaceUri] = range);
            Options.Add("s|source", () => Resources.OptionSource,
                _ => Requirements.Source = true);
            Options.Add("os=", () => Resources.OptionOS + "\n" + SupportedValues(Architecture.KnownOS),
                (OS os) => Requirements.Architecture = new Architecture(os, Requirements.Architecture.Cpu));
            Options.Add("cpu=", () => Resources.OptionCpu + "\n" + SupportedValues(Architecture.KnownCpu),
                (Cpu cpu) => Requirements.Architecture = new Architecture(Requirements.Architecture.OS, cpu));
            Options.Add("language=", () => Resources.OptionLanguage,
                (CultureInfo lang) => Requirements.Languages.Add(lang));

            Options.Add("xml", () => Resources.OptionXml, _ => ShowXml = true);
        }
        #endregion

        /// <inheritdoc/>
        public override void Parse(IEnumerable<string> args)
        {
            base.Parse(args);

            SetInterfaceUri(GetCanonicalUri(AdditionalArgs[0]));
            AdditionalArgs.RemoveAt(0);

            if (Requirements.InterfaceUri.IsFile && File.Exists(Requirements.InterfaceUri.LocalPath))
                TryParseSelectionsDocument();
        }

        /// <summary>
        /// Sets <see cref="Store.Model.Requirements.InterfaceUri"/> and applies <see cref="Requirements"/> options that need to be deferred to the end of the parsing process.
        /// </summary>
        protected void SetInterfaceUri(FeedUri uri)
        {
            Requirements.InterfaceUri = uri;

            if (_version != null)
                Requirements.ExtraRestrictions[Requirements.InterfaceUri] = _version;
            else if (_notBefore != null || _before != null)
                Requirements.ExtraRestrictions[Requirements.InterfaceUri] = new VersionRange(_notBefore, _before);
        }

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            Solve();
            if (FeedManager.ShouldRefresh)
            {
                Log.Info("Running Refresh Solve because feeds have become stale");
                RefreshSolve();
            }
            SelfUpdateCheck();

            ShowSelections();

            Handler.CancellationToken.ThrowIfCancellationRequested();
            return ShowOutput();
        }

        #region Helpers
        /// <summary>
        /// Trys to parse <see cref="Store.Model.Requirements.InterfaceUri"/> as a pre-computed <see cref="Store.Model.Selection.Selections"/> document.
        /// </summary>
        /// <seealso cref="SelectionsDocument"/>
        private void TryParseSelectionsDocument()
        {
            try
            { // Try to parse as selections document
                Selections = XmlStorage.LoadXml<Selections>(Requirements.InterfaceUri.LocalPath);
                Requirements.InterfaceUri = Selections.InterfaceUri;
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
        /// <exception cref="OperationCanceledException">The user canceled the process.</exception>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">An external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="SolverException">The <see cref="ISolver"/> was unable to provide a set of <see cref="Selections"/> that fulfill the <see cref="Requirements"/>.</exception>
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
            FeedManager.Stale = false;
            FeedManager.Refresh = true;
            Solve();
        }

        /// <summary>
        /// Displays the <see cref="Selections"/> to the user.
        /// </summary>
        protected void ShowSelections()
        {
            Handler.ShowSelections(Selections, FeedCache);
            if (CustomizeSelections && !SelectionsDocument) Handler.CustomizeSelections(SolveCallback);
            Handler.CancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Run <see cref="Solve"/> and inform the caller as well as the <see cref="ICommandHandler"/> of any changes.
        /// </summary>
        private Selections SolveCallback()
        {
            // Temporarily change configuration to make additional Solver calls as non-intrusive as possible
            bool backupRefresh = FeedManager.Refresh;
            FeedManager.Refresh = false;

            try
            {
                Solve();
            }
            finally
            {
                // Restore original configuration
                FeedManager.Refresh = backupRefresh;
            }

            Handler.ShowSelections(Selections, FeedCache);
            return Selections;
        }

        private ExitCode ShowOutput()
        {
            Handler.Output(Resources.SelectedImplementations, GetSelectionsOutput());
            return ExitCode.OK;
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
