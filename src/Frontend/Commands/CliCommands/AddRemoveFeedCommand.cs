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
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Common base class for <see cref="AddFeed"/> and <see cref="RemoveFeed"/>.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class AddRemoveFeedCommand : CliCommand
    {
        #region Metadata
        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] [INTERFACE] FEED"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMin { get { return 1; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 2; } }
        #endregion

        #region State
        /// <inheritdoc/>
        protected AddRemoveFeedCommand([NotNull] ICommandHandler handler) : base(handler)
        {
            Options.Add("o|offline", () => Resources.OptionOffline, _ => Config.NetworkUse = NetworkLevel.Offline);
            Options.Add("r|refresh", () => Resources.OptionRefresh, _ => FeedManager.Refresh = true);
        }
        #endregion

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            FeedUri feedUri;
            var interfaces = GetInterfaces(out feedUri);
            if (!interfaces.Any())
            {
                Handler.Output(Resources.FeedManagement, string.Format(Resources.MissingFeedFor, feedUri));
                return ExitCode.InvalidArguments;
            }

            var modifiedInterfaces = ApplyFeedToInterfaces(feedUri, interfaces);

            if (modifiedInterfaces.Count == 0)
            {
                Handler.OutputLow(Resources.FeedManagement, NoneModifiedMessage);
                return ExitCode.OK;
            }
            else
            {
                Handler.OutputLow(Resources.FeedManagement, string.Format(ModifiedMessage, StringUtils.Join(Environment.NewLine, modifiedInterfaces.Select(x => x.ToStringRfc()))));
                return ExitCode.NoChanges;
            }
        }

        /// <summary>
        /// Adds/removes a <see cref="FeedReference"/> to/from one or more <see cref="InterfacePreferences"/>.
        /// </summary>
        /// <returns>The interfaces that were actually affected.</returns>
        [NotNull, ItemNotNull]
        protected abstract ICollection<FeedUri> ApplyFeedToInterfaces([NotNull] FeedUri feedUri, [NotNull, ItemNotNull] IEnumerable<FeedUri> interfaces);

        /// <summary>Message to be displayed if the command resulted in an action.</summary>
        protected abstract string ModifiedMessage { get; }

        /// <summary>Message to be displayed if the command resulted in no changes.</summary>
        protected abstract string NoneModifiedMessage { get; }

        #region Helpers
        /// <summary>
        /// Determines which <see cref="InterfacePreferences.Feeds"/> are to be updated.
        /// </summary>
        /// <param name="feedUri">Returns the new feed being added/removed.</param>
        /// <returns>A list of interfaces IDs to be updated.</returns>
        private IEnumerable<FeedUri> GetInterfaces(out FeedUri feedUri)
        {
            if (AdditionalArgs.Count == 2)
            { // Main interface for feed specified explicitly
                feedUri = GetCanonicalUri(AdditionalArgs[1]);
                return new[] {GetCanonicalUri(AdditionalArgs[0])};
            }
            else
            { // Determine interfaces from feed content (<feed-for> tags)
                feedUri = GetCanonicalUri(AdditionalArgs[0]);
                var feed = FeedManager.GetFeed(feedUri);
                return feed.FeedFor.Select(reference => reference.Target).WhereNotNull();
            }
        }
        #endregion
    }
}
