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
using NanoByte.Common.Collections;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Register an additional source of implementations (versions) of a program.
    /// </summary>
    public sealed class AddFeed : AddRemoveFeedCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "add-feed";

        /// <inheritdoc/>
        public override string Description => Resources.DescriptionAddFeed;
        #endregion

        /// <inheritdoc/>
        public AddFeed([NotNull] ICommandHandler handler) : base(handler)
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
                if (preferences.Feeds.AddIfNew(source))
                    modifiedInterfaces.Add(interfaceUri);

                var effectiveStabilityPolicy = (preferences.StabilityPolicy == Stability.Unset)
                    ? (Config.HelpWithTesting ? Stability.Testing : Stability.Stable)
                    : preferences.StabilityPolicy;
                if (effectiveStabilityPolicy < suggestedStabilityPolicy)
                {
                    string stabilityMessage = string.Format(Resources.StabilityPolicySingleImplementation, suggestedStabilityPolicy);
                    if (Handler.Ask(
                        stabilityMessage + Environment.NewLine + string.Format(Resources.StabilityPolicyAutoSet, interfaceUri.ToStringRfc()),
                        defaultAnswer: false, alternateMessage: stabilityMessage))
                        preferences.StabilityPolicy = suggestedStabilityPolicy;
                }
                preferences.SaveFor(interfaceUri);
            }

            if (modifiedInterfaces.Count == 0)
            {
                Handler.OutputLow(Resources.FeedManagement, Resources.FeedAlreadyRegistered);
                return ExitCode.NoChanges;
            }
            else
            {
                Handler.OutputLow(Resources.FeedManagement,
                    Resources.FeedRegistered + Environment.NewLine +
                    StringUtils.Join(Environment.NewLine, modifiedInterfaces.Select(x => x.ToStringRfc())));
                return ExitCode.OK;
            }
        }
    }
}
