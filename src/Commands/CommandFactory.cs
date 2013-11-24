/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NDesk.Options;
using ZeroInstall.Backend;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Handles the creation of <see cref="FrontendCommand"/> instances for handling of commands like "0install COMMAND [OPTIONS]".
    /// </summary>
    [CLSCompliant(false)]
    public static class CommandFactory
    {
        #region Command list
        /// <summary>
        /// A list of command names (without alternatives) as used in command-line arguments in lower-case.
        /// </summary>
        internal static readonly string[] CommandNames = {Central.Name, Selection.Name, Download.Name, Update.Name, Run.Name, SelfUpdate.Name, Import.Name, List.Name, Configure.Name, AddFeed.Name, RemoveFeed.Name, ListFeeds.Name, Digest.Name, AddApp.Name, RemoveApp.Name, IntegrateApp.Name, AddAlias.Name, UpdateApps.Name, RepairApps.Name, SyncApps.Name, StoreMan.Name};

        /// <summary>
        /// Creates a nw <see cref="FrontendCommand"/> based on a name.
        /// </summary>
        /// <param name="commandName">The command name to look for; case-insensitive; may be <see langword="null"/>.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        /// <returns>The requested <see cref="FrontendCommand"/> or <see cref="DefaultCommand"/> if <paramref name="commandName"/> was <see langword="null"/>.</returns>
        /// <exception cref="OptionException">Thrown if <paramref name="commandName"/> is an unknown command.</exception>
        /// <exception cref="IOException">Thrown if there was a problem accessing a configuration file or one of the stores.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to a configuration file or one of the stores was not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a configuration file is damaged.</exception>
        private static FrontendCommand GetCommand(string commandName, IBackendHandler handler)
        {
            if (string.IsNullOrEmpty(commandName)) return new DefaultCommand(handler);
            switch (commandName.ToLowerInvariant())
            {
                case Central.Name:
                    return new Central(handler);
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
                case SelfUpdate.Name:
                    return new SelfUpdate(handler);
                case Import.Name:
                    return new Import(handler);
                case List.Name:
                    return new List(handler);
                case Configure.Name:
                    return new Configure(handler);
                case AddFeed.Name:
                    return new AddFeed(handler);
                case RemoveFeed.Name:
                    return new RemoveFeed(handler);
                case ListFeeds.Name:
                    return new ListFeeds(handler);
                case Digest.Name:
                    return new Digest(handler);
                case AddApp.Name:
                case AddApp.AltName:
                    return new AddApp(handler);
                case RemoveApp.Name:
                case RemoveApp.AltName:
                case RemoveApp.AltName2:
                    return new RemoveApp(handler);
                case IntegrateApp.Name:
                case IntegrateApp.AltName:
                case IntegrateApp.AltName2:
                    return new IntegrateApp(handler);
                case AddAlias.Name:
                case AddAlias.AltName:
                    return new AddAlias(handler);
                case UpdateApps.Name:
                case UpdateApps.AltName:
                    return new UpdateApps(handler);
                case RepairApps.Name:
                case RepairApps.AltName:
                    return new RepairApps(handler);
                case SyncApps.Name:
                    return new SyncApps(handler);
                case StoreMan.Name:
                    return new StoreMan(handler);
                default:
                    throw new OptionException(string.Format(Resources.UnknownCommand, commandName), null);
            }
        }
        #endregion

        #region Create and parse
        /// <summary>
        /// Parses command-line arguments, automatically creating an appropriate <see cref="FrontendCommand"/>.
        /// </summary>
        /// <param name="args">The command-line arguments to be parsed.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about download and IO tasks.</param>
        /// <returns>The newly created <see cref="FrontendCommand"/> after <see cref="FrontendCommand.Parse"/> has been called.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the user asked to see help information, version information, etc..</exception>
        /// <exception cref="OptionException">Thrown if <paramref name="args"/> contains unknown options or specified an unknown command.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a configuration file is damaged.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown when trying to set an invalid interface ID.</exception>
        public static FrontendCommand CreateAndParse(IEnumerable<string> args, IBackendHandler handler)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var arguments = new LinkedList<String>(args);
            FrontendCommand command = GetCommand(GetCommandName(arguments), handler);

            command.Parse(arguments);
            handler.SetGuiHints(() => command.ActionTitle, command.GuiDelay);
            return command;
        }

        /// <summary>
        /// Determines the command name specified in the command-line arguments.
        /// </summary>
        /// <param name="arguments">The command-line arguments to search for a command name. If a command is found it is removed from the collection.</param>
        /// <returns>The name of the command that was found or <see langword="null"/> if none was specified.</returns>
        private static string GetCommandName(ICollection<string> arguments)
        {
            #region Sanity checks
            if (arguments == null) throw new ArgumentNullException("arguments");
            #endregion

            string commandName = arguments.FirstOrDefault(argument => !argument.StartsWith("-") && !argument.StartsWith("/"));
            if (commandName != null) arguments.Remove(commandName);
            return commandName;
        }
        #endregion
    }
}
