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

using JetBrains.Annotations;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Common base class for sub-commands that are aggregated by a <see cref="MultiCommand"/>.
    /// </summary>
    public abstract class SubCommand : CliCommand
    {
        /// <summary>
        /// The <see cref="CliCommand.Name"/> of the <see cref="MultiCommand"/> this command is a sub-command of.
        /// </summary>
        protected abstract string ParentName { get; }

        /// <inheritdoc/>
        public override string Name { get { return ParentName + " " + base.Name; } }

        /// <inheritdoc/>
        protected SubCommand([NotNull] ICommandHandler handler) : base(handler)
        {}
    }
}
