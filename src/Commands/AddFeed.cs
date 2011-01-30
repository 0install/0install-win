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
using ZeroInstall.Injector;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Register an additional source of implementations (versions) of a program. 
    /// </summary>
    [CLSCompliant(false)]
    public class AddFeed : ManageFeeds
    {
        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return "add-feed"; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public AddFeed(IHandler handler) : base(handler)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override void Execute()
        {
            base.Execute();

            throw new NotImplementedException();
        }
        #endregion
    }
}
