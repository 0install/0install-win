// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
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
        public override string Description => Resources.DescriptionRemoveFeed;
        #endregion

        /// <inheritdoc/>
        public RemoveFeed([NotNull] ICommandHandler handler)
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
