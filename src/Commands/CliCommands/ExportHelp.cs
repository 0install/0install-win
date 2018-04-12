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

using JetBrains.Annotations;
using ZeroInstall.Commands.Utils;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Exports all <see cref="CliCommand"/> help texts as HTML.
    /// </summary>
    public class ExportHelp : CliCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "export-help";

        /// <inheritdoc/>
        public override string Description => "Exports all command help texts as HTML.";

        /// <inheritdoc/>
        public override string Usage => "";

        /// <inheritdoc/>
        protected override int AdditionalArgsMax => 0;
        #endregion

        #region State
        /// <inheritdoc/>
        public ExportHelp([NotNull] ICommandHandler handler)
            : base(handler)
        {}
        #endregion

        public override ExitCode Execute()
        {
            Handler.Output("Zero Install HTML Help Export", new HtmlHelpExporter().ToString());
            return ExitCode.OK;
        }
    }
}
