// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NDesk.Options;
using ZeroInstall.Commands.CliCommands;
using ZeroInstall.Commands.Properties;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Handles the creation of <see cref="CliCommand"/> instances for handling of commands like "0install COMMAND [OPTIONS]".
    /// </summary>
    public static class CommandFactory
    {
        #region Command list
        /// <summary>
        /// A list of command names (without alternatives) as used in command-line arguments in lower-case.
        /// </summary>
        internal static readonly string[] CommandNames = {Central.Name, SelfUpdate.Name, Selection.Name, Download.Name, Update.Name, Run.Name, Import.Name, Export.Name, Search.Name, List.Name, CatalogMan.Name, Configure.Name, AddFeed.Name, RemoveFeed.Name, ListFeeds.Name, AddApp.Name, RemoveApp.Name, RemoveAllApps.Name, IntegrateApp.Name, AddAlias.Name, ListApps.Name, UpdateApps.Name, RepairApps.Name, SyncApps.Name, ImportApps.Name, Digest.Name, StoreMan.Name, MaintenanceMan.Name};

        /// <summary>
        /// Creates a new <see cref="CliCommand"/> based on a name.
        /// </summary>
        /// <param name="commandName">The command name to look for; case-insensitive; can be <c>null</c>.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        /// <returns>The requested <see cref="CliCommand"/> or <see cref="DefaultCommand"/> if <paramref name="commandName"/> was <c>null</c>.</returns>
        /// <exception cref="OptionException"><paramref name="commandName"/> is an unknown command.</exception>
        /// <exception cref="IOException">There was a problem accessing a configuration file or one of the stores.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file or one of the stores was not permitted.</exception>
        /// <exception cref="InvalidDataException">A configuration file is damaged.</exception>
        [NotNull]
        public static CliCommand GetCommand([CanBeNull] string commandName, [NotNull] ICommandHandler handler)
        {
            if (string.IsNullOrEmpty(commandName)) return new DefaultCommand(handler);
            switch (commandName.ToLowerInvariant())
            {
                case ExportHelp.Name:
                    return new ExportHelp(handler);
                case Central.Name:
                    return new Central(handler);
                case SelfUpdate.Name:
                    return new SelfUpdate(handler);
                case Selection.Name:
                    return new Selection(handler);
                case Download.Name:
                    return new Download(handler);
                case Fetch.Name:
                    return new Fetch(handler);
                case Update.Name:
                    return new Update(handler);
                case Run.Name:
                    return new Run(handler);
                case Import.Name:
                    return new Import(handler);
                case Export.Name:
                    return new Export(handler);
                case Search.Name:
                    return new Search(handler);
                case List.Name:
                    return new List(handler);
                case CatalogMan.Name:
                    return new CatalogMan(handler);
                case Configure.Name:
                    return new Configure(handler);
                case AddFeed.Name:
                    return new AddFeed(handler);
                case RemoveFeed.Name:
                    return new RemoveFeed(handler);
                case ListFeeds.Name:
                    return new ListFeeds(handler);
                case AddApp.Name:
                case AddApp.AltName:
                    return new AddApp(handler);
                case RemoveApp.Name:
                case RemoveApp.AltName:
                case RemoveApp.AltName2:
                    return new RemoveApp(handler);
                case RemoveAllApps.Name:
                case RemoveAllApps.AltName:
                    return new RemoveAllApps(handler);
                case IntegrateApp.Name:
                case IntegrateApp.AltName:
                case IntegrateApp.AltName2:
                    return new IntegrateApp(handler);
                case AddAlias.Name:
                case AddAlias.AltName:
                    return new AddAlias(handler);
                case ListApps.Name:
                    return new ListApps(handler);
                case UpdateApps.Name:
                case UpdateApps.AltName:
                    return new UpdateApps(handler);
                case RepairApps.Name:
                case RepairApps.AltName:
                    return new RepairApps(handler);
                case ImportApps.Name:
                    return new ImportApps(handler);
                case SyncApps.Name:
                    return new SyncApps(handler);
                case Digest.Name:
                    return new Digest(handler);
                case StoreMan.Name:
                    return new StoreMan(handler);
                case MaintenanceMan.Name:
                    return new MaintenanceMan(handler);
                default:
                    throw new OptionException(string.Format(Resources.UnknownCommand, commandName), commandName);
            }
        }
        #endregion

        #region Create and parse
        /// <summary>
        /// Parses command-line arguments, automatically creating an appropriate <see cref="CliCommand"/>.
        /// </summary>
        /// <param name="args">The command-line arguments to be parsed.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        /// <returns>The newly created <see cref="CliCommand"/> after <see cref="CliCommand.Parse"/> has been called.</returns>
        /// <exception cref="OperationCanceledException">The user asked to see help information, version information, etc..</exception>
        /// <exception cref="OptionException"><paramref name="args"/> contains unknown options or specified an unknown command.</exception>
        /// <exception cref="IOException">A problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Creating a directory is not permitted.</exception>
        /// <exception cref="InvalidDataException">A configuration file is damaged.</exception>
        /// <exception cref="FormatException">An URI, local path, version number, etc. is invalid.</exception>
        [NotNull]
        public static CliCommand CreateAndParse([NotNull, ItemNotNull] IEnumerable<string> args, [NotNull] ICommandHandler handler)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            var command = GetCommand(GetCommandName(ref args), handler);
            command.Parse(args);
            return command;
        }

        /// <summary>
        /// Determines the command name specified in the command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to search for a command name. If a command is found it is removed from the collection.</param>
        /// <returns>The name of the command that was found or <c>null</c> if none was specified.</returns>
        [CanBeNull]
        public static string GetCommandName([NotNull, ItemNotNull] ref IEnumerable<string> args)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException(nameof(args));
            #endregion

            var arguments = new LinkedList<string>(args);
            string commandName = arguments.FirstOrDefault(argument => !argument.StartsWith("-") && !argument.StartsWith("/"));
            if (commandName != null) arguments.Remove(commandName);

            args = arguments;
            return commandName;
        }
        #endregion
    }
}
