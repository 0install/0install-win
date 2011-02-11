/*
 * Copyright 2010-2011 Bastian Eicher
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
    public sealed class List : CommandBase
    {
        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return "list"; } }

        /// <inheritdoc/>
        public override string Description { get { return Resources.DescriptionList; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[PATTERN]"; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public List(IHandler handler) : base(handler)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            // Allow 0 or 1 arguments
            string pattern;
            switch (AdditionalArgs.Count)
            {
                case 0: pattern = null; break;
                case 1: pattern = AdditionalArgs.First; break;
                default: throw new OptionException(Resources.TooManyArguments, Name);
            }

            ExecuteHelper();

            // Build a list of all feed cache entries
            var builder = new StringBuilder();
	        var feeds = Policy.FeedManager.Cache.ListAll();
	        foreach (Uri entry in feeds)
	        {
                if (pattern == null || entry.ToString().Contains(pattern))
                    builder.AppendLine(entry.ToString());
	        }
            builder.Remove(builder.Length - 1, 1); // Remove trailing line-break
            Handler.Inform(Resources.FoundFeeds, builder.ToString());

            return 0;
        }
        #endregion
    }
}
