using System;
using System.Collections.Generic;
using Common.Utils;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Common base class for commands that manage <see cref="ZeroInstall.DesktopIntegration"/>.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class IntegrationCommand : FrontendCommand
    {
        #region Variables
        /// <summary>Apply the operation sachine-wide instead of just for the current user.</summary>
        protected bool MachineWide;
        #endregion

        #region Constructor
        /// <inheritdoc/>
        protected IntegrationCommand(Policy policy) : base(policy)
        {
            Options.Add("batch", Resources.OptionBatch, unused => Policy.Handler.Batch = true);

            Options.Add("o|offline", Resources.OptionOffline, unused => Policy.Config.NetworkUse = NetworkLevel.Offline);
            Options.Add("r|refresh", Resources.OptionRefresh, unused => Policy.FeedManager.Refresh = true);

            Options.Add("m|machine", Resources.OptionMachine, unused => MachineWide = true);
        }
        #endregion

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
        protected AppEntry CreateAppEntry(IIntegrationManager integrationManager, ref string interfaceID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            #endregion

            // Detect replaced feeds
            var feed = Policy.FeedManager.GetFeed(interfaceID, Policy);
            if (feed.ReplacedBy != null)
            {
                if (Policy.Handler.AskQuestion(
                    string.Format(Resources.FeedReplacedAsk, feed.Name, interfaceID, feed.ReplacedBy.Target),
                    string.Format(Resources.FeedReplaced, interfaceID, feed.ReplacedBy.Target)))
                {
                    interfaceID = feed.ReplacedBy.Target.ToString();
                    feed = Policy.FeedManager.GetFeed(interfaceID, Policy);
                }
            }

            var appEntry = integrationManager.AddApp(interfaceID, feed);

            // Pre-download application in background for later use
            if (Policy.Config.EffectiveNetworkUse == NetworkLevel.Full)
            {
                // ToDo: Automatically switch to GTK# on Linux
                ProcessUtils.LaunchAssembly("0install-win", "download --batch " + interfaceID.EscapeArgument());
            }

            return appEntry;
        }
        #endregion
    }
}
