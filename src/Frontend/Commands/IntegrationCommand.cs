using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Common base class for commands that manage <see cref="DesktopIntegration"/>.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class IntegrationCommand : FrontendCommand
    {
        #region Metadata
        /// <inheritdoc/>
        protected IntegrationCommand([NotNull] ICommandHandler handler) : base(handler)
        {
            Options.Add("o|offline", () => Resources.OptionOffline, _ => Config.NetworkUse = NetworkLevel.Offline);
            Options.Add("r|refresh", () => Resources.OptionRefresh, _ => FeedManager.Refresh = true);

            Options.Add("no-fetch", () => Resources.OptionNoFetch, _ => _noFetch = true);

            Options.Add("m|machine", () => Resources.OptionMachine, _ => MachineWide = true);
        }
        #endregion

        #region State
        /// <summary>Do not download the application itself yet.</summary>
        private bool _noFetch;

        /// <summary>Apply the operation machine-wide instead of just for the current user.</summary>
        protected bool MachineWide;
        #endregion

        /// <inheritdoc/>
        public override void Parse(IEnumerable<string> args)
        {
            base.Parse(args);

            if (MachineWide && !WindowsUtils.IsAdministrator) throw new NotAdminException(Resources.MustBeAdminForMachineWide);
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
        protected AppEntry CreateAppEntry([NotNull] IIntegrationManager integrationManager, [NotNull] ref FeedUri interfaceUri)
        {
            #region Sanity checks
            if (interfaceUri == null) throw new ArgumentNullException("interfaceUri");
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            #endregion

            var feed = FeedManager.GetFeedFresh(interfaceUri);
            DetectReplacement(ref interfaceUri, ref feed);
            //TryToSolve(interfaceUri);

            var appEntry = integrationManager.AddApp(interfaceUri, feed);
            PreDownload(interfaceUri);
            return appEntry;
        }

        /// <summary>
        /// Detects and handles <see cref="Feed.ReplacedBy"/>.
        /// </summary>
        private void DetectReplacement(ref FeedUri interfaceUri, ref Feed feed)
        {
            if (feed.ReplacedBy == null || feed.ReplacedBy.Target == null) return;

            if (Handler.AskQuestion(
                string.Format(Resources.FeedReplacedAsk, feed.Name, interfaceUri, feed.ReplacedBy.Target),
                batchInformation: string.Format(Resources.FeedReplaced, interfaceUri, feed.ReplacedBy.Target)))
            {
                interfaceUri = feed.ReplacedBy.Target;
                feed = FeedManager.GetFeedFresh(interfaceUri);
            }
        }

        /// <summary>
        /// Pre-download application in background for later use.
        /// </summary>
        private void PreDownload([NotNull] FeedUri interfaceUri)
        {
            if (!_noFetch && Config.NetworkUse == NetworkLevel.Full)
            {
                ProcessUtils.LaunchAssembly(
                    /*MonoUtils.IsUnix ? "0install-gtk" :*/ "0install-win",
                    "download --batch " + interfaceUri.OriginalString);
            }
        }
        #endregion
    }
}
