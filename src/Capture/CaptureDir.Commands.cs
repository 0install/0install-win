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
using Common;
using Microsoft.Win32;
using ZeroInstall.Capture.Properties;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.Capture
{
    public partial class CaptureDir
    {
        /// <summary>
        /// Locates the directory into which the new application was installed.
        /// </summary>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        private static string GetInstallationDir(Snapshot snapshotDiff)
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
                Log.Info(string.Format(Resources.UsingInstallationDir, installationDir));
            }
            return installationDir;
        }

        /// <summary>
        /// Detects all EXE files within the installation directory and returns them as <see cref="Command"/>s.
        /// </summary>
        /// <param name="installationDir">The fully qualified path to the installation directory; may be <see langword="null"/>.</param>
        private static IEnumerable<Command> GetCommands(string installationDir)
        {
            if (installationDir == null) return new Command[0];
            installationDir = Path.GetFullPath(installationDir);

            bool firstExe = true;
            var commands = new C5.LinkedList<Command>();
            foreach (string absolutePath in Directory.GetFiles(installationDir, "*.exe", SearchOption.AllDirectories))
            {
                // Ignore uninstallers
                if (absolutePath.Contains("uninstall") || absolutePath.Contains("unins0")) continue;

                // Cut away installation directory plus trailing slash
                string relativePath = absolutePath.Substring(installationDir.Length + 1);

                // Assume first detected EXE is the main entry point
                // ToDo: Better heuristic
                commands.Add(new Command { Name = firstExe ? "run" : relativePath.Replace(".exe", "").Replace(Path.DirectorySeparatorChar, '.'), Path = relativePath });
                firstExe = false;
            }
            return commands;
        }

        /// <summary>
        /// Retreives data about a verb (an executable command) from the registry.
        /// </summary>
        /// <param name="progID">The programatic indentifier the verb is associated with.</param>
        /// <param name="verb">The internal name of the verb.</param>
        /// <param name="commandProvider">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        private static Verb GetVerb(string progID, string verb, CommandProvider commandProvider)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(progID)) throw new ArgumentNullException("progID");
            if (string.IsNullOrEmpty(verb)) throw new ArgumentNullException("verb");
            if (commandProvider == null) throw new ArgumentNullException("commandProvider");
            #endregion

            using (var verbKey = Registry.ClassesRoot.OpenSubKey(progID + @"\shell\" + verb))
            {
                if (verbKey == null) return default(Verb);

                string description = verbKey.GetValue("", "").ToString();
                string commandLine;
                using (var commandKey = verbKey.OpenSubKey("command"))
                {
                    if (commandKey == null) return default(Verb);
                    commandLine = commandKey.GetValue("", "").ToString();
                }

                string additionalArgs;
                var command = commandProvider.GetCommand(commandLine, out additionalArgs);
                if (command == null) return default(Verb);
                return new Verb
                {
                    Name = verb,
                    Description = description,
                    Command = command.Name,
                    Arguments = additionalArgs
                };
            }
        }
    }
}
