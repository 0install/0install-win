// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
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
    public class Selection : CliCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "select";

        /// <inheritdoc/>
        public override string Description => Resources.DescriptionSelect;

        /// <inheritdoc/>
        public override string Usage => "[OPTIONS] URI";

        /// <inheritdoc/>
        protected override int AdditionalArgsMin => 1;

        /// <inheritdoc/>
        protected override int AdditionalArgsMax => 1;
        #endregion

        #region State
        /// <summary>
        /// A set of requirements/restrictions imposed by the user on the implementation selection process as parsed from the command-line arguments.
        /// </summary>
        [NotNull]
        protected Requirements Requirements { get; } = new Requirements();

        // Intermediate variables, transferred to Requirements after parsing
        private VersionRange _version;
        private ImplementationVersion _before, _notBefore;

        /// <summary>Indicates the user provided a pre-computed <see cref="Selections"/> XML document instead of using the <see cref="ISolver"/>.</summary>
        protected bool SelectionsDocument;

        /// <summary>Indicates the user wants a UI to modify the <see cref="Selections"/>.</summary>
        protected bool CustomizeSelections;

        /// <summary>Indicates the user wants a machine-readable output.</summary>
        protected bool ShowXml;

        /// <inheritdoc/>
        public Selection([NotNull] ICommandHandler handler)
            : base(handler)
        {
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
            Options.Add("os=", () => Resources.OptionOS + Environment.NewLine + SupportedValues<OS>(),
                (OS os) => Requirements.Architecture = new Architecture(os, Requirements.Architecture.Cpu));
            Options.Add("cpu=", () => Resources.OptionCpu + Environment.NewLine + SupportedValues<Cpu>(),
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
                Requirements.ExtraRestrictions[Requirements.InterfaceUri] = new Constraint {NotBefore = _notBefore, Before = _before};
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

        /// <summary>Cached <see cref="ISolver"/> results.</summary>
        protected Selections Selections;

        /// <summary>
        /// Trys to parse <see cref="Store.Model.Requirements.InterfaceUri"/> as a pre-computed <see cref="Selection.Selections"/> document.
        /// </summary>
        /// <seealso cref="SelectionsDocument"/>
        private void TryParseSelectionsDocument()
        {
            try
            { // Try to parse as selections document
                Selections = XmlStorage.LoadXml<Selections>(Requirements.InterfaceUri.LocalPath);
                Selections.Normalize();
                Requirements.InterfaceUri = Selections.InterfaceUri;
                SelectionsDocument = true;
            }
            catch (InvalidDataException)
            { // If that fails assume it is an interface
            }
        }

        /// <summary>
        /// Runs <see cref="ISolver.Solve"/> (unless <see cref="SelectionsDocument"/> is <c>true</c>) and stores the result in <see cref="Selections"/>.
        /// </summary>
        /// <returns>The same result as stored in <see cref="Selections"/>.</returns>
        /// <exception cref="OperationCanceledException">The user canceled the task.</exception>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">An external application or file required by the solver could not be accessed.</exception>
        /// <exception cref="SolverException">The <see cref="ISolver"/> was unable to provide <see cref="Selections"/> that fulfill the <see cref="Requirements"/>.</exception>
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

            try
            {
                Selections.Name = FeedCache.GetFeed(Selections.InterfaceUri).Name;
            }
            #region Error handling
            catch (KeyNotFoundException)
            {
                // Fall back to using feed file name
                Selections.Name = Selections.InterfaceUri.ToString().GetRightPartAtLastOccurrence('/');
            }
            #endregion

            Handler.CancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Run <see cref="Solve"/> with <see cref="IFeedManager.Refresh"/> set to <c>true</c>.
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
            Handler.ShowSelections(Selections, FeedManager);
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

            Handler.ShowSelections(Selections, FeedManager);
            return Selections;
        }

        protected virtual ExitCode ShowOutput()
        {
            if (ShowXml) Handler.Output(Resources.SelectedImplementations, Selections.ToXmlString());
            else Handler.Output(Resources.SelectedImplementations, SelectionsManager.GetTree(Selections));
            return ExitCode.OK;
        }
    }
}
