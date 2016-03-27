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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall.Commands.CliCommands
{
    partial class StoreMan
    {
        private abstract class DirCommand : StoreSubCommand
        {
            #region Metadata
            protected override string Usage { get { return "PATH"; } }

            protected override int AdditionalArgsMin { get { return 1; } }

            protected override int AdditionalArgsMax { get { return 1; } }

            /// <summary>Apply the operation machine-wide instead of just for the current user.</summary>
            protected bool MachineWide { get; private set; }

            protected DirCommand([NotNull] ICommandHandler handler) : base(handler)
            {
                Options.Add("m|machine", () => Resources.OptionMachine, _ => MachineWide = true);
            }
            #endregion

            protected string GetPath()
            {
                return Locations.IsPortable ? AdditionalArgs[0] : Path.GetFullPath(AdditionalArgs[0]);
            }

            protected IEnumerable<string> GetImplementationDirs()
            {
                return MachineWide
                    ? StoreConfig.GetMachineWideImplementationDirs()
                    : StoreConfig.GetUserImplementationDirs();
            }

            protected void SetImplementationDirs([NotNull] IEnumerable<string> paths)
            {
                if (MachineWide) StoreConfig.SetMachineWideImplementationDirs(paths);
                else StoreConfig.SetUserImplementationDirs(paths);
            }
        }

        private class AddDir : DirCommand
        {
            #region Metadata
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new const string Name = "add-dir";

            protected override string Description { get { return Resources.DescriptionStoreAddDir; } }

            public AddDir([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                string path = GetPath();

                // Init new store to ensure the target is suitable
                Store = new DirectoryStore(path);

                var dirs = GetImplementationDirs().ToList();
                if (dirs.AddIfNew(path))
                {
                    SetImplementationDirs(dirs);
                    return ExitCode.OK;
                }
                else
                {
                    Log.Warn(string.Format(Resources.AlreadyInImplDirs, path));
                    return ExitCode.NoChanges;
                }
            }
        }

        private class RemoveDir : DirCommand
        {
            #region Metadata
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new const string Name = "remove-dir";

            protected override string Description { get { return Resources.DescriptionStoreRemoveDir; } }

            public RemoveDir([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                string path = GetPath();

                var dirs = GetImplementationDirs().ToList();
                if (dirs.Remove(path))
                {
                    SetImplementationDirs(dirs);
                    return ExitCode.OK;
                }
                else
                {
                    Log.Warn(string.Format(Resources.NotInImplDirs, path));
                    return ExitCode.NoChanges;
                }
            }
        }

        internal class List : StoreSubCommand
        {
            #region Metadata
            public new const string Name = "list";

            protected override string Description { get { return Resources.DescriptionStoreList; } }

            protected override string Usage { get { return ""; } }

            protected override int AdditionalArgsMax { get { return 0; } }

            public List([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                var composite = Store as CompositeStore;
                Handler.Output(Resources.CachedInterfaces, (composite == null) ? new[] {Store} : composite.Stores);
                return ExitCode.OK;
            }
        }
    }
}
