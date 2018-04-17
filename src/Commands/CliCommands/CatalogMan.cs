// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

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
        public CatalogMan([NotNull] ICommandHandler handler)
            : base(handler)
        {}
        #endregion

        /// <inheritdoc/>
        public override IEnumerable<string> SubCommandNames => new[] {Search.Name, Refresh.Name, Add.Name, Remove.Name, Reset.Name, List.Name};

        /// <inheritdoc/>
        public override SubCommand GetCommand(string commandName)
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

            protected CatalogSubCommand([NotNull] ICommandHandler handler)
                : base(handler)
            {}
        }
    }
}
