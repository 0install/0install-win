﻿/*
 * Copyright 2010-2012 Bastian Eicher
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
using ZeroInstall.Injector;
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Un-register a feed, reversing the effect of <see cref="AddFeed"/>.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class RemoveFeed : AddRemoveFeedCommand
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "remove-feed";
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionRemoveFeed; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionRemoveFeed; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public RemoveFeed(Policy policy) : base(policy)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Removes a <see cref="FeedReference"/> from one or more <see cref="InterfacePreferences"/>.
        /// </summary>
        /// <returns>The interfaces that were actually affected.</returns>
        protected override ICollection<string> ApplyFeedToInterfaces(string feedID, IEnumerable<string> interfaces)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            if (interfaces == null) throw new ArgumentNullException("interfaces");
            #endregion

            var modified = new List<string>();
            var reference = new FeedReference {Source = feedID};
            foreach (var interfaceID in interfaces)
            {
                var preferences = InterfacePreferences.LoadFor(interfaceID);
                if (preferences.Feeds.Remove(reference))
                    modified.Add(interfaceID);
                preferences.SaveFor(interfaceID);
            }
            return modified;
        }

        /// <inheritdoc/>
        protected override string ModifiedMessage { get { return Resources.FeedUnregistered; } }

        /// <inheritdoc/>
        protected override string NoneModifiedMessage { get { return Resources.FeedNotRegistered; } }
        #endregion
    }
}
