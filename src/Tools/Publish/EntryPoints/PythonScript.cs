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

using System.ComponentModel;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// A script written in Python.
    /// </summary>
    public sealed class PythonScript : InterpretedScript
    {
        /// <summary>
        /// Does this application have a graphical interface an no terminal output? Only enable if you are sure!
        /// </summary>
        [Category("Details (Python)"), DisplayName(@"GUI only"), Description("Does this application have a graphical interface an no terminal output? Only enable if you are sure!")]
        [UsedImplicitly]
        public bool GuiOnly { get => !NeedsTerminal; set => NeedsTerminal = !value; }

        /// <inheritdoc/>
        internal override bool Analyze(DirectoryInfo baseDirectory, FileInfo file)
        {
            if (!base.Analyze(baseDirectory, file)) return false;
            if (StringUtils.EqualsIgnoreCase(file.Extension, @".pyw"))
            {
                GuiOnly = true;
                return true;
            }
            else if (StringUtils.EqualsIgnoreCase(file.Extension, @".py") || HasShebang(file, "python"))
            {
                GuiOnly = false;
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        protected override FeedUri InterpreterInterface => new FeedUri("http://repo.roscidus.com/python/python");

        /// <inheritdoc/>
        public override Command CreateCommand()
        {
            var command = base.CreateCommand();
            command.Runner.Command = NeedsTerminal ? Command.NameRun : Command.NameRunGui;
            return command;
        }
    }
}
