// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

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
        public override string Description
        {
            get
            {
                var builder = new StringBuilder(Resources.TryHelpWith + Environment.NewLine);
                foreach (string possibleSubCommand in SubCommandNames)
                    builder.AppendLine("0install " + Name + " " + possibleSubCommand);
                return builder.ToString();
            }
        }

        /// <inheritdoc/>
        public override string Usage => "SUBCOMMAND";

        /// <inheritdoc/>
        protected override int AdditionalArgsMin => 1;
        #endregion

        /// <inheritdoc/>
        protected MultiCommand([NotNull] ICommandHandler handler)
            : base(handler)
        {
            // Defer all option parsing to the sub-commands
            Options.Clear();
        }

        /// <summary>
        /// A list of sub-command names (without alternatives) as used in command-line arguments in lower-case.
        /// </summary>
        [NotNull, ItemNotNull]
        public abstract IEnumerable<string> SubCommandNames { get; }

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
        public abstract SubCommand GetCommand([NotNull] string commandName);

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
