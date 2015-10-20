using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
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
        protected IntegrationCommand([NotNull] ICommandHandler handler) : base(handler)
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

            if (MachineWide)
            {
                if (ProgramUtils.PerUserInstall)
                {
                    string machineWideInstallBase = RegistryUtils.GetString(@"HKEY_LOCAL_MACHINE\SOFTWARE\Zero Install", "InstallLocation");
                    string hint = (machineWideInstallBase != Locations.InstallBase && !string.IsNullOrEmpty(machineWideInstallBase))
                        // Recommend using existing machine-wide instance
                        ? string.Format(Resources.NoPerUserMachineWideUse, new[] {Path.Combine(machineWideInstallBase, "0install.exe"), Name}.Concat(args).JoinEscapeArguments())
                        // Recommend downloading machine-wide installer
                        : Resources.PleaseRunInstaller;
                    throw new NotSupportedException(Resources.NoPerUserMachineWide + Environment.NewLine + hint);
                }

                if (!WindowsUtils.IsAdministrator) throw new NotAdminException(Resources.MustBeAdminForMachineWide);
            }
        }

        #region Helpers
        /// <summary>
        /// Finds an existing <see cref="AppEntry"/> or creates a new one for a specific interface URI.
        /// </summary>
        /// <param name="integrationManager">Manages desktop integration operations.</param>
        /// <param name="interfaceUri">The interface URI to create an <see cref="AppEntry"/> for. Will be updated if <see cref="Feed.ReplacedBy"/> is set and accepted by the user.</param>
        protected virtual AppEntry GetAppEntry([NotNull] IIntegrationManager integrationManager, [NotNull] ref FeedUri interfaceUri)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            if (interfaceUri == null) throw new ArgumentNullException("interfaceUri");
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
                {
                    // Only use exact exception type match
                    if (ex.GetType() != typeof(InvalidOperationException)) throw;

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
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            if (interfaceUri == null) throw new ArgumentNullException("interfaceUri");
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
