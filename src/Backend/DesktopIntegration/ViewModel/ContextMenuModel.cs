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
    /// Wraps a <see cref="ContextMenu"/> for data binding.
    /// </summary>
    public class ContextMenuModel : CapabilityModel
    {
        private readonly ContextMenu _contextMenu;

        /// <summary>
        /// The name of the stored <see cref="ContextMenu.Verb"/>.
        /// </summary>
        public string Name { get { return _contextMenu.Verb.Descriptions.GetBestLanguage(CultureInfo.CurrentUICulture) ?? _contextMenu.Verb.Name; } }

        /// <inheritdoc/>
        public ContextMenuModel(ContextMenu contextMenu, bool used) : base(contextMenu, used)
        {
            _contextMenu = contextMenu;
        }
    }
}
