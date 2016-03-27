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
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Downloads a set of <see cref="Implementation"/>s piped in as XML via stdin. Only for IPC-use.
    /// </summary>
    public class Fetch : CliCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "fetch";

        /// <inheritdoc/>
        protected override string Description { get { return "Downloads a set of implementations piped in as XML via stdin. Only for IPC-use."; } }

        /// <inheritdoc/>
        protected override string Usage { get { return ""; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMax { get { return 0; } }
        #endregion

        /// <inheritdoc/>
        public Fetch([NotNull] ICommandHandler handler) : base(handler)
        {}

        /// <inheritdoc/>
        public override ExitCode Execute()
        {
            string input = Console.ReadLine();
            if (string.IsNullOrEmpty(input)) return ExitCode.InvalidData;

            var feedFragment = XmlStorage.FromXmlString<Feed>(input);
            Fetcher.Fetch(feedFragment.Elements.OfType<Implementation>());

            return ExitCode.OK;
        }
    }
}
