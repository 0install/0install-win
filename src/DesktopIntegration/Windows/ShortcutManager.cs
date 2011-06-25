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

//using System;
//using System.Globalization;
//using Common.Tasks;
//using Common.Utils;
//using IWshRuntimeLibrary;
//using ZeroInstall.Model;

//namespace ZeroInstall.DesktopIntegration.Windows
//{
//    /// <summary>
//    /// Utility class for creating and modifying Windows shortcut files (.lnk).
//    /// </summary>
//    public static class ShortcutManager
//    {
//        private static readonly WshShellClass _wshShell = new WshShellClass();

//        /// <summary>
//        /// Creates a new Windows shortcut.
//        /// </summary>
//        /// <param name="path">The location to place the shorcut at.</param>
//        /// <param name="target">The target the shortcut shall point to.</param>
//        /// <param name="command"></param>
//        /// <param name="systemWide"></param>
//        /// <param name="handler"></param>
//        public static void CreateShortcut(string path, InterfaceFeed target, string command, bool systemWide, ITaskHandler handler)
//        {
//            #region Sanity checks
//            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
//            #endregion

//            var shortcut = (IWshShortcut)_wshShell.CreateShortcut(path);
//            shortcut.TargetPath = target.Feed.NeedsTerminal ? "0install.exe" : "0install-win.exe";

//            string arguments = "run ";
//            if (!string.IsNullOrEmpty(command)) arguments += " --command=" + StringUtils.EscapeWhitespace(command);
//            arguments += " " + target.InterfaceID;
//            shortcut.Arguments = arguments;

//            string description = target.Feed.Descriptions.GetBestLanguage(CultureInfo.CurrentCulture);
//            if (!string.IsNullOrEmpty(description)) shortcut.Description = description;

//            var suitableIcons = target.Feed.Icons.FindAll(icon => icon.MimeType == Icon.MimeTypeIco && icon.Location != null);
//            if (!suitableIcons.IsEmpty) shortcut.IconLocation = IconProvider.GetIcon(suitableIcons.First, systemWide, handler);

//            shortcut.Save();
//        }
//    }
//}

