/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.IO;
using System.Text;
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Feeds;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// List all known feed IDs for a specific interface.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class ListFeeds : CommandBase
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "list-feeds";
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionListFeeds; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "[OPTIONS] URI"; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public ListFeeds(Policy policy) : base(policy)
        { }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            #region Sanity checks
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count == 0) throw new OptionException(Resources.MissingArguments, "");
            if (AdditionalArgs.Count > 1) throw new OptionException(Resources.TooManyArguments, "");
            #endregion

            string interfaceID = AdditionalArgs[0];
            if (File.Exists(AdditionalArgs[0])) interfaceID = Path.GetFullPath(AdditionalArgs[0]);

            var preferences = InterfacePreferences.LoadFor(interfaceID);
            var builder = new StringBuilder();
            foreach (var feedReference in preferences.Feeds)
                builder.AppendLine(feedReference.Source);

            Policy.Handler.Output(string.Format(Resources.FeedsRegistered, interfaceID), builder.ToString());
            return 0;
        }
        #endregion
    }
}
