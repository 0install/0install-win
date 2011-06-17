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
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Remove an application from the application list and undoes any desktop environment integration.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class RemoveApp : AppCommand
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "remove-app";
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionRemoveApp; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public RemoveApp(Policy policy) : base(policy)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        protected override int ExecuteHelper(string interfaceID, CategoryIntegrationManager integrationManager)
        {
            integrationManager.RemoveApp(interfaceID);

            // Show a "done" message (but not in batch mode, since it is too unimportant)
            if (!Policy.Handler.Batch) Policy.Handler.Output(Resources.AppList, string.Format(Resources.AppListRemoved, interfaceID));
            return 0;
        }
        #endregion
    }
}
