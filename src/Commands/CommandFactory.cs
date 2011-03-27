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

using System;
using System.Collections.Generic;
using System.IO;
using Common;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Handles the creation of <see cref="CommandBase"/> instances for handling of commands like "0install COMMAND [OPTIONS]".
    /// </summary>
    [CLSCompliant(false)]
    public static class CommandFactory
    {
        #region Valid commands
        /// <summary>
        /// A list of command names as used in command-line arguments in lower-case.
        /// </summary>
        internal static readonly string[] ValidCommandNames = new[] {Selection.Name, Download.Name, Update.Name, Run.Name, Import.Name, List.Name, Configure.Name, AddFeed.Name, RemoveFeed.Name, ListFeeds.Name};

        /// <summary>
        /// Creates a nw <see cref="CommandBase"/> based on a name.
        /// </summary>
        /// <param name="commandName">The command name to look for; case-insensitive; may be <see langword="null"/>.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or is to be about download and IO tasks.</param>
        /// <returns>The requested <see cref="CommandBase"/> or <see cref="DefaultCommand"/> if <paramref name="commandName"/> was <see langword="null"/>.</returns>
        /// <exception cref="OptionException">Thrown if <paramref name="commandName"/> is an unknown command.</exception>
        private static CommandBase GetCommand(string commandName, IHandler handler)
        {
            var policy = Policy.CreateDefault(handler);
            if (string.IsNullOrEmpty(commandName)) return new DefaultCommand(policy);
            switch (commandName.ToLowerInvariant())
            {
                case Selection.Name: return new Selection(policy);
                case Download.Name: return new Download(policy);
                case Update.Name: return new Update(policy);
                case Run.Name: return new Run(policy);
                case Import.Name: return new Import(policy);
                case List.Name: return new List(policy);
                case Configure.Name: return new Configure(policy);
                case AddFeed.Name: return new AddFeed(policy);
                case RemoveFeed.Name: return new RemoveFeed(policy);
                case ListFeeds.Name: return new ListFeeds(policy);
                default: throw new OptionException(string.Format(Resources.UnknownCommand, commandName), null);
            }
        }
        #endregion

        #region Create and parse
        /// <summary>
        /// Parses command-line arguments, automatically creating an appropriate <see cref="CommandBase"/>.
        /// </summary>
        /// <param name="args">The command-line arguments to be parsed.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or is to be about download and IO tasks.</param>
        /// <returns>The newly created <see cref="CommandBase"/> after <see cref="CommandBase.Parse"/> has been called.</returns>
        /// <exception cref="UserCancelException">Thrown if the user asked to see help information, version information, etc..</exception>
        /// <exception cref="OptionException">Thrown if <paramref name="args"/> contains unknown options or specified an unknown command.</exception>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown when trying to set an invalid interface ID.</exception>
        public static CommandBase CreateAndParse(IEnumerable<string> args, IHandler handler)
        {
            #region Sanity checks
            if (args == null) throw new ArgumentNullException("args");            
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var arguments = new LinkedList<String>(args);
            CommandBase command = GetCommand(GetCommandName(arguments), handler);

            command.Parse(arguments);
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

            string commandName = null;
            foreach (var argument in arguments)
            {
                if (!argument.StartsWith("-") && !argument.StartsWith("/"))
                {
                    commandName = argument;
                    arguments.Remove(argument);
                    break;
                }
            }

            return commandName;
        }
        #endregion
    }
}
