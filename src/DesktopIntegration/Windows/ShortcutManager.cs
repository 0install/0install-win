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
//using IWshRuntimeLibrary;

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
//        /// <param name="icon">The path of the icon for the shortcut; may be <see langword="null"/>.</param>
//        /// <param name="description">The shortcut description text; may be <see langword="null"/>.</param>
//        public static void CreateShortcut(string path, string target, string icon, string description)
//        {
//            #region Sanity checks
//            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
//            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
//            #endregion

//            var shortcut = (IWshShortcut)_wshShell.CreateShortcut(target);
//            shortcut.TargetPath = target;
//            if (description != null) shortcut.Description = description;
//            if (!string.IsNullOrEmpty(icon)) shortcut.IconLocation = icon;
//            shortcut.Save();
//        }
//    }
//}

