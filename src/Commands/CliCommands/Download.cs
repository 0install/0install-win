// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Collections.Generic;
using System.IO;
using System.Net;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// This behaves similarly to <see cref="Selection"/>, except that it also downloads the selected versions if they are not already cached.
    /// </summary>
    public class Download : Selection
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "download";

        /// <inheritdoc/>
        public override string Description => Resources.DescriptionDownload;
        #endregion

        #region State
        /// <summary>Indicates the user wants the implementation locations on the disk.</summary>
        private bool _show;

        /// <summary><see cref="Implementation"/>s referenced in <see cref="Selection.Selections"/> that are not available in the <see cref="IStore"/>.</summary>
        protected ICollection<Implementation> UncachedImplementations;

        /// <inheritdoc/>
        public Download([NotNull] ICommandHandler handler)
            : base(handler)
        {
            Options.Add("show", () => Resources.OptionShow, _ => _show = true);
        }
        #endregion

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            try
            {
                Solve();
                if (FeedManager.ShouldRefresh)
                {
                    Log.Info("Running Refresh Solve because feeds have become stale");
                    RefreshSolve();
                }

                DownloadUncachedImplementations();
            }
            #region Error handling
            catch (WebException ex) when (Handler.Background)
            {
                Log.Info("Suppressed network-related error message due to background mode");
                Log.Info(ex);
                return ExitCode.WebError;
            }
            catch (SolverException ex) when (Handler.Background)
            {
                Log.Info("Suppressed Solver-related error message due to background mode");
                Log.Info(ex);
                return ExitCode.SolverError;
            }
            #endregion

            SelfUpdateCheck();

            Handler.CancellationToken.ThrowIfCancellationRequested();
            return ShowOutput();
        }

        /// <inheritdoc/>
        protected override void Solve()
        {
            base.Solve();

            try
            {
                UncachedImplementations = SelectionsManager.GetUncachedImplementations(Selections);
            }
            #region Error handling
            catch (InvalidDataException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new SolverException(ex.Message, ex);
            }
            #endregion
        }

        /// <summary>
        /// Downloads any <see cref="Implementation"/>s in <see cref="Selection"/> that are missing from <see cref="IStore"/>.
        /// </summary>
        /// <remarks>Makes sure <see cref="ISolver"/> ran with up-to-date feeds before downloading any implementations.</remarks>
        protected void DownloadUncachedImplementations()
        {
            if (UncachedImplementations.Count != 0 && !FeedManager.Refresh)
            {
                Log.Info("Running Refresh Solve because there are un-cached implementations");
                RefreshSolve();
            }

            if (CustomizeSelections || UncachedImplementations.Count != 0) ShowSelections();

            if (UncachedImplementations.Count != 0)
            {
                try
                {
                    Fetcher.Fetch(UncachedImplementations);
                }
                #region Error handling
                catch
                {
                    // Suppress any left-over errors if the user canceled anyway
                    Handler.CancellationToken.ThrowIfCancellationRequested();
                    throw;
                }
                #endregion
            }
        }

        protected override ExitCode ShowOutput()
        {
            if (_show || ShowXml) return base.ShowOutput();

            Handler.OutputLow(Resources.DownloadComplete, Resources.AllComponentsDownloaded);
            return ExitCode.OK;
        }
    }
}
