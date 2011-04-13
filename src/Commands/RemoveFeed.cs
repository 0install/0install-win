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
using System.IO;
using Common.Utils;
using NDesk.Options;
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
    public sealed class RemoveFeed : CommandBase
    {
        #region Variables
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "remove-feed";
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] [INTERFACE] FEED"; } }

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionRemoveFeed; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public RemoveFeed(Policy policy) : base(policy)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            #region Sanity checks
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count == 0) throw new OptionException(Resources.MissingArguments, "");
            if (AdditionalArgs.Count > 2) throw new OptionException(Resources.TooManyArguments, "");
            #endregion

            string feedID;
            IEnumerable<string> interfaces;
            if (AdditionalArgs.Count == 2)
            {
                interfaces = new[] {ModelUtils.CanonicalID(StringUtils.Unescape(AdditionalArgs[0]))};
                feedID = AdditionalArgs[1];
                if (File.Exists(feedID)) feedID = Path.GetFullPath(feedID);
            }
            else
            {
                feedID = ModelUtils.CanonicalID(StringUtils.Unescape(AdditionalArgs[0]));

                Policy.FeedManager.Refresh = true;
                bool stale;
                var feed = Policy.FeedManager.GetFeed(feedID, Policy, out stale);
                interfaces = feed.FeedFor.Map(reference => reference.Target.ToString());
            }

            bool removed = false;
            foreach (var interfaceID in interfaces)
            {
                var preferences = InterfacePreferences.LoadFor(interfaceID);
                removed |= preferences.Feeds.Remove(new FeedReference {Source = feedID});
                preferences.SaveFor(interfaceID);
            }

            // Show a confirmation message (but not in batch mode, since it is too unimportant)
            if (!Policy.Handler.Batch) Policy.Handler.Output(Resources.FeedManagement, removed ? Resources.FeedUnregistered : Resources.FeedNotRegistered);
            return removed ? 0 : 1;
        }
        #endregion
    }
}
