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
using JetBrains.Annotations;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Manages the <see cref="Catalog"/>s provided by the <see cref="ICatalogManager"/>.
    /// </summary>
    public sealed partial class CatalogMan : MultiCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "catalog";

        /// <inheritdoc/>
        public CatalogMan([NotNull] ICommandHandler handler) : base(handler)
        {}
        #endregion

        /// <inheritdoc/>
        protected override IEnumerable<string> SubCommandNames => new[] {Search.Name, Refresh.Name, Add.Name, Remove.Name, Reset.Name, List.Name};

        /// <inheritdoc/>
        protected override SubCommand GetCommand(string commandName)
        {
            #region Sanity checks
            if (commandName == null) throw new ArgumentNullException(nameof(commandName));
            #endregion

            switch (commandName)
            {
                case Search.Name:
                    return new Search(Handler);
                case Refresh.Name:
                    return new Refresh(Handler);
                case Add.Name:
                    return new Add(Handler);
                case Remove.Name:
                    return new Remove(Handler);
                case Reset.Name:
                    return new Reset(Handler);
                case List.Name:
                    return new List(Handler);
                default:
                    throw new OptionException(string.Format(Resources.UnknownCommand, commandName), commandName);
            }
        }

        private abstract class CatalogSubCommand : SubCommand
        {
            protected override string ParentName => CatalogMan.Name;

            protected CatalogSubCommand([NotNull] ICommandHandler handler) : base(handler)
            {}
        }
    }
}
