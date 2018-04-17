// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
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
        public AddFeed([NotNull] ICommandHandler handler)
            : base(handler)
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
