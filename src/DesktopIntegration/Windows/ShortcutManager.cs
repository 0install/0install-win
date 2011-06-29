/*
 * Copyright 2010-2011 Bastian Eicher
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

#if !MONO
using System;
using System.Collections.Generic;
using System.Globalization;
using Common.Tasks;
using Common.Utils;
using IWshRuntimeLibrary;
using ZeroInstall.Model;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Utility class for creating and modifying Windows shortcut files (.lnk).
    /// </summary>
    public static class ShortcutManager
    {
        private static readonly WshShellClass _wshShell = new WshShellClass();

        /// <summary>
        /// Creates a new Windows shortcut.
        /// </summary>
        /// <param name="path">The location to place the shorcut at.</param>
        /// <param name="target">The target the shortcut shall point to.</param>
        /// <param name="command"></param>
        /// <param name="systemWide"></param>
        /// <param name="handler"></param>
        public static void CreateShortcut(string path, InterfaceFeed target, string command, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            var shortcut = (IWshShortcut)_wshShell.CreateShortcut(path);
            shortcut.TargetPath = target.Feed.NeedsTerminal ? "0install.exe" : "0install-win.exe";

            string arguments = "run ";
            if (!string.IsNullOrEmpty(command)) arguments += " --command=" + StringUtils.EscapeWhitespace(command);
            arguments += " " + StringUtils.EscapeWhitespace(target.InterfaceID);
            shortcut.Arguments = arguments;
            shortcut.Description = target.Feed.GetDescription(CultureInfo.CurrentCulture, command);

            // Set icon if available
            try
            {
                var icon = target.Feed.GetIcon(Icon.MimeTypeIco, command);
                shortcut.IconLocation = IconProvider.GetIconPath(icon, systemWide, handler);
            }
            catch (KeyNotFoundException) { }

            shortcut.Save();
        }
    }
}
#endif
