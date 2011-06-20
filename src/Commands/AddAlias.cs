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
using System.Collections.Generic;
using Common;
using Common.Collections;
using Common.Storage;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Injector;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Create an alias for a <see cref="Run"/> command.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class AddAlias : IntegrationCommand
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "add-alias";
        #endregion

        #region Variables
        private bool _resolve;
        private bool _remove;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Usage { get { return "ALIAS [INTERFACE [COMMAND]]"; } }

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionAddAlias; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionAppCommand; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public AddAlias(Policy policy) : base(policy)
        {
            Options.Add("resolve", Resources.OptionAliasResolve, unused => _resolve = true);
            Options.Add("remove", Resources.OptionAliasRemove, unused => _remove = true);
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            #region Sanity checks
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            #endregion

            if (Locations.IsPortable) throw new NotSupportedException(Resources.NotAvailableInPortableMode);
            if (SystemWide && WindowsUtils.IsWindows && !WindowsUtils.IsAdministrator) return RerunAsAdmin();

            if (AdditionalArgs.Count < 1) throw new OptionException(Resources.MissingArguments, "");
            string aliasName = AdditionalArgs[0];
            var integrationManager = new IntegrationManager(SystemWide);

            if (_resolve || _remove)
            {
                if (AdditionalArgs.Count > 1) throw new OptionException(Resources.TooManyArguments, "");

                AppEntry appEntry;
                var appAlias = GetAppAlias(integrationManager.AppList, AdditionalArgs[0], out appEntry);
                if (appAlias == null)
                {
                    Policy.Handler.Output(Resources.AppAlias, string.Format(Resources.AliasNotFound, aliasName));
                    return 1;
                }

                if (_resolve)
                {
                    string result = appEntry.InterfaceID;
                    if (!string.IsNullOrEmpty(appAlias.Command)) result += Environment.NewLine + "Command: " + appAlias.Command;
                    Policy.Handler.Output(Resources.AppAlias, result);
                    return 0;
                }
                if (_remove)
                {
                    integrationManager.RemoveAccessPoints(appEntry.InterfaceID, new AccessPoint[] {appAlias});

                    // Show a "integration complete" message (but not in batch mode, since it is too unimportant)
                    Policy.Handler.Output(Resources.AppAlias, string.Format(Resources.AliasRemoved, aliasName, appEntry.Name));
                    return 0;
                }
            }

            if (AdditionalArgs.Count < 2) throw new OptionException(Resources.MissingArguments, "");
            if (AdditionalArgs.Count > 3) throw new OptionException(Resources.TooManyArguments, "");

            // Collect the alias data
            var alias = new AppAlias { Name = aliasName };
            string interfaceID = ModelUtils.CanonicalID(StringUtils.UnescapeWhitespace(AdditionalArgs[1]));
            if (AdditionalArgs.Count >= 3) alias.Command = AdditionalArgs[2];

            Policy.Handler.ShowProgressUI(Cancel);
            CacheFeed(interfaceID);
            bool stale;
            var feed = Policy.FeedManager.GetFeed(interfaceID, Policy, out stale);

            if (Canceled) throw new UserCancelException();

            // Apply the new alias
            try { integrationManager.AddAccessPoints(new InterfaceFeed(interfaceID, feed), new AccessPoint[] {alias}, Policy.Handler); }
            catch (InvalidOperationException ex)
            {
                // Show a "failed to comply" message
                Policy.Handler.Output(Resources.AppAlias, ex.Message);
                return 1;
            }

            // Show a "integration complete" message (but not in batch mode, since it is too unimportant)
            if (!Policy.Handler.Batch) Policy.Handler.Output(Resources.DesktopIntegration, string.Format(Resources.AliasCreated, aliasName, feed.Name));
            return 0;
        }

        /// <summary>
        /// Retreives a specific <see cref="AppAlias"/>.
        /// </summary>
        /// <param name="appList">The list of <see cref="AppEntry"/>s to search.</param>
        /// <param name="aliasName">The name of the alias to search for.</param>
        /// <param name="foundAppEntry">Returns the <see cref="AppEntry"/> containing the found <see cref="AppAlias"/>; <see langword="null"/> if none was found.</param>
        /// <returns>The first <see cref="AppAlias"/> in <paramref name="appList"/> matching <paramref name="aliasName"/>; <see langword="null"/> if none was found.</returns>
        private static AppAlias GetAppAlias(AppList appList, string aliasName, out AppEntry foundAppEntry)
        {
            foreach (var appEntry in appList.Entries)
            {
                foreach (var appAlias in EnumerableUtils.OfType<AppAlias>(appEntry.AccessPoints.Entries))
                {
                    if (appAlias.Name == aliasName)
                    {
                        foundAppEntry = appEntry;
                        return appAlias;
                    }
                }
            }

            foundAppEntry = null;
            return null;
        }
        #endregion
    }
}
