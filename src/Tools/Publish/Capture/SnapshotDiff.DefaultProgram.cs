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
using System.IO;
using JetBrains.Annotations;
using Microsoft.Win32;
using NanoByte.Common;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.Publish.Capture
{
    partial class SnapshotDiff
    {
        /// <summary>
        /// Collects data about default programs.
        /// </summary>
        /// <param name="commandMapper">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <param name="appName">Is set to the name of the application as displayed to the user; unchanged if the name was not found.</param>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
        public void CollectDefaultPrograms([NotNull] CommandMapper commandMapper, [NotNull] CapabilityList capabilities, ref string appName)
        {
            #region Sanity checks
            if (capabilities == null) throw new ArgumentNullException(nameof(capabilities));
            if (commandMapper == null) throw new ArgumentNullException(nameof(commandMapper));
            #endregion

            // Ambiguity warnings
            if (ServiceAssocs.Length > 1)
                Log.Warn(Resources.MultipleDefaultProgramsDetected);

            foreach (var serviceAssoc in ServiceAssocs)
            {
                string service = serviceAssoc.Key;
                string client = serviceAssoc.Value;

                using (var clientKey = Registry.LocalMachine.OpenSubKey(DesktopIntegration.Windows.DefaultProgram.RegKeyMachineClients + @"\" + service + @"\" + client))
                {
                    if (clientKey == null) continue;

                    if (string.IsNullOrEmpty(appName)) appName = clientKey.GetValue("", "").ToString();
                    if (string.IsNullOrEmpty(appName)) appName = clientKey.GetValue(DesktopIntegration.Windows.DefaultProgram.RegValueLocalizedName, "").ToString();

                    var defaultProgram = new DefaultProgram
                    {
                        ID = client,
                        Service = service
                    };
                    defaultProgram.Verbs.AddRange(GetVerbs(clientKey, commandMapper));
                    defaultProgram.InstallCommands = GetInstallCommands(clientKey, commandMapper.InstallationDir);
                    capabilities.Entries.Add(defaultProgram);
                }
            }
        }

        #region Install commands
        /// <summary>
        /// Retrieves commands the application registered for use by Windows' "Set Program Access and Defaults".
        /// </summary>
        /// <param name="clientKey">The registry key containing the application's registration data.</param>
        /// <param name="installationDir">The fully qualified path to the installation directory.</param>
        /// <exception cref="IOException">There was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the registry was not permitted.</exception>
        private static InstallCommands GetInstallCommands([NotNull] RegistryKey clientKey, [NotNull] string installationDir)
        {
            #region Sanity checks
            if (clientKey == null) throw new ArgumentNullException(nameof(clientKey));
            if (string.IsNullOrEmpty(installationDir)) throw new ArgumentNullException(nameof(installationDir));
            #endregion

            using (var installInfoKey = clientKey.OpenSubKey(DesktopIntegration.Windows.DefaultProgram.RegSubKeyInstallInfo))
            {
                if (installInfoKey == null) return default(InstallCommands);

                string reinstall = IsolateCommand(installInfoKey.GetValue(DesktopIntegration.Windows.DefaultProgram.RegValueReinstallCommand, "").ToString(), installationDir, out string reinstallArgs);
                string showIcons = IsolateCommand(installInfoKey.GetValue(DesktopIntegration.Windows.DefaultProgram.RegValueShowIconsCommand, "").ToString(), installationDir, out string showIconsArgs);
                string hideIcons = IsolateCommand(installInfoKey.GetValue(DesktopIntegration.Windows.DefaultProgram.RegValueHideIconsCommand, "").ToString(), installationDir, out string hideIconsArgs);

                return new InstallCommands
                {
                    Reinstall = reinstall, ReinstallArgs = reinstallArgs,
                    ShowIcons = showIcons, ShowIconsArgs = showIconsArgs,
                    HideIcons = hideIcons, HideIconsArgs = hideIconsArgs
                };
            }
        }

        /// <summary>
        /// Isolates the path of an executable specified in a command-line relative to a base directory.
        /// </summary>
        /// <param name="commandLine"></param>
        /// <param name="baseDir">The directory relative to which the executable is located.</param>
        /// <param name="additionalArguments">Returns any additional arguments found.</param>
        /// <returns>The path of the executable relative to <paramref name="baseDir"/> without any arguments.</returns>
        private static string IsolateCommand(string commandLine, string baseDir, out string additionalArguments)
        {
            if (!commandLine.StartsWithIgnoreCase("\"" + baseDir + "\\"))
            {
                additionalArguments = null;
                return null;
            }
            commandLine = commandLine.Substring(baseDir.Length + 2);

            additionalArguments = commandLine.GetRightPartAtFirstOccurrence("\" ");
            return commandLine.GetLeftPartAtFirstOccurrence('"');
        }
        #endregion
    }
}
