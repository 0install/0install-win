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

using System.ComponentModel;

namespace ZeroInstall.Commands.WinForms.AccessPointModels
{
    internal abstract class AccessPointModel
    {
        #region Variables
        /// <summary>
        /// Stores whether the <see cref="CapabilityModel.Capability" /> was already used or not.
        /// </summary>
        private readonly bool _wasUsed;

        /// <summary>
        /// Indicates whether the <see cref="CapabilityModel.Capability" /> shall be used or not.
        /// </summary>
        // ReSharper disable MemberCanBePrivate.Global
        public bool Use { get; set; }

        /// <summary>
        /// Indicates whether the <see cref="Use" /> of the <see cref="CapabilityModel.Capability" /> has been changed.
        /// </summary>
        [Browsable(false)]
        public bool Changed { get { return _wasUsed != Use; } }
        #endregion

        protected AccessPointModel(bool used)
        {
            _wasUsed = Use = used;
        }
    }
}