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
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Un-register a feed, reversing the effect of <see cref="AddFeed"/>.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class RemoveFeed : AddRemoveFeedCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "remove-feed";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionRemoveFeed; } }

        /// <inheritdoc/>
        public RemoveFeed(ICommandHandler handler) : base(handler)
        {}
        #endregion

        /// <summary>
        /// Removes a <see cref="FeedReference"/> from one or more <see cref="InterfacePreferences"/>.
        /// </summary>
        /// <returns>The interfaces that were actually affected.</returns>
        protected override ICollection<FeedUri> ApplyFeedToInterfaces(FeedUri feedUri, IEnumerable<FeedUri> interfaces)
        {
            #region Sanity checks
            if (feedUri == null) throw new ArgumentNullException("feedUri");
            if (interfaces == null) throw new ArgumentNullException("interfaces");
            #endregion

            var modified = new List<FeedUri>();
            var reference = new FeedReference {Source = feedUri};
            foreach (var interfaceUri in interfaces)
            {
                var preferences = InterfacePreferences.LoadFor(interfaceUri);
                if (preferences.Feeds.Remove(reference))
                    modified.Add(interfaceUri);
                preferences.SaveFor(interfaceUri);
            }
            return modified;
        }

        /// <inheritdoc/>
        protected override string ModifiedMessage { get { return Resources.FeedUnregistered; } }

        /// <inheritdoc/>
        protected override string NoneModifiedMessage { get { return Resources.FeedNotRegistered; } }
    }
}
