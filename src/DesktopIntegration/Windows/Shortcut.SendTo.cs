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
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;

namespace ZeroInstall.DesktopIntegration.Windows
{
    public static partial class Shortcut
    {
        public static void Create(SendTo sendTo, InterfaceFeed target, ITaskHandler handler)
        {
            string filePath = GetSendToPath(sendTo.Name);
            CreateShortcut(filePath, target, sendTo.Command, false, handler);
        }

        public static void Remove(QuickLaunch quickLaunch)
        {
            string filePath = GetQuickLaunchPath(quickLaunch.Name);
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        private static string GetSendToPath(string name)
        {
            if (string.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                throw new IOException(string.Format(Resources.NameInvalidChars, name));

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SendTo), name + ".lnk");
        }
    }
}
