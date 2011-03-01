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
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Un-register a feed, reversing the effect of <see cref="AddFeed"/>.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class RemoveFeed : ManageFeeds
    {
        #region Variables
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "remove-feed";
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Usage { get { return "FEED"; } }

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionRemoveFeed; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public RemoveFeed(Policy policy) : base(policy)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);

            // ToDo: Implement

            Policy.Handler.Output("Not implemented", "This feature is not implemented yet.");
            return 1;
        }
        #endregion
    }
}
