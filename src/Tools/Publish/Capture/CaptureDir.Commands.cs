/*
 * Copyright 2011 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as Captureed by
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Win32;
using NanoByte.Common;
using NanoByte.Common.Collections;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.Publish.Capture
{
    public partial class CaptureDir
    {
        /// <summary>
        /// Locates the directory into which the new application was installed.
        /// </summary>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        [CanBeNull]
        private static string GetInstallationDir([NotNull] Snapshot snapshotDiff)
        {
            #region Sanity checks
            if (snapshotDiff == null) throw new ArgumentNullException("snapshotDiff");
            #endregion

            string installationDir;
            if (snapshotDiff.ProgramsDirs.Length == 0)
            {
                Log.Warn(Resources.NoInstallationDirDetected);
                installationDir = null;
            }
            else
            {
                if (snapshotDiff.ProgramsDirs.Length > 1)
                    Log.Warn(Resources.MultipleInstallationDirsDetected);

                installationDir = snapshotDiff.ProgramsDirs[0];
                Log.Info(string.Format(Resources.InstallationDirDetected, installationDir));
            }
            return installationDir;
        }

        /// <summary>
        /// Detects all EXE files within the installation directory and returns them as <see cref="Command"/>s.
        /// </summary>
        /// <param name="installationDir">The fully qualified path to the installation directory; leave <see langword="null"/> for auto-detection.</param>
        /// <param name="mainExe">The relative path to the main EXE; leave <see langword="null"/> for auto-detection.</param>
        [NotNull, ItemNotNull]
        private static IEnumerable<Command> GetCommands([NotNull] string installationDir, [CanBeNull] string mainExe)
        {
            installationDir = Path.GetFullPath(installationDir);

            bool isFirstExe = true;
            var commands = new List<Command>();
            foreach (string absolutePath in Directory.GetFiles(installationDir, "*.exe", SearchOption.AllDirectories))
            {
                // Ignore uninstallers
                if (absolutePath.ContainsIgnoreCase("uninstall") || absolutePath.ContainsIgnoreCase("unins0")) continue;

                // Cut away installation directory plus trailing slash
                string relativePath = absolutePath.Substring(installationDir.Length + 1);

                // Assume first detected EXE is the main entry point if not specified explicitly
                string name = (isFirstExe && (mainExe == null)) || StringUtils.EqualsIgnoreCase(relativePath, mainExe)
                    ? Command.NameRun
                    : relativePath.Replace(".exe", "").Replace(Path.DirectorySeparatorChar, '.');
                commands.Add(new Command {Name = name, Path = relativePath.Replace(Path.DirectorySeparatorChar, '/')});
                isFirstExe = false;
            }
            return commands;
        }

        /// <summary>
        /// Retrieves data about multiple verbs (executable commands) from the registry.
        /// </summary>
        /// <param name="typeKey">The registry key containing information about the file type / protocol the verbs belong to.</param>
        /// <param name="commandMapper">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <returns>A list of detected <see cref="Verb"/>.</returns>
        [NotNull, ItemNotNull]
        private static IEnumerable<Verb> GetVerbs([NotNull] RegistryKey typeKey, [NotNull] CommandMapper commandMapper)
        {
            #region Sanity checks
            if (typeKey == null) throw new ArgumentNullException("typeKey");
            if (commandMapper == null) throw new ArgumentNullException("commandMapper");
            #endregion

            return RegUtils.GetSubKeyNames(typeKey, "shell").
                Select(verbName => GetVerb(typeKey, commandMapper, verbName)).WhereNotNull();
        }

        /// <summary>
        /// Retrieves data about a verb (an executable command) from the registry.
        /// </summary>
        /// <param name="typeKey">The registry key containing information about the file type / protocol the verb belongs to.</param>
        /// <param name="commandMapper">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <param name="verbName">The internal name of the verb.</param>
        /// <returns>The detected <see cref="Verb"/> or an empty <see cref="Verb"/> if no match was found.</returns>
        [CanBeNull]
        private static Verb GetVerb([NotNull] RegistryKey typeKey, [NotNull] CommandMapper commandMapper, [NotNull] string verbName)
        {
            #region Sanity checks
            if (typeKey == null) throw new ArgumentNullException("typeKey");
            if (string.IsNullOrEmpty(verbName)) throw new ArgumentNullException("verbName");
            if (commandMapper == null) throw new ArgumentNullException("commandMapper");
            #endregion

            using (var verbKey = typeKey.OpenSubKey(@"shell\" + verbName))
            {
                if (verbKey == null) return null;

                string description = verbKey.GetValue("", "").ToString();
                string commandLine;
                using (var commandKey = verbKey.OpenSubKey("command"))
                {
                    if (commandKey == null) return null;
                    commandLine = commandKey.GetValue("", "").ToString();
                }

                string additionalArgs;
                var command = commandMapper.GetCommand(commandLine, out additionalArgs);
                if (command == null) return null;
                string commandName = command.Name;

                if (commandName == Command.NameRun) commandName = null;
                var verb = new Verb
                {
                    Name = verbName,
                    Command = commandName,
                    Arguments = additionalArgs
                };
                if (!string.IsNullOrEmpty(description)) verb.Descriptions.Add(description);
                return verb;
            }
        }
    }
}
