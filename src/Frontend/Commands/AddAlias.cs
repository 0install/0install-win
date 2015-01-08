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
using System.Linq;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Create an alias for a <see cref="Run"/> command.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class AddAlias : IntegrationCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "alias";

        /// <summary>The alternative name of this command as used in command-line arguments in lower-case.</summary>
        public const string AltName = "add-alias";

        /// <inheritdoc/>
        protected override string Usage { get { return "ALIAS [INTERFACE [COMMAND]]"; } }

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionAddAlias; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMin { get { return 1; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 3; } }

        /// <inheritdoc/>
        public AddAlias(ICommandHandler handler) : base(handler)
        {
            Options.Add("resolve", () => Resources.OptionAliasResolve, _ => _resolve = true);
            Options.Add("remove", () => Resources.OptionAliasRemove, _ => _remove = true);
        }
        #endregion

        #region State
        private bool _resolve;
        private bool _remove;
        #endregion

        /// <inheritdoc/>
        public override int Execute()
        {
            if (_resolve || _remove)
            {
                if (AdditionalArgs.Count > 1) throw new OptionException(Resources.TooManyArguments, "");
                return ResolveOrRemove(
                    aliasName: AdditionalArgs[0]);
            }
            else
            {
                if (AdditionalArgs.Count < 2 || string.IsNullOrEmpty(AdditionalArgs[1])) throw new OptionException(Resources.MissingArguments, "");
                return CreateAlias(
                    aliasName: AdditionalArgs[0],
                    interfaceUri: GetCanonicalUri(AdditionalArgs[1]),
                    command: (AdditionalArgs.Count >= 3) ? AdditionalArgs[2] : null);
            }
        }

        #region Helpers
        /// <summary>
        /// Resolves or removes existing aliases.
        /// </summary>
        /// <param name="aliasName">The name of the existing alias.</param>
        /// <returns>The exit status code to end the process with. 0 means OK, 1 means generic error.</returns>
        private int ResolveOrRemove(string aliasName)
        {
            using (var integrationManager = new IntegrationManager(Handler, MachineWide))
            {
                AppEntry appEntry;
                var appAlias = GetAppAlias(integrationManager.AppList, aliasName, out appEntry);
                if (appAlias == null)
                {
                    Handler.Output(Resources.AppAlias, string.Format(Resources.AliasNotFound, aliasName));
                    return 1;
                }

                if (_resolve)
                {
                    string result = appEntry.InterfaceUri.ToStringRfc();
                    if (!string.IsNullOrEmpty(appAlias.Command)) result += Environment.NewLine + "Command: " + appAlias.Command;
                    Handler.Output(Resources.AppAlias, result);
                }
                if (_remove)
                {
                    integrationManager.RemoveAccessPoints(appEntry, new AccessPoint[] {appAlias});

                    Handler.Output(Resources.AppAlias, string.Format(Resources.AliasRemoved, aliasName, appEntry.Name));
                }
                return 0;
            }
        }

        /// <summary>
        /// Creates a new alias.
        /// </summary>
        /// <param name="aliasName">The name of the alias to create.</param>
        /// <param name="interfaceUri">The interface URI the alias shall point to.</param>
        /// <param name="command">A command within the interface the alias shall point to; may be <see langword="null"/>.</param>
        /// <returns>The exit status code to end the process with. 0 means OK, 1 means generic error.</returns>
        private int CreateAlias(string aliasName, FeedUri interfaceUri, string command = null)
        {
            using (var integrationManager = new IntegrationManager(Handler, MachineWide))
            {
                // Check this before modifying the environment
                bool needsReopenTerminal = NeedsReopenTerminal(integrationManager.MachineWide);

                var appEntry = GetAppEntry(integrationManager, ref interfaceUri);

                // Apply the new alias
                var alias = new AppAlias {Name = aliasName, Command = command};
                try
                {
                    integrationManager.AddAccessPoints(appEntry, FeedManager.GetFeedFresh(interfaceUri), new AccessPoint[] {alias});
                }
                catch (ConflictException ex)
                {
                    Log.Warn(ex.Message);
                    return 1;
                }

                Handler.OutputLow(
                    Resources.DesktopIntegration,
                    string.Format(needsReopenTerminal ? Resources.AliasCreatedReopenTerminal : Resources.AliasCreated, aliasName, appEntry.Name));
                return 0;
            }
        }

        /// <summary>
        /// Determines whether the user may need to reopen the terminal to be able to use newly created aliases.
        /// </summary>
        private static bool NeedsReopenTerminal(bool machineWide)
        {
            // Non-windows terminals may require rehashing to find new aliases
            if (!WindowsUtils.IsWindows) return true;

            // If the default alias directory is already in the PATH terminals will find new aliases right away
            string stubDirPath = Locations.GetIntegrationDirPath("0install.net", machineWide, "desktop-integration", "aliases");
            var variableTarget = machineWide ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User;
            string existingValue = Environment.GetEnvironmentVariable("PATH", variableTarget);
            return existingValue == null || !existingValue.Contains(stubDirPath);
        }

        /// <summary>
        /// Retrieves a specific <see cref="AppAlias"/>.
        /// </summary>
        /// <param name="appList">The list of <see cref="AppEntry"/>s to search.</param>
        /// <param name="aliasName">The name of the alias to search for.</param>
        /// <param name="foundAppEntry">Returns the <see cref="AppEntry"/> containing the found <see cref="AppAlias"/>; <see langword="null"/> if none was found.</param>
        /// <returns>The first <see cref="AppAlias"/> in <paramref name="appList"/> matching <paramref name="aliasName"/>; <see langword="null"/> if none was found.</returns>
        internal static AppAlias GetAppAlias(AppList appList, string aliasName, out AppEntry foundAppEntry)
        {
            #region Sanity checks
            if (appList == null) throw new ArgumentNullException("appList");
            if (string.IsNullOrEmpty(aliasName)) throw new ArgumentNullException("aliasName");
            #endregion

            var results =
                from entry in appList.Entries
                where entry.AccessPoints != null
                from alias in entry.AccessPoints.Entries.OfType<AppAlias>()
                where alias.Name == aliasName
                select new {entry, alias};

            var result = results.FirstOrDefault();
            if (result == null)
            {
                foundAppEntry = null;
                return null;
            }
            else
            {
                foundAppEntry = result.entry;
                return result.alias;
            }
        }
        #endregion
    }
}
