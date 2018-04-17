// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Store;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Create an alias for a <see cref="Run"/> command.
    /// </summary>
    public sealed class AddAlias : IntegrationCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "alias";

        /// <summary>The alternative name of this command as used in command-line arguments in lower-case.</summary>
        public const string AltName = "add-alias";

        /// <inheritdoc/>
        public override string Usage => "ALIAS [INTERFACE [COMMAND]]";

        /// <inheritdoc/>
        public override string Description => Resources.DescriptionAddAlias;

        /// <inheritdoc/>
        protected override int AdditionalArgsMin => 1;

        /// <inheritdoc/>
        protected override int AdditionalArgsMax => 3;
        #endregion

        #region State
        private bool _resolve;
        private bool _remove;

        /// <inheritdoc/>
        public AddAlias([NotNull] ICommandHandler handler)
            : base(handler)
        {
            Options.Add("no-download", () => Resources.OptionNoDownload, _ => NoDownload = true);

            Options.Add("resolve", () => Resources.OptionAliasResolve, _ => _resolve = true);
            Options.Add("remove", () => Resources.OptionAliasRemove, _ => _remove = true);
        }
        #endregion

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            if (_resolve || _remove)
            {
                if (AdditionalArgs.Count > 1) throw new OptionException(Resources.TooManyArguments + Environment.NewLine + AdditionalArgs[1].EscapeArgument(), null);
                return ResolveOrRemove(
                    aliasName: AdditionalArgs[0]);
            }
            else
            {
                if (AdditionalArgs.Count < 2 || string.IsNullOrEmpty(AdditionalArgs[1])) throw new OptionException(Resources.MissingArguments, null);
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
        /// <returns>The exit status code to end the process with.</returns>
        private ExitCode ResolveOrRemove(string aliasName)
        {
            using (var integrationManager = new IntegrationManager(Handler, MachineWide))
            {
                var appAlias = GetAppAlias(integrationManager.AppList, aliasName, out var appEntry);
                if (appAlias == null)
                {
                    Handler.Output(Resources.AppAlias, string.Format(Resources.AliasNotFound, aliasName));
                    return ExitCode.InvalidArguments;
                }

                if (_resolve)
                {
                    string result = appEntry.InterfaceUri.ToStringRfc();
                    if (!string.IsNullOrEmpty(appAlias.Command)) result += Environment.NewLine + "Command: " + appAlias.Command;
                    Handler.OutputLow(Resources.AppAlias, result);
                }
                if (_remove)
                {
                    integrationManager.RemoveAccessPoints(appEntry, new AccessPoint[] {appAlias});

                    Handler.OutputLow(Resources.AppAlias, string.Format(Resources.AliasRemoved, aliasName, appEntry.Name));
                }
                return ExitCode.OK;
            }
        }

        /// <summary>
        /// Creates a new alias.
        /// </summary>
        /// <param name="aliasName">The name of the alias to create.</param>
        /// <param name="interfaceUri">The interface URI the alias shall point to.</param>
        /// <param name="command">A command within the interface the alias shall point to; can be <c>null</c>.</param>
        /// <returns>The exit status code to end the process with.</returns>
        private ExitCode CreateAlias(string aliasName, FeedUri interfaceUri, string command = null)
        {
            CheckInstallBase();

            using (var integrationManager = new IntegrationManager(Handler, MachineWide))
            {
                // Check this before modifying the environment
                bool needsReopenTerminal = NeedsReopenTerminal(integrationManager.MachineWide);

                var appEntry = GetAppEntry(integrationManager, ref interfaceUri);

                // Apply the new alias
                var alias = new AppAlias {Name = aliasName, Command = command};
                integrationManager.AddAccessPoints(appEntry, FeedManager[interfaceUri], new AccessPoint[] {alias});

                string message = string.Format(Resources.AliasCreated, aliasName, appEntry.Name);
                if (needsReopenTerminal) message += Environment.NewLine + Resources.ReopenTerminal;
                Handler.OutputLow(Resources.DesktopIntegration, message);
                return ExitCode.OK;
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
            string stubDirPath = DesktopIntegration.Windows.AppAlias.GetStubDir(machineWide);
            var variableTarget = machineWide ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User;
            string existingValue = Environment.GetEnvironmentVariable("Path", variableTarget);
            return existingValue == null || !existingValue.Contains(stubDirPath);
        }

        /// <summary>
        /// Retrieves a specific <see cref="AppAlias"/>.
        /// </summary>
        /// <param name="appList">The list of <see cref="AppEntry"/>s to search.</param>
        /// <param name="aliasName">The name of the alias to search for.</param>
        /// <param name="foundAppEntry">Returns the <see cref="AppEntry"/> containing the found <see cref="AppAlias"/>; <c>null</c> if none was found.</param>
        /// <returns>The first <see cref="AppAlias"/> in <paramref name="appList"/> matching <paramref name="aliasName"/>; <c>null</c> if none was found.</returns>
        [ContractAnnotation("=>null,foundAppEntry:null; =>notnull,foundAppEntry:notnull")]
        internal static AppAlias GetAppAlias([NotNull] AppList appList, [NotNull] string aliasName, out AppEntry foundAppEntry)
        {
            #region Sanity checks
            if (appList == null) throw new ArgumentNullException(nameof(appList));
            if (string.IsNullOrEmpty(aliasName)) throw new ArgumentNullException(nameof(aliasName));
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
