/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common.Storage;
using Common.Utils;
using NDesk.Options;
using ZeroInstall.Backend;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Downloads a set of <see cref="Implementation"/>s piped in as XML via stdin. Only for IPC-use.
    /// </summary>
    [CLSCompliant(false)]
    public class Fetch : FrontendCommand
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "fetch";
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return "Downloads a set of implementations piped in as XML via stdin. Only for IPC-use."; } }

        /// <inheritdoc/>
        protected override string Usage { get { return ""; } }

        /// <inheritdoc/>
        public override string ActionTitle { get { return Resources.ActionDownload; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Fetch(Resolver resolver) : base(resolver)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count != 0) throw new OptionException(Resources.TooManyArguments + "\n" + AdditionalArgs.JoinEscapeArguments(), "");

            Resolver.Handler.ShowProgressUI();

            var feedFragment = XmlStorage.FromXmlString<Feed>(Console.ReadLine());
            Resolver.Fetcher.Fetch(feedFragment.Elements.OfType<Implementation>());
            
            return 0;
        }
        #endregion
    }
}
