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

using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.ViewModel
{
    /// <summary>
    /// Wraps a <see cref="DefaultProgram"/> for data binding.
    /// </summary>
    public class DefaultProgramModel : IconCapabilityModel
    {
        private readonly DefaultProgram _defaultProgram;

        /// <summary>
        /// Returns <see cref="DefaultProgram.Service"/>.
        /// </summary>
        public string Service { get { return _defaultProgram.Service; } }

        /// <inheritdoc/>
        public DefaultProgramModel(DefaultProgram capability, bool used) : base(capability, used)
        {
            _defaultProgram = capability;
        }
    }
}
