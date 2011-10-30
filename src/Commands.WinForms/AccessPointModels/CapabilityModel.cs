/*
 * Copyright 2011 Simon E. Silva Lauinger
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
using System.ComponentModel;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.Commands.WinForms.AccessPointModels
{
    /// <summary>
    /// Intermediate wrapper to present <see cref="DefaultCapability"/>s to the user interface.
    /// </summary>
    internal abstract class CapabilityModel : AccessPointModel
    {
        #region Properties
        /// <summary>
        /// The wrapped <see cref="Capability" />.
        /// </summary>
        [Browsable(false)]
        public DefaultCapability Capability { get; private set; }
        #endregion

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="capability">That shall be wrapped.</param>
        /// <param name="used">Indicates whether the <see cref="Capability" /> was already used.</param>
        protected CapabilityModel(DefaultCapability capability, bool used) : base(used)
        {
            #region Sanity Checks
            if (capability == null) throw new ArgumentNullException("capability");
            #endregion

            Capability = capability;
        }
    }
}
