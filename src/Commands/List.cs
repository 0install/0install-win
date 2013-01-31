/*
 * Copyright 2010-2013 Bastian Eicher
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
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// List all known interface (program) URIs.
    /// </summary>
    /// <remarks>If a search term is given, only URIs containing that string are shown (case insensitive).</remarks>
    [CLSCompliant(false)]
    public sealed class List : FrontendCommand
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "list";
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionList; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[PATTERN]"; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public List(Policy policy) : base(policy)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);

            // Allow 0 or 1 arguments
            string pattern;
            switch (AdditionalArgs.Count)
            {
                case 0:
                    pattern = null;
                    break;
                case 1:
                    pattern = AdditionalArgs[0];
                    break;
                default:
                    throw new OptionException(Resources.TooManyArguments, "");
            }

            Policy.Handler.Output(Resources.FoundFeeds, GetList(pattern));
            return 0;
        }

        /// <summary>
        /// Build a list of all feed cache entries.
        /// </summary>
        /// <param name="pattern">Only return feeds that contain this substring; <see langword="null"/> to return all.</param>
        private string GetList(string pattern)
        {
            var builder = new StringBuilder();
            var feeds = Policy.FeedManager.Cache.ListAll();
            foreach (string entry in feeds.Where(entry => pattern == null || entry.Contains(pattern)))
                builder.AppendLine(entry);
            return (builder.Length == 0 ? "" : builder.ToString(0, builder.Length - Environment.NewLine.Length)); // Remove trailing line-break
        }
        #endregion
    }
}
