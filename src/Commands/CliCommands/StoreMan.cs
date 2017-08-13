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
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Manages the contents of the <see cref="IStore"/>s.
    /// </summary>
    public sealed partial class StoreMan : MultiCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "store";

        /// <inheritdoc/>
        public StoreMan([NotNull] ICommandHandler handler) : base(handler)
        {}
        #endregion

        /// <inheritdoc/>
        public override IEnumerable<string> SubCommandNames => new[] {Add.Name, Audit.Name, Copy.Name, Export.Name, Find.Name, List.Name, ListImplementations.Name, Manage.Name, Optimise.Name, Purge.Name, Remove.Name, Verify.Name, AddDir.Name, RemoveDir.Name};

        /// <inheritdoc/>
        public override SubCommand GetCommand(string commandName)
        {
            #region Sanity checks
            if (commandName == null) throw new ArgumentNullException(nameof(commandName));
            #endregion

            switch (commandName)
            {
                case Add.Name:
                    return new Add(Handler);
                case Audit.Name:
                    return new Audit(Handler);
                case Copy.Name:
                    return new Copy(Handler);
                case Export.Name:
                    return new Export(Handler);
                case Find.Name:
                    return new Find(Handler);
                case List.Name:
                    return new List(Handler);
                case ListImplementations.Name:
                    return new ListImplementations(Handler);
                case Manage.Name:
                    return new Manage(Handler);
                case "manifest":
                    throw new NotSupportedException(string.Format(Resources.UseInstead, "0install digest --manifest"));
                case Optimise.Name:
                case Optimise.AltName:
                    return new Optimise(Handler);
                case Purge.Name:
                    return new Purge(Handler);
                case Remove.Name:
                    return new Remove(Handler);
                case Verify.Name:
                    return new Verify(Handler);
                case AddDir.Name:
                    return new AddDir(Handler);
                case RemoveDir.Name:
                    return new RemoveDir(Handler);
                default:
                    throw new OptionException(string.Format(Resources.UnknownCommand, commandName), commandName);
            }
        }

        internal abstract class StoreSubCommand : SubCommand
        {
            protected override string ParentName => StoreMan.Name;

            protected StoreSubCommand([NotNull] ICommandHandler handler) : base(handler)
            {}

            /// <summary>
            /// Returns the default <see cref="IStore"/> or a <see cref="CompositeStore"/> as specifief by the <see cref="CliCommand.AdditionalArgs"/>.
            /// </summary>
            protected IStore GetEffectiveStore()
            {
                if (AdditionalArgs.Count == 0) return Store;
                else
                {
                    foreach (string path in AdditionalArgs)
                        if (!Directory.Exists(path)) throw new DirectoryNotFoundException(string.Format(Resources.FileOrDirNotFound, path));

                    return new CompositeStore(
                        AdditionalArgs.Select(x => (IStore)new DirectoryStore(x, useWriteProtection: false)));
                }
            }
        }
    }
}
