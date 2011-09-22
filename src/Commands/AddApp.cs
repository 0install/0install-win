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
using Common;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Add an application to the application list.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class AddApp : AppCommand
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "add-app";
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionAddApp; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public AddApp(Policy policy) : base(policy)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        protected override int ExecuteHelper(string interfaceID, CategoryIntegrationManager integrationManager)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            #endregion

            var feed = GetFeed(interfaceID);

            try { integrationManager.AddApp(new InterfaceFeed(interfaceID, feed)); }
            catch(InvalidOperationException ex)
            {
                // Show a "nothing to do" message (but not in batch mode, since it is too unimportant));
                if (!Policy.Handler.Batch) Policy.Handler.Output(Resources.AppList, ex.Message);
                return 0;
            }

            // Show a "done" message (but not in batch mode, since it is too unimportant));
            if (!Policy.Handler.Batch) Policy.Handler.Output(Resources.AppList, string.Format(Resources.AppListAdded, feed.Name));
            return 0;
        }
        #endregion
    }
}
