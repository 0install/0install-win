/*
 * Copyright 2010-2016 Bastian Eicher
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
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Un-register a feed, reversing the effect of <see cref="AddFeed"/>.
    /// </summary>
    public sealed class RemoveFeed : AddRemoveFeedCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "remove-feed";

        /// <inheritdoc/>
        protected override string Description => Resources.DescriptionRemoveFeed;
        #endregion

        /// <inheritdoc/>
        public RemoveFeed([NotNull] ICommandHandler handler) : base(handler)
        {}

        /// <inheritdoc/>
        protected override ExitCode ExecuteHelper(IEnumerable<FeedUri> interfaces, FeedReference source, Stability suggestedStabilityPolicy)
        {
            #region Sanity checks
            if (interfaces == null) throw new ArgumentNullException(nameof(interfaces));
            if (source == null) throw new ArgumentNullException(nameof(source));
            #endregion

            var modifiedInterfaces = new List<FeedUri>();
            foreach (var interfaceUri in interfaces)
            {
                var preferences = InterfacePreferences.LoadFor(interfaceUri);
                if (preferences.Feeds.Remove(source))
                {
                    modifiedInterfaces.Add(interfaceUri);
                    if (preferences.StabilityPolicy == suggestedStabilityPolicy && suggestedStabilityPolicy != Stability.Unset)
                    {
                        if (Handler.Ask(string.Format(Resources.StabilityPolicyReset, interfaceUri.ToStringRfc()), defaultAnswer: false))
                            preferences.StabilityPolicy = Stability.Unset;
                    }
                    preferences.SaveFor(interfaceUri);
                }
            }

            if (modifiedInterfaces.Count == 0)
            {
                Handler.OutputLow(Resources.FeedManagement, Resources.FeedNotRegistered);
                return ExitCode.NoChanges;
            }
            else
            {
                Handler.OutputLow(Resources.FeedManagement,
                    Resources.FeedUnregistered + Environment.NewLine +
                    StringUtils.Join(Environment.NewLine, modifiedInterfaces.Select(x => x.ToStringRfc())));
                return ExitCode.OK;
            }
        }
    }
}
