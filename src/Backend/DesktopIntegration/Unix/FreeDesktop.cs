/*
 * Copyright 2010-2014 Bastian Eicher
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
using NanoByte.Common.Tasks;
using ZeroInstall.DesktopIntegration.AccessPoints;

namespace ZeroInstall.DesktopIntegration.Unix
{
    /// <summary>
    /// Utility class for creating and modifying FreeDesktop.org Desktop Entries.
    /// </summary>
    public static class FreeDesktop
    {
        public static void Create(MenuEntry menuEntry, InterfaceFeed target, bool machineWide, ITaskHandler handler)
        {
            throw new NotImplementedException();
        }

        public static void Remove(MenuEntry menuEntry, bool machineWide)
        {
            throw new NotImplementedException();
        }

        public static void Create(DesktopIcon desktopIcon, InterfaceFeed target, bool machineWide, ITaskHandler handler)
        {
            throw new NotImplementedException();
        }

        public static void Remove(DesktopIcon desktopIcon, bool machineWide)
        {
            throw new NotImplementedException();
        }
    }
}
