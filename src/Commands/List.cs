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
    public sealed class List : ManageFeeds
    {
        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return "list"; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public List(IHandler handler) : base(handler)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override void Execute()
        {
            base.Execute();

            if (AdditionalArgs.Count != 0) throw new OptionException(Resources.UnknownOption, Name);

            //var feeds = results.Policy.FeedManager.Cache.ListAll();
            //foreach (Uri entry in feeds)
            //{
            //    if (results.Feed == null || entry.ToString().Contains(results.Feed))
            //        Console.WriteLine(entry);
            //}

            throw new NotImplementedException();
        }
        #endregion
    }
}
