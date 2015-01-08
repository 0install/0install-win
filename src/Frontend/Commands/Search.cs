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
using NanoByte.Common;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services.Feeds;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Searches for feeds indexed by the mirror server.
    /// </summary>
    [CLSCompliant(false)]
    public class Search : FrontendCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "search";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionSearch; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "QUERY"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMin
        {
            get
            {
                // GUI handlers may allow the user to enter search queries after launching
                return (Handler is CliCommandHandler) ? 1 : 0;
            }
        }

        /// <inheritdoc/>
        public Search(ICommandHandler handler) : base(handler)
        {}
        #endregion

        /// <inheritdoc/>
        public override int Execute()
        {
            string keywords = StringUtils.Join(" ", AdditionalArgs);
            Handler.ShowFeedSearch(SearchQuery.Perform(Config, keywords));
            return 0;
        }
    }
}
