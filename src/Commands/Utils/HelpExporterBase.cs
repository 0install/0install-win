/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NanoByte.Common;
using NDesk.Options;
using ZeroInstall.Commands.CliCommands;

namespace ZeroInstall.Commands.Utils
{
    /// <summary>
    /// Common base class for exporting all <see cref="CliCommand"/> help texts in a structured text format.
    /// </summary>
    public abstract class HelpExporterBase
    {
        /// <summary>
        /// Returns all <see cref="CliCommand"/> help texts in a structured text format.
        /// </summary>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine(CommandListHeader());
            ForEachCommand(command => builder.AppendLine(CommandListEntry(command.Name, command.Description)));
            builder.AppendLine(CommandListFooter());

            ForEachCommand(command =>
            {
                builder.AppendLine(CommandDetails(command.Name, command.Description, command.Usage));
                builder.AppendLine(OptionListHeader());
                foreach (var option in command.Options.Where(x => x.Prototype != "<>" && x.Description != null))
                    builder.AppendLine(OptionListEntry(GetPrototypes(option), option.Description));
                builder.AppendLine(OptionListFooter());
            });

            return builder.ToString();
        }

        private static void ForEachCommand(Action<CliCommand> action)
        {
            using (var handler = new CliCommandHandler())
            {
                foreach (string commandName in CommandFactory.CommandNames)
                {
                    var command = CommandFactory.GetCommand(commandName, handler);
                    if (command is MultiCommand multiCommand)
                    {
                        foreach (string stringCommandName in multiCommand.SubCommandNames)
                            action(multiCommand.GetCommand(stringCommandName));
                    }
                    else
                        action(command);
                }
            }
        }

        private static readonly Regex _descriptionParameterRegex = new Regex("{([A-Z]*)}");

        private static IEnumerable<string> GetPrototypes(Option option)
        {
            var parameters = _descriptionParameterRegex.Matches(option.Description).Cast<Match>().Select(x => x.Captures[0].Value).ToList();
            var prototypes = option.Prototype.TrimEnd('=').Split('|').Select(x => (x.Length == 1 ? $"-{x}" : $"--{x}"));
            if (parameters.Count > 0) prototypes = prototypes.Select(x => x + " " + StringUtils.Join(" ", parameters));
            return prototypes;
        }

        protected abstract string CommandListHeader();
        protected abstract string CommandListEntry(string name, string description);
        protected abstract string CommandListFooter();
        protected abstract string CommandDetails(string name, string description, string usage);
        protected abstract string OptionListHeader();
        protected abstract string OptionListEntry(IEnumerable<string> prototypes, string description);
        protected abstract string OptionListFooter();
    }
}