/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common.Utils;
using ZeroInstall.Backend;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Model;
using ZeroInstall.Model.Preferences;
using ZeroInstall.Store;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Common base class for <see cref="AddFeed"/> and <see cref="RemoveFeed"/>.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class AddRemoveFeedCommand : FrontendCommand
    {
        #region Properties
        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] [INTERFACE] FEED"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMin { get { return 1; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 2; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        protected AddRemoveFeedCommand(Resolver resolver) : base(resolver)
        {
            Options.Add("batch", () => Resources.OptionBatch, unused => Resolver.Handler.Batch = true);

            Options.Add("o|offline", () => Resources.OptionOffline, unused => Resolver.Config.NetworkUse = NetworkLevel.Offline);
            Options.Add("r|refresh", () => Resources.OptionRefresh, unused => Resolver.FeedManager.Refresh = true);
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            Resolver.Handler.ShowProgressUI();

            string feedID;
            var interfaces = GetInterfaces(out feedID);
            if (interfaces.Count == 0)
            {
                Resolver.Handler.Output(Resources.FeedManagement, string.Format(Resources.MissingFeedFor, feedID));
                return 1;
            }

            var modified = ApplyFeedToInterfaces(feedID, interfaces);

            Resolver.Handler.OutputLow(Resources.FeedManagement, (modified.Count == 0)
                ? NoneModifiedMessage
                : string.Format(ModifiedMessage, Environment.NewLine.Join(modified)));
            return (modified.Count == 0) ? 0 : 1;
        }
        #endregion

        #region Helpers
        private IList<string> GetInterfaces(out string feedID)
        {
            if (AdditionalArgs.Count == 2)
            {
                // Main interface for feed specified explicitly
                feedID = GetCanonicalID(AdditionalArgs[1]);
                return new[] {GetCanonicalID(AdditionalArgs[0])};
            }
            else
            {
                // Determine interfaces from feed content (<feed-for> tags)
                feedID = GetCanonicalID(AdditionalArgs[0]);
                var feed = Resolver.FeedManager.GetFeed(feedID);
                return feed.FeedFor.Map(reference => reference.Target.ToString());
            }
        }

        /// <summary>
        /// Adds/removes a <see cref="FeedReference"/> to/from one or more <see cref="InterfacePreferences"/>.
        /// </summary>
        /// <returns>The interfaces that were actually affected.</returns>
        protected abstract ICollection<string> ApplyFeedToInterfaces(string feedID, IEnumerable<string> interfaces);

        /// <summary>Message to be displayed if the command resulted in an action.</summary>
        protected abstract string ModifiedMessage { get; }

        /// <summary>Message to be displayed if the command resulted in no changes.</summary>
        protected abstract string NoneModifiedMessage { get; }
        #endregion
    }
}
