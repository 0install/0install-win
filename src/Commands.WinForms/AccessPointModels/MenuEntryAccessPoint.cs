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

using ZeroInstall.DesktopIntegration.AccessPoints;

namespace ZeroInstall.Commands.WinForms.AccessPointModels
{
    class MenuEntryAccessPointModel : AccessPointModel
    {
        private readonly MenuEntry _menuEntry;

        public string Name {get { return _menuEntry.Name; } set { _menuEntry.Name = value; }}

        public string Category { get { return _menuEntry.Category; } set { _menuEntry.Category = value; }}

        public string Command { get { return _menuEntry.Command; } }

        public MenuEntryAccessPointModel(MenuEntry entry, bool used) : base(used)
        {
            _menuEntry = entry;
        }
    }
}
