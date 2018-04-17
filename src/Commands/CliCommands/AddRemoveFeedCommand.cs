// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common.Collections;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Common base class for <see cref="AddFeed"/> and <see cref="RemoveFeed"/>.
    /// </summary>
    public abstract class AddRemoveFeedCommand : CliCommand
    {
        #region Metadata
        /// <inheritdoc/>
        public override string Usage => "[OPTIONS] [INTERFACE] FEED";

        /// <inheritdoc/>
        protected override int AdditionalArgsMin => 1;

        /// <inheritdoc/>
        protected override int AdditionalArgsMax => 2;
        #endregion

        #region State
        /// <inheritdoc/>
        protected AddRemoveFeedCommand([NotNull] ICommandHandler handler)
            : base(handler)
        {
            Options.Add("o|offline", () => Resources.OptionOffline, _ => Config.NetworkUse = NetworkLevel.Offline);
            Options.Add("r|refresh", () => Resources.OptionRefresh, _ => FeedManager.Refresh = true);
        }
        #endregion

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            FeedUri feedUri;
            IEnumerable<FeedUri> interfaces;
            var suggestedStabilityPolicy = Stability.Unset;
            if (AdditionalArgs.Count == 2)
            { // Main interface for feed specified explicitly
                feedUri = GetCanonicalUri(AdditionalArgs[1]);
                interfaces = new[] {GetCanonicalUri(AdditionalArgs[0])};
            }
            else
            { // Determine interfaces from feed content (<feed-for> tags)
                feedUri = GetCanonicalUri(AdditionalArgs[0]);
                interfaces = GetInterfaces(feedUri, ref suggestedStabilityPolicy);
            }

            return ExecuteHelper(interfaces, new FeedReference {Source = feedUri}, suggestedStabilityPolicy);
        }

        /// <summary>
        /// Returns a list of all interface URIs a given feed can be registered for.
        /// </summary>
        /// <param name="feedUri">The URI of the feed to register.</param>
        /// <param name="suggestedStabilityPolicy">Returns the suggested value for <see cref="InterfacePreferences.StabilityPolicy"/>.</param>
        /// <returns>A set of interface URIs.</returns>
        private IEnumerable<FeedUri> GetInterfaces(FeedUri feedUri, ref Stability suggestedStabilityPolicy)
        {
            var feed = FeedManager.GetFresh(feedUri);
            var interfaces = feed.FeedFor.Select(reference => reference.Target).WhereNotNull().ToList();
            if (interfaces.Count == 0)
                throw new OptionException(string.Format(Resources.MissingFeedFor, feedUri), null);

            if (feed.Elements.Count == 1)
            {
                if (feed.Elements[0] is Implementation singletonImplementation)
                    suggestedStabilityPolicy = singletonImplementation.Stability;
            }

            return interfaces;
        }

        /// <summary>
        /// Registers or unregisters an additional feed source for a set of interfaces.
        /// </summary>
        /// <param name="interfaces">The set of interface URIs to register the feed <paramref name="source"/> for.</param>
        /// <param name="source">The feed reference to register for the <paramref name="interfaces"/>.</param>
        /// <param name="suggestedStabilityPolicy">The suggested value for <see cref="InterfacePreferences.StabilityPolicy"/>. Will be <see cref="Stability.Unset"/> unless there is exactly one <see cref="Implementation"/> in the <see cref="Feed"/>.</param>
        /// <returns></returns>
        protected abstract ExitCode ExecuteHelper(IEnumerable<FeedUri> interfaces, FeedReference source, Stability suggestedStabilityPolicy);
    }
}
