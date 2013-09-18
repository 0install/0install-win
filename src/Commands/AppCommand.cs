/*
 * Copyright 2010-2013 Bastian Eicher
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
using ZeroInstall.Backend;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Common base class for commands that manage an <see cref="AppList"/>.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class AppCommand : IntegrationCommand
    {
        #region Properties
        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionAppCommand; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMin { get { return 1; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 1; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        protected AppCommand(Resolver resolver) : base(resolver)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            Resolver.Handler.ShowProgressUI();
            string interfaceID = GetCanonicalID(AdditionalArgs[0]);
            using (var integrationManager = new CategoryIntegrationManager(Resolver.Handler, MachineWide))
                return ExecuteHelper(integrationManager, interfaceID);
        }

        /// <summary>
        /// Template method that performs the actual operation.
        /// </summary>
        /// <param name="integrationManager">Manages desktop integration operations.</param>
        /// <param name="interfaceID">The interface for the application to perform the operation on.</param>
        /// <returns>The exit status code to end the process with. 0 means OK, 1 means generic error.</returns>
        protected abstract int ExecuteHelper(ICategoryIntegrationManager integrationManager, string interfaceID);
        #endregion
    }
}
