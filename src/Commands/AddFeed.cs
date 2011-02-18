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
    /// Register an additional source of implementations (versions) of a program. 
    /// </summary>
    [CLSCompliant(false)]
    public sealed class AddFeed : ManageFeeds
    {
        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return "add-feed"; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "NEW-FEED"; } }

        /// <inheritdoc/>
        public override string Description { get { return Resources.DescriptionAddFeed; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public AddFeed(IHandler handler, Policy policy) : base(handler, policy)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            ExecuteHelper();

            // ToDo: Implement

            Handler.Inform("Not implemented", "This feature is not implemented yet.");
            return 1;
        }
        #endregion
    }
}
