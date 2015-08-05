/*
 * Copyright 2010-2015 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Store;
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
        /// <param name="command">The command within <paramref name="target"/> the shorcut shall point to; can be <see langword="null"/>.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="machineWide">Create the shortcut machine-wide instead of just for the current user.</param>
        private static void Create([NotNull] string path, FeedTarget target, [CanBeNull] string command, [NotNull] ITaskHandler handler, bool machineWide)
        {
            if (string.IsNullOrEmpty(command)) command = Command.NameRun;

            var entryPoint = target.Feed.GetEntryPoint(command);
            bool needsTerminal = (entryPoint != null && entryPoint.NeedsTerminal);

            string arguments = "run ";
            if (!needsTerminal) arguments += "--no-wait ";
            if (command != Command.NameRun) arguments += "--command " + command.EscapeArgument() + " ";
            arguments += target.Uri.ToStringRfc().EscapeArgument();

            var icon = target.Feed.GetIcon(Icon.MimeTypeIco, command);

            Create(path,
                targetPath: Path.Combine(Locations.InstallBase, needsTerminal ? "0install.exe" : "0install-win.exe"),
                arguments: arguments,
                iconLocation: (icon == null) ? null : IconProvider.GetIconPath(icon, handler, machineWide),
                description: target.Feed.GetBestSummary(CultureInfo.CurrentUICulture, command));
        }

        /// <summary>
        /// Creates a new Windows shortcut.
        /// </summary>
        private static void Create([NotNull] string path, [NotNull] string targetPath, [CanBeNull] string arguments = null, [CanBeNull] string iconLocation = null, [CanBeNull] string description = null)
        {
#if !__MonoCS__
            if (File.Exists(path)) File.Delete(path);

            var wshShell = new IWshRuntimeLibrary.WshShellClass();
            var shortcut = (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(path);

            shortcut.TargetPath = targetPath;
            if (!string.IsNullOrEmpty(arguments)) shortcut.Arguments = arguments;
            if (!string.IsNullOrEmpty(iconLocation)) shortcut.IconLocation = iconLocation;
            if (!string.IsNullOrEmpty(description)) shortcut.Description = description.Substring(0, Math.Min(description.Length, 256));

            shortcut.Save();
#endif
        }

        /// <summary>
        /// Ensures that the given name can be used as a file name.
        /// </summary>
        /// <exception cref="IOException"><paramref name="name"/> contains invalid characters.</exception>
        private static void CheckName([CanBeNull] string name)
        {
            if (string.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                throw new IOException(string.Format(Resources.NameInvalidChars, name));
        }
    }
}
