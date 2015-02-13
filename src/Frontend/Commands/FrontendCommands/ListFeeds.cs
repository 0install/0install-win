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
using System.Linq;
using JetBrains.Annotations;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Model.Preferences;

namespace ZeroInstall.Commands.FrontendCommands
{
    /// <summary>
    /// List all known feed URIs for a specific interface.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class ListFeeds : FrontendCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "list-feeds";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionListFeeds; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] URI"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMin { get { return 1; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 1; } }
        #endregion

        /// <inheritdoc/>
        public ListFeeds([NotNull] ICommandHandler handler) : base(handler)
        {}

        /// <inheritdoc/>
        public override int Execute()
        {
            var interfaceUri = GetCanonicalUri(AdditionalArgs[0]);
            var preferences = InterfacePreferences.LoadFor(interfaceUri);

            Handler.Output(
                string.Format(Resources.FeedsRegistered, interfaceUri),
                preferences.Feeds.Select(x => x.Source));
            return 0;
        }
    }
}
