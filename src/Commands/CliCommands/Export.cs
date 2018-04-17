// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common.Collections;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Commands.Utils;
using ZeroInstall.Services;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Exports all feeds and implementations required to launch the program specified by URI.
    /// </summary>
    public sealed class Export : Download
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "export";

        /// <inheritdoc/>
        public override string Description => Resources.DescriptionExport;

        /// <inheritdoc/>
        public override string Usage => "[OPTIONS] URI DIRECTORY";

        /// <inheritdoc/>
        protected override int AdditionalArgsMin => 2;

        /// <inheritdoc/>
        protected override int AdditionalArgsMax => 2;
        #endregion

        #region State
        private bool _noImplementations;
        private bool _includeZeroInstall;
        private BootstrapMode _bootstrapType = BootstrapMode.Run;

        /// <inheritdoc/>
        public Export([NotNull] ICommandHandler handler)
            : base(handler)
        {
            //Options.Remove("xml");
            //Options.Remove("show");

            Options.Add("no-implementations", () => Resources.OptionExportNoImplementations, _ => _noImplementations = true);
            Options.Add("include-zero-install", () => Resources.OptionExportIncludeZeroInstall, _ => _includeZeroInstall = true);
            Options.Add("bootstrap=", () => Resources.OptionExportBootstrap + Environment.NewLine + SupportedValues<BootstrapMode>(), (BootstrapMode x) => _bootstrapType = x);
        }
        #endregion

        private string _outputPath;

        public override void Parse(IEnumerable<string> args)
        {
            base.Parse(args);

            try
            {
                _outputPath = Path.GetFullPath(AdditionalArgs[0]);
            }
            #region Error handling
            catch (ArgumentException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new UriFormatException(ex.Message);
            }
            catch (NotSupportedException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new UriFormatException(ex.Message);
            }
            #endregion
        }

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            Solve();

            var exporter = new Exporter(Selections, Requirements, _outputPath);

            exporter.ExportFeeds(FeedCache, OpenPgp);
            if (!_noImplementations)
            {
                DownloadUncachedImplementations();
                exporter.ExportImplementations(Store, Handler);
            }

            exporter.DeployImportScript();
            switch (_bootstrapType)
            {
                case BootstrapMode.Run:
                    exporter.DeployBootstrapRun(Handler);
                    break;
                case BootstrapMode.Integrate:
                    exporter.DeployBootstrapIntegrate(Handler);
                    break;
            }

            SelfUpdateCheck();

            return ShowOutput();
        }

        protected override void Solve()
        {
            base.Solve();

            if (_includeZeroInstall)
            {
                try
                {
                    var selfSelections = Solver.Solve(new Requirements(Config.SelfUpdateUri));

                    Selections.Implementations.AddRange(selfSelections.Implementations);
                    UncachedImplementations.AddRange(SelectionsManager.GetUncachedImplementations(selfSelections));
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
        }

        protected override ExitCode ShowOutput()
        {
            Handler.OutputLow(
                title: Resources.ExportComplete,
                message: string.Format(Resources.AllComponentsExported, Selections.Name ?? "app", _outputPath));

            return ExitCode.OK;
        }
    }
}
