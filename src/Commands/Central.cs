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
using Common.Utils;
using ZeroInstall.Backend;
using ZeroInstall.Commands.Properties;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Opens the central graphical user interface for launching and managing applications.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class Central : FrontendCommand
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "central";
        #endregion

        #region Variables
        private bool _machineWide;
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionCentral; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS]"; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Central(IBackendHandler handler) : base(handler)
        {
            Options.Add("m|machine", () => Resources.OptionMachine, unused => _machineWide = true);
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            ProcessUtils.LaunchAssembly(
                /*MonoUtils.IsUnix ? "ZeroInstall-gtk" :*/ "ZeroInstall",
                _machineWide ? "-m" : null);
            return 0;
        }
        #endregion
    }
}
