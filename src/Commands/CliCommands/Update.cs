// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Check for updates to the program and download them if found.
    /// </summary>
    public sealed class Update : Download
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "update";

        /// <inheritdoc/>
        public override string Description => Resources.DescriptionUpdate;
        #endregion

        #region State
        private Selections _oldSelections;

        /// <inheritdoc/>
        public Update([NotNull] ICommandHandler handler)
            : base(handler)
        {
            //Options.Remove("o|offline");
            //Options.Remove("r|refresh");
            //Options.Remove("xml");
            //Options.Remove("show");
        }
        #endregion

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            if (SelectionsDocument) throw new NotSupportedException(Resources.NoSelectionsDocumentUpdate);

            try
            {
                OldSolve();
                Log.Info("Running Refresh Solve to find updates");
                RefreshSolve();
            }
            catch (SolverException ex) when (Handler.Background)
            {
                Log.Info("Suppressed Solver-related error message due to background mode");
                Log.Info(ex);
                return ExitCode.SolverError;
            }

            DownloadUncachedImplementations();
            SelfUpdateCheck();

            return ShowOutput();
        }

        /// <summary>
        /// Run solver with refresh forced off to get the old values
        /// </summary>
        private void OldSolve()
        {
            FeedManager.Refresh = false;
            _oldSelections = Solver.Solve(Requirements);
        }

        /// <summary>
        /// Shows a list of changes found by the update process.
        /// </summary>
        protected override ExitCode ShowOutput()
        {
            var diff = SelectionsManager.GetDiff(_oldSelections, Selections).ToList();

            if (diff.Count == 0)
            {
                Handler.OutputLow(Resources.NoUpdatesFound, Resources.NoUpdatesFound);
                return ExitCode.NoChanges;
            }
            else
            {
                Handler.Output(Resources.ChangesFound, diff);
                return ExitCode.OK;
            }
        }
    }
}
