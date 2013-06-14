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
using NDesk.Options;
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
        #endregion

        #region Constructor
        /// <inheritdoc/>
        protected AddRemoveFeedCommand(Resolver resolver) : base(resolver)
        {
            Options.Add("batch", Resources.OptionBatch, unused => Resolver.Handler.Batch = true);

            Options.Add("o|offline", Resources.OptionOffline, unused => Resolver.Config.NetworkUse = NetworkLevel.Offline);
            Options.Add("r|refresh", Resources.OptionRefresh, unused => Resolver.FeedManager.Refresh = true);
        }
        #endregion

        /// <inheritdoc/>
        public override int Execute()
        {
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count == 0 || string.IsNullOrEmpty(AdditionalArgs[0])) throw new OptionException(Resources.MissingArguments, "");
            if (AdditionalArgs.Count > 2) throw new OptionException(Resources.TooManyArguments, "");

            Resolver.Handler.ShowProgressUI();

            string feedID;
            IEnumerable<string> interfaces;
            if (AdditionalArgs.Count == 2)
            { // Main interface for feed specified explicitly
                feedID = GetCanonicalID(AdditionalArgs[1]);
                interfaces = new[] {GetCanonicalID(AdditionalArgs[0])};
            }
            else
            { // Determine interfaces from feed content (<feed-for> tags)
                feedID = GetCanonicalID(AdditionalArgs[0]);
                var feed = Resolver.FeedManager.GetFeed(feedID);
                if (feed.FeedFor.IsEmpty)
                {
                    Resolver.Handler.Output(Resources.FeedManagement, string.Format(Resources.MissingFeedFor, feedID));
                    return 1;
                }
                interfaces = feed.FeedFor.Map(reference => reference.Target.ToString());
            }
            var modified = ApplyFeedToInterfaces(feedID, interfaces);

            // Show a confirmation message (but not in batch mode, since it is not important enough)
            if (!Resolver.Handler.Batch)
            {
                Resolver.Handler.Output(Resources.FeedManagement, (modified.Count == 0)
                    ? NoneModifiedMessage
                    : string.Format(ModifiedMessage, "\n".Join(modified)));
            }
            return modified.Count == 0 ? 0 : 1;
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
    }
}
