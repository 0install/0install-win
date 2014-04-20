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
using System.IO;
using System.Text;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services;
using ZeroInstall.Store.Model.Preferences;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// List all known feed IDs for a specific interface.
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

        /// <inheritdoc/>
        public ListFeeds(ICommandHandler handler) : base(handler)
        {}
        #endregion

        /// <inheritdoc/>
        public override int Execute()
        {
            string interfaceID = AdditionalArgs[0];
            if (File.Exists(AdditionalArgs[0])) interfaceID = Path.GetFullPath(AdditionalArgs[0]);

            Handler.Output(
                string.Format(Resources.FeedsRegistered, interfaceID),
                GetRegisteredFeeds(interfaceID));
            return 0;
        }

        #region Helpers
        private static string GetRegisteredFeeds(string interfaceID)
        {
            var preferences = InterfacePreferences.LoadFor(interfaceID);

            var builder = new StringBuilder();
            foreach (var feedReference in preferences.Feeds)
                builder.AppendLine(feedReference.Source);
            return builder.ToString();
        }
        #endregion
    }
}
