/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Collections.Generic;
using ZeroInstall.Backend;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Remove an application from the application list and undoes any desktop environment integration.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class RemoveApp : AppCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "remove";

        /// <summary>The alternative name of this command as used in command-line arguments in lower-case.</summary>
        public const string AltName = "remove-app";

        /// <summary>Another alternative name of this command as used in command-line arguments in lower-case.</summary>
        public const string AltName2 = "destory";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionRemoveApp; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] (PET-NAME|INTERFACE)"; } }

        /// <inheritdoc/>
        public override int GuiDelay { get { return 1000; } }

        /// <inheritdoc/>
        public RemoveApp(IBackendHandler handler) : base(handler)
        {}
        #endregion

        /// <inheritdoc/>
        protected override int ExecuteHelper(ICategoryIntegrationManager integrationManager, string interfaceID)
        {
            if (string.IsNullOrEmpty(interfaceID)) throw new ArgumentNullException("interfaceID");
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");

            try
            {
                integrationManager.RemoveApp(integrationManager.AppList[interfaceID]);
            }
            catch (KeyNotFoundException ex)
            {
                Handler.OutputLow(Resources.AppList, ex.Message);
                return 0;
            }

            return 0;
        }
    }
}
