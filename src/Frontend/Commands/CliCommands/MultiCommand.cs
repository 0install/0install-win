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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using NDesk.Options;
using ZeroInstall.Commands.Properties;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Common base class for commands that provide multiple <see cref="SubCommand"/>s.
    /// </summary>
    public abstract class MultiCommand : CliCommand
    {
        #region Metadata
        /// <inheritdoc/>
        protected override string Description
        {
            get
            {
                var builder = new StringBuilder(Resources.TryHelpWith + Environment.NewLine);
                foreach (var possibleSubCommand in SubCommandNames)
                    builder.AppendLine("0install " + Name + " " + possibleSubCommand);
                return builder.ToString();
            }
        }

        /// <inheritdoc/>
        protected override string Usage { get { return "SUBCOMMAND"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMin { get { return 1; } }
        #endregion

        /// <inheritdoc/>
        protected MultiCommand([NotNull] ICommandHandler handler) : base(handler)
        {
            // Defer all option parsing to the sub-commands
            Options.Clear();
        }

        /// <summary>
        /// A list of sub-command names (without alternatives) as used in command-line arguments in lower-case.
        /// </summary>
        [NotNull, ItemNotNull]
        protected abstract IEnumerable<string> SubCommandNames { get; }

        /// <summary>
        /// Creates a new <see cref="SubCommand"/> based on a name.
        /// </summary>
        /// <param name="commandName">The command name to look for; case-insensitive.</param>
        /// <returns>The requested <see cref="SubCommand"/>.</returns>
        /// <exception cref="OptionException"><paramref name="commandName"/> is an unknown command.</exception>
        /// <exception cref="IOException">There was a problem accessing a configuration file or one of the stores.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file or one of the stores was not permitted.</exception>
        /// <exception cref="InvalidDataException">A configuration file is damaged.</exception>
        [NotNull]
        protected abstract SubCommand GetCommand([NotNull] string commandName);

        /// <summary>The sub-command selected in <see cref="Parse"/> and used in <see cref="Execute"/>.</summary>
        [CanBeNull]
        private SubCommand _subCommand;

        /// <inheritdoc/>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public override void Parse(IEnumerable<string> args)
        {
            base.Parse(args);

            string subCommandName = CommandFactory.GetCommandName(ref args);
            if (subCommandName == null) return;

            _subCommand = GetCommand(subCommandName);
            _subCommand.Parse(args);
        }

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            if (_subCommand == null)
            {
                Handler.Output(Resources.CommandLineArguments, HelpText);
                return ExitCode.UserCanceled;
            }

            return _subCommand.Execute();
        }
    }
}
