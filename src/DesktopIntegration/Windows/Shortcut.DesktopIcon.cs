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
using System.IO;
using Common.Tasks;
using Microsoft.Win32;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;

namespace ZeroInstall.DesktopIntegration.Windows
{
    public static partial class Shortcut
    {
        public static void Create(DesktopIcon desktopIcon, InterfaceFeed target, bool machineWide, ITaskHandler handler)
        {
            string filePath = GetDesktopPath(desktopIcon.Name, machineWide);
            CreateShortcut(filePath, target, desktopIcon.Command, machineWide, handler);
        }

        public static void Remove(DesktopIcon desktopIcon, bool machineWide)
        {
            string filePath = GetDesktopPath(desktopIcon.Name, machineWide);
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        private static string GetDesktopPath(string name, bool machineWide)
        {
            if (String.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                throw new IOException(String.Format(Resources.NameInvalidChars, name));

            string desktopDir = machineWide
                ? Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "Common Desktop", "").ToString()
                : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            return Path.Combine(desktopDir, name + ".lnk");
        }
    }
}
