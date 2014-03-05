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
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Register an additional source of implementations (versions) of a program.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class AddFeed : AddRemoveFeedCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "add-feed";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionAddFeed; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionAddFeed; } }

        /// <inheritdoc/>
        public AddFeed(ICommandHandler handler) : base(handler)
        {}
        #endregion

        /// <summary>
        /// Adds a <see cref="FeedReference"/> to one or more <see cref="InterfacePreferences"/>.
        /// </summary>
        /// <returns>The interfaces that were actually affected.</returns>
        protected override ICollection<string> ApplyFeedToInterfaces(string feedID, IEnumerable<string> interfaces)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            if (interfaces == null) throw new ArgumentNullException("interfaces");
            #endregion

            var modified = new List<string>();
            foreach (var interfaceID in interfaces)
            {
                var preferences = InterfacePreferences.LoadFor(interfaceID);
                var reference = new FeedReference {Source = feedID};
                if (!preferences.Feeds.Contains(reference)) // Prevent duplicate entries
                {
                    preferences.Feeds.Add(reference);
                    modified.Add(interfaceID);
                }
                preferences.SaveFor(interfaceID);
            }
            return modified;
        }

        /// <inheritdoc/>
        protected override string ModifiedMessage { get { return Resources.FeedRegistered; } }

        /// <inheritdoc/>
        protected override string NoneModifiedMessage { get { return Resources.FeedAlreadyRegistered; } }
    }
}
