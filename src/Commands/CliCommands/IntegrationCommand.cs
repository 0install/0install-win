// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Common base class for commands that manage <see cref="DesktopIntegration"/>.
    /// </summary>
    public abstract class IntegrationCommand : CliCommand
    {
        #region State
        /// <summary>Do not download the application itself yet.</summary>
        protected bool NoDownload;

        /// <summary>Apply the operation machine-wide instead of just for the current user.</summary>
        protected bool MachineWide { get; private set; }

        /// <inheritdoc/>
        protected IntegrationCommand([NotNull] ICommandHandler handler)
            : base(handler)
        {
            Options.Add("o|offline", () => Resources.OptionOffline, _ => Config.NetworkUse = NetworkLevel.Offline);
            Options.Add("r|refresh", () => Resources.OptionRefresh, _ => FeedManager.Refresh = true);

            Options.Add("m|machine", () => Resources.OptionMachine, _ => MachineWide = true);
        }
        #endregion

        /// <inheritdoc/>
        public override void Parse(IEnumerable<string> args)
        {
            base.Parse(args);

            if (MachineWide && !WindowsUtils.IsAdministrator) throw new NotAdminException(Resources.MustBeAdminForMachineWide);
        }

        #region Helpers
        /// <summary>
        /// Checks the current <see cref="Locations.InstallBase"/> to determine whether it is suitable for operations that persist it.
        /// </summary>
        /// <remarks>
        /// This should be called before performing any operations that persist <see cref="Locations.InstallBase"/> somewhere, e.g. in generated shortcuts or stubs.
        /// It is not required for operations that only remove things from the system.
        /// </remarks>
        /// <exception cref="UnsuitableInstallBaseException">The current Zero Install instance is installed in a location unsuitable for the desired operation.</exception>
        protected void CheckInstallBase()
        {
            if (Locations.IsPortable)
            {
                // NOTE: Portable instances remain decoupled from local instances, so we do not use UnsuitableInstallBaseException here, which would redirect commands to other instances.
                if (Handler.Ask(Resources.AskDeployZeroInstall + Environment.NewLine + Resources.NoIntegrationFromPortable,
                    defaultAnswer: false, alternateMessage: Resources.NoIntegrationFromPortable))
                {
                    var deployArgs = new[] {MaintenanceMan.Name, MaintenanceMan.Deploy.Name, "--restart-central"};
                    if (MachineWide) deployArgs = deployArgs.Append("--machine");
                    ProgramUtils.Run("0install", deployArgs, Handler);
                }
                throw new OperationCanceledException();
            }

            if (ProgramUtils.IsRunningFromCache) throw new UnsuitableInstallBaseException(Resources.NoIntegrationFromCache, MachineWide);
            if (MachineWide && ProgramUtils.IsRunningFromPerUserDir) throw new UnsuitableInstallBaseException(Resources.NoMachineWideIntegrationFromPerUser, MachineWide);
        }

        /// <summary>
        /// Finds an existing <see cref="AppEntry"/> or creates a new one for a specific interface URI.
        /// </summary>
        /// <param name="integrationManager">Manages desktop integration operations.</param>
        /// <param name="interfaceUri">The interface URI to create an <see cref="AppEntry"/> for. Will be updated if <see cref="Feed.ReplacedBy"/> is set and accepted by the user.</param>
        protected virtual AppEntry GetAppEntry([NotNull] IIntegrationManager integrationManager, [NotNull] ref FeedUri interfaceUri)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException(nameof(integrationManager));
            if (interfaceUri == null) throw new ArgumentNullException(nameof(interfaceUri));
            #endregion

            try
            {
                // Try to find an existing AppEntry
                return integrationManager.AppList[interfaceUri];
            }
            catch (KeyNotFoundException)
            {
                try
                {
                    // Create a new AppEntry
                    return CreateAppEntry(integrationManager, ref interfaceUri);
                }
                catch (InvalidOperationException ex)
                    // Only use exact exception type match
                    when (ex.GetType() == typeof(InvalidOperationException))
                {
                    // Find the existing AppEntry after interface URI replacement
                    return integrationManager.AppList[interfaceUri];
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="AppEntry"/> for a specific interface URI.
        /// </summary>
        /// <param name="integrationManager">Manages desktop integration operations.</param>
        /// <param name="interfaceUri">The interface URI to create an <see cref="AppEntry"/> for. Will be updated if <see cref="Feed.ReplacedBy"/> is set and accepted by the user.</param>
        /// <exception cref="InvalidOperationException">The application is already in the list.</exception>
        /// <exception cref="SolverException">The <see cref="ISolver"/> could not ensure <paramref name="interfaceUri"/> specifies a runnable application.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "False positive caused by by-reference argument")]
        protected AppEntry CreateAppEntry([NotNull] IIntegrationManager integrationManager, [NotNull] ref FeedUri interfaceUri)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException(nameof(integrationManager));
            if (interfaceUri == null) throw new ArgumentNullException(nameof(interfaceUri));
            #endregion

            var target = new FeedTarget(interfaceUri, FeedManager.GetFresh(interfaceUri));
            DetectReplacement(ref target);

            Log.Info("Creating app entry for " + target.Uri.ToStringRfc());
            var appEntry = integrationManager.AddApp(target);
            BackgroundDownload(target.Uri);
            return appEntry;
        }

        /// <summary>
        /// Detects and handles <see cref="Feed.ReplacedBy"/>.
        /// </summary>
        private void DetectReplacement(ref FeedTarget target)
        {
            if (target.Feed.ReplacedBy == null || target.Feed.ReplacedBy.Target == null) return;

            if (Handler.Ask(
                string.Format(Resources.FeedReplacedAsk, target.Feed.Name, target.Uri, target.Feed.ReplacedBy.Target),
                defaultAnswer: false, alternateMessage: string.Format(Resources.FeedReplaced, target.Uri, target.Feed.ReplacedBy.Target)))
            {
                target = new FeedTarget(
                    target.Feed.ReplacedBy.Target,
                    FeedManager.GetFresh(target.Feed.ReplacedBy.Target));
            }
        }

        /// <summary>
        /// Pre-download application in a background proccess for later use.
        /// </summary>
        private void BackgroundDownload([NotNull] FeedUri interfaceUri)
        {
            if (!NoDownload && Config.NetworkUse == NetworkLevel.Full)
            {
                Log.Info("Starting background download for later use");
                StartCommandBackground(Download.Name, "--batch", interfaceUri.ToStringRfc());
            }
        }
        #endregion
    }
}
