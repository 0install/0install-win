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
using System.Text;
using ZeroInstall.Commands.Properties;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// List all known interface (program) URIs.
    /// </summary>
    /// <remarks>If a search term is given, only URIs containing that string are shown (case insensitive).</remarks>
    [CLSCompliant(false)]
    public sealed class List : FrontendCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "list";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionList; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[PATTERN]"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 1; } }

        /// <inheritdoc/>
        public List(ICommandHandler handler) : base(handler)
        {}
        #endregion

        /// <inheritdoc/>
        public override int Execute()
        {
            Handler.Output(Resources.FoundFeeds,
                (AdditionalArgs.Count == 0) ? GetList() : GetList(AdditionalArgs[0]));
            return 0;
        }

        #region Helpers
        /// <summary>
        /// Build a list of all feed cache entries.
        /// </summary>
        /// <param name="pattern">Only return feeds that contain this substring; <see langword="null"/> to return all.</param>
        private string GetList(string pattern = null)
        {
            var builder = new StringBuilder();
            var feeds = FeedCache.ListAll();
            foreach (string entry in feeds.Where(entry => pattern == null || entry.Contains(pattern)))
                builder.AppendLine(entry);
            return (builder.Length == 0 ? "" : builder.ToString(0, builder.Length - Environment.NewLine.Length)); // Remove trailing line-break
        }
        #endregion
    }
}
