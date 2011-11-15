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

using Common.Utils;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.Commands.WinForms.CapabilityModels
{
    /// <summary>
    /// Wraps a <see cref="AutoPlay"/> for data binding.
    /// </summary>
    internal class AutoPlayModel : IconCapabilityModel
    {
        private readonly AutoPlay _autoPlay;

        /// <summary>
        /// All <see cref="AutoPlay.Events"/> concatenated with ", ".
        /// </summary>
        public string Events { get { return StringUtils.Concatenate(_autoPlay.Events.Map(ev => ev.Name), ", "); } }

        /// <inheritdoc />
        public AutoPlayModel(AutoPlay capability, bool used) : base(capability, used)
        {
            _autoPlay = capability;
        }
    }
}
