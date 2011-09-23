/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.IO;
using System.Net;
using Common;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Common base class for commands that manage an <see cref="AppList"/>.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class AppCommand : IntegrationCommand
    {
        #region Properties
        /// <inheritdoc/>
        protected override string Usage { get { return "INTERFACE [OPTIONS]"; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionAppCommand; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        protected AppCommand(Policy policy) : base(policy)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            #region Sanity checks
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count == 0 || string.IsNullOrEmpty(AdditionalArgs.First)) throw new OptionException(Resources.MissingArguments, "");
            if (AdditionalArgs.Count > 1) throw new OptionException(Resources.TooManyArguments, "");
            #endregion

            if (SystemWide && WindowsUtils.IsWindows && !WindowsUtils.IsAdministrator) return RerunAsAdmin();

            Policy.Handler.ShowProgressUI(Cancel);
            string interfaceID = GetCanonicalID(AdditionalArgs[0]);
            using (var integrationManager = new CategoryIntegrationManager(SystemWide, Policy.Handler))
                return ExecuteHelper(interfaceID, integrationManager);
        }

        /// <summary>
        /// Template method that performs the actual operation.
        /// </summary>
        /// <param name="interfaceID">The interface for the application to perform the operation on.</param>
        /// <param name="integrationManager">Manages desktop integration operations.</param>
        /// <returns>The exit status code to end the process with. 0 means OK, 1 means generic error.</returns>
        protected abstract int ExecuteHelper(string interfaceID, CategoryIntegrationManager integrationManager);

        /// <summary>
        /// Returns a specific <see cref="Feed"/>.
        /// </summary>
        /// <param name="feedID">The canonical ID used to identify the feed.</param>
        /// <returns>The parsed <see cref="Feed"/> object.</returns>
        /// <exception cref="UserCancelException">Thrown if the user canceled the process.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown if <paramref name="feedID"/> is an invalid interface ID.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while reading the feed file.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while fetching the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the cache is not permitted.</exception>
        /// <exception cref="SignatureException">Thrown if the signature data of a feed file could not be handled.</exception>
        protected Feed GetFeed(string feedID)
        {
            bool stale;
            Feed feed = Policy.FeedManager.GetFeed(feedID, Policy, out stale);
            if (Canceled) throw new UserCancelException();

            // Refresh if stale instead of spawning background updater like 'run'
            if (stale)
            {
                feed = Policy.FeedManager.GetFeed(feedID, Policy, out stale);
                if (Canceled) throw new UserCancelException();
            }

            return feed;
        }
        #endregion
    }
}
