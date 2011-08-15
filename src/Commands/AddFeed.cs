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
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Feeds;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Register an additional source of implementations (versions) of a program. 
    /// </summary>
    [CLSCompliant(false)]
    public sealed class AddFeed : CommandBase
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "add-feed";
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] NEW-FEED"; } }

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionAddFeed; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionAddFeed; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public AddFeed(Policy policy) : base(policy)
        {
            Options.Add("batch", Resources.OptionBatch, unused => Policy.Handler.Batch = true);

            Options.Add("o|offline", Resources.OptionOffline, unused => Policy.Config.NetworkUse = NetworkLevel.Offline);
            Options.Add("r|refresh", Resources.OptionRefresh, unused => Policy.FeedManager.Refresh = true);
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            #region Sanity checks
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count == 0) throw new OptionException(Resources.MissingArguments, "");
            if (AdditionalArgs.Count > 1) throw new OptionException(Resources.TooManyArguments, "");
            #endregion

            Policy.Handler.ShowProgressUI(Cancel);

            string feedID = GetCanonicalID(StringUtils.UnescapeArgument(AdditionalArgs[0]));

            // Download the feed to be registered
            bool stale;
            var feed = Policy.FeedManager.GetFeed(feedID, Policy, out stale);
            if (Canceled) throw new UserCancelException();

            if (feed.FeedFor.IsEmpty)
            {
                Policy.Handler.Output(Resources.FeedManagement, string.Format(Resources.MissingFeedFor, feedID));
                return 1;
            }

            // Add feed to interface preference fies
            ICollection<string> addedTo = new LinkedList<string>();
            var interfaces = feed.FeedFor.Map(reference => reference.Target.ToString());
            foreach (var interfaceID in interfaces)
            {
                var preferences = InterfacePreferences.LoadFor(interfaceID);
                var reference = new FeedReference {Source = feedID};
                if (!preferences.Feeds.Contains(reference))
                {
                    preferences.Feeds.Add(reference);
                    addedTo.Add(interfaceID);
                }
                preferences.SaveFor(interfaceID);
            }

            // Show a confirmation message (but not in batch mode, since it is too unimportant)
            if (!Policy.Handler.Batch)
            {
                Policy.Handler.Output(Resources.FeedManagement, (addedTo.Count == 0)
                    ? Resources.FeedAlreadyRegistered
                    : string.Format(Resources.FeedRegistered, StringUtils.Concatenate(addedTo, "\n")));
            }
            return addedTo.Count == 0 ? 0 : 1;
        }
        #endregion
    }
}
