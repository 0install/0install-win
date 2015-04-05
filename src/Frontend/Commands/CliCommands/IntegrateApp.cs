/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common.Collections;
using NanoByte.Common.Tasks;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Add an application to the <see cref="AppList"/> (if missing) and integrate it into the desktop environment.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class IntegrateApp : AppCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "integrate";

        /// <summary>The alternative name of this command as used in command-line arguments in lower-case.</summary>
        public const string AltName = "integrate-app";

        /// <summary>Another alternative name of this command as used in command-line arguments in lower-case.</summary>
        public const string AltName2 = "desktop";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionIntegrateApp; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] (PET-NAME|INTERFACE)"; } }
        #endregion

        #region State
        /// <summary>A list of all <see cref="AccessPoint"/> categories to be added to the already applied ones.</summary>
        private readonly List<string> _addCategories = new List<string>();

        /// <summary>A list of all <see cref="AccessPoint"/> categories to be removed from the already applied ones.</summary>
        private readonly List<string> _removeCategories = new List<string>();

        /// <inheritdoc/>
        public IntegrateApp([NotNull] ICommandHandler handler) : base(handler)
        {
            Options.Add("no-download", () => Resources.OptionNoDownload, _ => NoDownload = true);

            Options.Add("add-standard", () => Resources.OptionIntegrateAddStandard, _ => _addCategories.AddRange(CategoryIntegrationManager.StandardCategories));
            Options.Add("add-all", () => Resources.OptionIntegrateAddAll, _ => _addCategories.AddRange(CategoryIntegrationManager.AllCategories));
            Options.Add("add=", () => Resources.OptionIntegrateAdd + "\n" + SupportedValues(CategoryIntegrationManager.AllCategories), category =>
            {
                category = category.ToLower();
                if (!CategoryIntegrationManager.AllCategories.Contains(category)) throw new OptionException(string.Format(Resources.UnknownCategory, category), "add");
                _addCategories.Add(category);
            });
            Options.Add("remove-all", () => Resources.OptionIntegrateRemoveAll, _ => _removeCategories.AddRange(CategoryIntegrationManager.AllCategories));
            Options.Add("remove=", () => Resources.OptionIntegrateRemove + "\n" + SupportedValues(CategoryIntegrationManager.AllCategories), category =>
            {
                category = category.ToLower();
                if (!CategoryIntegrationManager.AllCategories.Contains(category)) throw new OptionException(string.Format(Resources.UnknownCategory, category), "remove");
                _removeCategories.Add(category);
            });
        }
        #endregion

        /// <inheritdoc/>
        protected override ExitCode ExecuteHelper(ICategoryIntegrationManager integrationManager, FeedUri interfaceUri)
        {
            #region Sanity checks
            if (interfaceUri == null) throw new ArgumentNullException("interfaceUri");
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            #endregion

            if (RemoveOnly())
            {
                RemoveOnly(integrationManager, interfaceUri);
                return ExitCode.OK;
            }

            var appEntry = GetAppEntry(integrationManager, ref interfaceUri);
            var feed = FeedManager.GetFeed(interfaceUri);

            if (NoSpecifiedIntegrations())
            {
                var state = new IntegrationState(integrationManager, appEntry, feed);
                Retry:
                Handler.ShowIntegrateApp(state);
                try
                {
                    state.ApplyChanges();
                }
                    #region Error handling
                catch (ConflictException ex)
                {
                    if (Handler.Ask(
                        Resources.IntegrateAppInvalid + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine + Resources.IntegrateAppRetry,
                        defaultAnswer: false, alternateMessage: ex.Message))
                        goto Retry;
                }
                catch (InvalidDataException ex)
                {
                    if (Handler.Ask(
                        Resources.IntegrateAppInvalid + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine + Resources.IntegrateAppRetry,
                        defaultAnswer: false, alternateMessage: ex.Message))
                        goto Retry;
                }
                #endregion

                return ExitCode.OK;
            }

            RemoveAndAdd(integrationManager, feed, appEntry);
            return ExitCode.OK;
        }

        #region Helpers
        /// <summary>
        /// Determines whether the user specified only removals. This means we do not need to fetch any feeds.
        /// </summary>
        private bool RemoveOnly()
        {
            return !_addCategories.Any() && _removeCategories.Any();
        }

        /// <summary>
        /// Applies the <see cref="_removeCategories"/> specified by the user.
        /// </summary>
        private void RemoveOnly(ICategoryIntegrationManager integrationManager, FeedUri interfaceUri)
        {
            integrationManager.RemoveAccessPointCategories(integrationManager.AppList[interfaceUri], _removeCategories.ToArray());
        }

        /// <summary>
        /// Determines whether the user specified no integration changes. This means we need a GUI to ask what to do.
        /// </summary>
        private bool NoSpecifiedIntegrations()
        {
            return !_addCategories.Any() && !_removeCategories.Any();
        }

        /// <summary>
        /// Applies the <see cref="_removeCategories"/> and <see cref="_addCategories"/> specified by the user.
        /// </summary>
        private void RemoveAndAdd(ICategoryIntegrationManager integrationManager, Feed feed, AppEntry appEntry)
        {
            if (_removeCategories.Any())
                integrationManager.RemoveAccessPointCategories(appEntry, _removeCategories.ToArray());

            if (_addCategories.Any())
                integrationManager.AddAccessPointCategories(appEntry, feed, _addCategories.ToArray());
        }

        /// <summary>
        /// Finds an existing <see cref="AppEntry"/> or creates a new one for a specific interface URI and feed.
        /// </summary>
        /// <param name="integrationManager">Manages desktop integration operations.</param>
        /// <param name="interfaceUri">The interface URI to create an <see cref="AppEntry"/> for. Will be updated if <see cref="Feed.ReplacedBy"/> is set and accepted by the user.</param>
        protected override AppEntry GetAppEntry(IIntegrationManager integrationManager, ref FeedUri interfaceUri)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            if (interfaceUri == null) throw new ArgumentNullException("interfaceUri");
            #endregion

            var appEntry = base.GetAppEntry(integrationManager, ref interfaceUri);

            // Detect feed changes that may make an AppEntry update necessary
            var feed = FeedManager.GetFeedFresh(interfaceUri);
            if (!appEntry.CapabilityLists.UnsequencedEquals(feed.CapabilityLists))
            {
                string changedMessage = string.Format(Resources.CapabilitiesChanged, appEntry.Name);
                if (Handler.Ask(
                    changedMessage + " " + Resources.AskUpdateCapabilities,
                    defaultAnswer: false, alternateMessage: changedMessage))
                    integrationManager.UpdateApp(appEntry, feed);
            }
            return appEntry;
        }
        #endregion
    }
}
