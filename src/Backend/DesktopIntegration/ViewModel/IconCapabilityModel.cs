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

using System.Globalization;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.ViewModel
{
    /// <summary>
    /// Wraps an <see cref="IconCapability"/> for data binding.
    /// </summary>
    public class IconCapabilityModel : CapabilityModel
    {
        private readonly IconCapability _iconCapability;

        /// <summary>
        /// Returns the description of the <see cref="IconCapability" /> dependend on <see cref="CultureInfo.CurrentUICulture" />.
        /// </summary>
        public string Description { get { return _iconCapability.Descriptions.GetBestLanguage(CultureInfo.CurrentUICulture) ?? _iconCapability.ID; } }

        /// <inheritdoc/>
        protected IconCapabilityModel(IconCapability capability, bool used) : base(capability, used)
        {
            _iconCapability = capability;
        }
    }
}
