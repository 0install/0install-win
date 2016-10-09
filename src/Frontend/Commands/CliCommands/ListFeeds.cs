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

using System.Linq;
using JetBrains.Annotations;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Model.Preferences;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// List all known feed URIs for a specific interface.
    /// </summary>
    public sealed class ListFeeds : CliCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "list-feeds";

        /// <inheritdoc/>
        public override string Description => Resources.DescriptionListFeeds;

        /// <inheritdoc/>
        public override string Usage => "[OPTIONS] URI";

        /// <inheritdoc/>
        protected override int AdditionalArgsMin => 1;

        /// <inheritdoc/>
        protected override int AdditionalArgsMax => 1;
        #endregion

        /// <inheritdoc/>
        public ListFeeds([NotNull] ICommandHandler handler) : base(handler)
        {}

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            var interfaceUri = GetCanonicalUri(AdditionalArgs[0]);
            var preferences = InterfacePreferences.LoadFor(interfaceUri);

            Handler.Output(
                string.Format(Resources.FeedsRegistered, interfaceUri),
                preferences.Feeds.Select(x => x.Source));
            return ExitCode.OK;
        }
    }
}
