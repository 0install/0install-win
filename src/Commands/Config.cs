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
    /// View or change <see cref="Preferences"/>.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class Config : CommandBase
    {
        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return "config"; } }

        /// <inheritdoc/>
        public override string Description { get { return "View or change configuration settings. With no arguments, '0install config' displays all configuration settings. With one argument, it displays the current value of the named setting. With two arguments, it sets the setting to the given value."; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[NAME [VALUE]]"; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Config(IHandler handler) : base(handler)
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
