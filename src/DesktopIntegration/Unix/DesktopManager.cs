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
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Model;

namespace ZeroInstall.DesktopIntegration.Unix
{
    /// <summary>
    /// Utility class for creating and modifying FreeDesktop.org Desktop Entries.
    /// </summary>
    public static class DesktopManager
    {
        public static void CreateIcon(DesktopIcon desktopIcon, Feed feed, bool machineWide)
        {
            throw new NotImplementedException();
        }

        public static void RemoveIcon(DesktopIcon desktopIcon, bool machineWide)
        {
            throw new NotImplementedException();
        }

        public static void CreateMenuEntry(MenuEntry menuEntry, Feed feed, bool machineWide)
        {
            throw new NotImplementedException();
        }

        public static void RemoveMenuEntry(MenuEntry menuEntry, bool machineWide)
        {
            throw new NotImplementedException();
        }
    }
}
