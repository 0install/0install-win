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
using System.Linq;
using Common.Tasks;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;

namespace ZeroInstall.DesktopIntegration.Windows
{
    public static partial class Shortcut
    {
        public static void Create(QuickLaunch quickLaunch, InterfaceFeed target, ITaskHandler handler)
        {
            string filePath = GetQuickLaunchPath(quickLaunch.Name);
            CreateShortcut(filePath, target, quickLaunch.Command, false, handler);
        }

        public static void Remove(SendTo sendTo)
        {
            string filePath = GetSendToPath(sendTo.Name);
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        private static string GetQuickLaunchPath(string name)
        {
            if (string.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                throw new IOException(string.Format(Resources.NameInvalidChars, name));

            return new[] { Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Internet Explorer", "Quick Launch", name + ".lnk" }.Aggregate(Path.Combine);
        }
    }
}
