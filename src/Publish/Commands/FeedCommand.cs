/*
 * Copyright 2010-2012 Bastian Eicher
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

using Common.Undo;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.Commands
{
    /// <summary>
    /// An undo command that modifies a <see cref="Feed"/>.
    /// </summary>
    public abstract class FeedCommand : SimpleCommand
    {
        #region Variables
        /// <summary>
        /// The <see cref="Feed"/> to be modified.
        /// </summary>
        protected readonly Feed Feed;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new feed command.
        /// </summary>
        /// <param name="feed">The <see cref="Feed"/> to be modified.</param>
        protected FeedCommand(Feed feed)
        {
            Feed = feed;
        }
        #endregion
    }
}
