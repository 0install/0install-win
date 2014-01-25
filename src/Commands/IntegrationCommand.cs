using System;
using System.Collections.Generic;
using Common;
using Common.Utils;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Feeds;
using ZeroInstall.Model;
using ZeroInstall.Solvers;
using ZeroInstall.Store;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Common base class for commands that manage <see cref="ZeroInstall.DesktopIntegration"/>.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class IntegrationCommand : FrontendCommand
    {
        #region Metadata
        /// <inheritdoc/>
        protected IntegrationCommand(IBackendHandler handler) : base(handler)
        {
            Options.Add("batch", () => Resources.OptionBatch, _ => Handler.Batch = true);

            Options.Add("o|offline", () => Resources.OptionOffline, _ => Config.NetworkUse = NetworkLevel.Offline);
            Options.Add("r|refresh", () => Resources.OptionRefresh, _ => FeedManager.Refresh = true);

            Options.Add("m|machine", () => Resources.OptionMachine, _ => MachineWide = true);
        }
        #endregion

        #region State
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
        /// Finds an existing <see cref="AppEntry"/> or creates a new one for a specific interface ID.
        /// </summary>
        /// <param name="integrationManager">Manages desktop integration operations.</param>
        /// <param name="interfaceID">The interface ID to create an <see cref="AppEntry"/> for. Will be updated if <see cref="Feed.ReplacedBy"/> is set and accepted by the user.</param>
        protected virtual AppEntry GetAppEntry(IIntegrationManager integrationManager, ref string interfaceID)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            try
            {
                // Try to find an existing AppEntry
                return integrationManager.AppList[interfaceID];
            }
            catch (KeyNotFoundException)
            {
                try
                {
                    // Create a new AppEntry
                    return CreateAppEntry(integrationManager, ref interfaceID);
                }
                catch (InvalidOperationException ex)
                {
                    // Only use exact exception type match
                    if (ex.GetType() != typeof(InvalidOperationException)) throw;

                    // Find the existing AppEntry after interface ID replacement
                    return integrationManager.AppList[interfaceID];
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="AppEntry"/> for a specific interface ID.
        /// </summary>
        /// <param name="integrationManager">Manages desktop integration operations.</param>
        /// <param name="interfaceID">The interface ID to create an <see cref="AppEntry"/> for. Will be updated if <see cref="Feed.ReplacedBy"/> is set and accepted by the user.</param>
        /// <exception cref="InvalidOperationException">Thrown if the application is already in the list.</exception>
        /// <exception cref="SolverException">Thrown if the <see cref="ISolver"/> could not ensure <paramref name="interfaceID"/> specifies a runnable application.</exception>
        protected AppEntry CreateAppEntry(IIntegrationManager integrationManager, ref string interfaceID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            #endregion

            var feed = FeedManager.GetFeedFresh(interfaceID);
            DetectReplacement(ref interfaceID, ref feed);
            //TryToSolve(interfaceID);

            var appEntry = integrationManager.AddApp(interfaceID, feed);
            PreDownload(interfaceID);
            return appEntry;
        }

        /// <summary>
        /// Detects and handles <see cref="Feed.ReplacedBy"/>.
        /// </summary>
        private void DetectReplacement(ref string interfaceID, ref Feed feed)
        {
            if (feed.ReplacedBy == null) return;

            if (Handler.AskQuestion(
                string.Format(Resources.FeedReplacedAsk, feed.Name, interfaceID, feed.ReplacedBy.Target),
                string.Format(Resources.FeedReplaced, interfaceID, feed.ReplacedBy.Target)))
            {
                interfaceID = feed.ReplacedBy.Target.ToString();
                feed = FeedManager.GetFeedFresh(interfaceID);
            }
        }

        /// <summary>
        /// Pre-download application in background for later use.
        /// </summary>
        private void PreDownload(string interfaceID)
        {
            if (Config.NetworkUse == NetworkLevel.Full)
            {
                ProcessUtils.LaunchAssembly(
                    /*MonoUtils.IsUnix ? "0install-gtk" :*/ "0install-win",
                    "download --batch " + interfaceID.EscapeArgument());
            }
        }
        #endregion
    }
}
