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
using System.Linq;
using Common.Collections;
using Common.Storage;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Add an application to the application list (if missing) and integrate it into the desktop environment.
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

        /// <inheritdoc/>
        public override int GuiDelay { get { return Handler.Batch ? 0 : 1000; } }

        /// <inheritdoc/>
        public IntegrateApp(ICommandHandler handler) : base(handler)
        {
            string categoryList = StringUtils.Join(", ", CategoryIntegrationManager.Categories);

            Options.Add("a|add=", () => Resources.OptionAppAdd + "\n" + Resources.OptionAppCategory + categoryList, category =>
            {
                category = category.ToLower();
                if (!CategoryIntegrationManager.Categories.Contains(category)) throw new OptionException(string.Format(Resources.UnknownCategory, category), "add");
                _addCategories.Add(category);
            });
            Options.Add("x|remove=", () => Resources.OptionAppRemove + "\n" + Resources.OptionAppCategory + categoryList, category =>
            {
                category = category.ToLower();
                if (!CategoryIntegrationManager.Categories.Contains(category)) throw new OptionException(string.Format(Resources.UnknownCategory, category), "remove");
                _removeCategories.Add(category);
            });
            Options.Add("i|import=", () => Resources.OptionAppImport, path => _importLists.Add(path));
        }
        #endregion

        #region State
        /// <summary>A list of all <see cref="AccessPoint"/> categories to be added to the already applied ones.</summary>
        private readonly List<string> _addCategories = new List<string>();

        /// <summary>A list of all <see cref="AccessPoint"/> categories to be removed from the already applied ones.</summary>
        private readonly List<string> _removeCategories = new List<string>();

        /// <summary>A list of <see cref="AccessPointList"/> files to be imported.</summary>
        private readonly List<string> _importLists = new List<string>();
        #endregion

        /// <inheritdoc/>
        protected override int ExecuteHelper(ICategoryIntegrationManager integrationManager, string interfaceID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            #endregion

            if (RemoveOnly())
            {
                RemoveOnly(integrationManager, interfaceID);
                return 0;
            }

            var appEntry = GetAppEntry(integrationManager, ref interfaceID);
            var feed = FeedManager.GetFeedFresh(interfaceID);

            if (NoSpecifiedIntegrations())
            {
                Handler.ShowIntegrateApp(integrationManager, appEntry, feed);
                return 0;
            }

            RemoveAndAdd(integrationManager, feed, appEntry);
            return 0;
        }

        #region Helpers
        /// <summary>
        /// Determines whether the user specified only removals. This means we do not need to fetch any feeds.
        /// </summary>
        private bool RemoveOnly()
        {
            return !_addCategories.Any() && !_importLists.Any() && _removeCategories.Any();
        }

        /// <summary>
        /// Applies the <see cref="_removeCategories"/> specified by the user.
        /// </summary>
        private void RemoveOnly(ICategoryIntegrationManager integrationManager, string interfaceID)
        {
            integrationManager.RemoveAccessPointCategories(integrationManager.AppList[interfaceID], _removeCategories.ToArray());
        }

        /// <summary>
        /// Determines whether the user specified no integration changes. This means we need a GUI to ask what to do.
        /// </summary>
        private bool NoSpecifiedIntegrations()
        {
            return !_addCategories.Any() && !_removeCategories.Any() && !_importLists.Any();
        }

        /// <summary>
        /// Applies the <see cref="_removeCategories"/> and <see cref="_addCategories"/> specified by the user.
        /// </summary>
        private void RemoveAndAdd(ICategoryIntegrationManager integrationManager, Feed feed, AppEntry appEntry)
        {
            if (_removeCategories.Any())
                integrationManager.RemoveAccessPointCategories(appEntry, _removeCategories.ToArray());

            try
            {
                if (_addCategories.Any())
                    integrationManager.AddAccessPointCategories(appEntry, feed, _addCategories.ToArray());

                foreach (string path in _importLists)
                    integrationManager.AddAccessPoints(appEntry, feed, XmlStorage.LoadXml<AccessPointList>(path).Entries);
            }
                #region Error handling
            catch (InvalidOperationException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new NotSupportedException(ex.Message, ex);
            }
            #endregion
        }

        /// <summary>
        /// Finds an existing <see cref="AppEntry"/> or creates a new one for a specific interface ID and feed.
        /// </summary>
        /// <param name="integrationManager">Manages desktop integration operations.</param>
        /// <param name="interfaceID">The interface ID to create an <see cref="AppEntry"/> for. Will be updated if <see cref="Feed.ReplacedBy"/> is set and accepted by the user.</param>
        protected override AppEntry GetAppEntry(IIntegrationManager integrationManager, ref string interfaceID)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            #endregion

            var appEntry = base.GetAppEntry(integrationManager, ref interfaceID);

            // Detect feed changes that may make an AppEntry update necessary
            var feed = FeedManager.GetFeedFresh(interfaceID);
            if (!appEntry.CapabilityLists.UnsequencedEquals(feed.CapabilityLists))
            {
                string changedMessage = string.Format(Resources.CapabilitiesChanged, appEntry.Name);
                if (Handler.AskQuestion(changedMessage + " " + Resources.AskUpdateCapabilities, changedMessage))
                    integrationManager.UpdateApp(appEntry, feed);
            }
            return appEntry;
        }
        #endregion
    }
}
