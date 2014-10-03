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
using System.Globalization;
using System.IO;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Creates Windows shortcut files (.lnk).
    /// </summary>
    public static partial class Shortcut
    {
        /// <summary>
        /// Creates a new Windows shortcut.
        /// </summary>
        /// <param name="path">The location to place the shorcut at.</param>
        /// <param name="target">The target the shortcut shall point to.</param>
        /// <param name="command">The command within <paramref name="target"/> the shorcut shall point to; may be <see langword="null"/>.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="machineWide">Create the shortcut machine-wide instead of just for the current user.</param>
        public static void Create(string path, InterfaceFeed target, string command, ITaskHandler handler, bool machineWide = false)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            if (string.IsNullOrEmpty(command)) command = Command.NameRun;

#if !__MonoCS__
            if (File.Exists(path)) File.Delete(path);

            var wshShell = new IWshRuntimeLibrary.WshShellClass();
            var shortcut = (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(path);

            var entryPoint = target.Feed.GetEntryPoint(command);
            bool needsTerminal = target.Feed.NeedsTerminal || (entryPoint != null && entryPoint.NeedsTerminal);
            shortcut.TargetPath = Path.Combine(Locations.InstallBase, needsTerminal ? "0install.exe" : "0install-win.exe");

            string arguments = "run ";
            if (!needsTerminal) arguments += "--no-wait ";
            if (command != Command.NameRun) arguments += "--command=" + command.EscapeArgument() + " ";
            arguments += target.InterfaceID.EscapeArgument();
            shortcut.Arguments = arguments;

            // .lnk descriptions may not be longer than 260 characters
            const int maxDescriptionLength = 256;
            string description = target.Feed.GetBestSummary(CultureInfo.CurrentUICulture, command);
            shortcut.Description = description.Substring(0, Math.Min(description.Length, maxDescriptionLength));

            // Set icon if available
            var icon = target.Feed.GetIcon(Icon.MimeTypeIco, command);
            if (icon != null) shortcut.IconLocation = IconProvider.GetIconPath(icon, handler, machineWide);

            shortcut.Save();
#endif
        }
    }
}
