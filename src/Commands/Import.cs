/*
 * Copyright 2010-2012 Bastian Eicher
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
using NDesk.Options;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Import a feed from a local file, as if it had been downloaded from the network.
    /// </summary>
    /// <remarks>This is useful when testing a feed file, to avoid uploading it to a remote server in order to download it again. The file must have a trusted digital signature, as when fetching from the network.</remarks>
    [CLSCompliant(false)]
    public sealed class Import : CommandBase
    {
        #region Constants
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public const string Name = "import";
        #endregion

        #region Properties
        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionImport; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "FEED-FILE"; } }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public Import(Policy policy) : base(policy)
        {
            Options.Add("batch", Resources.OptionBatch, unused => Policy.Handler.Batch = true);
        }
        #endregion

        //--------------------//

        #region Execute
        /// <inheritdoc/>
        public override int Execute()
        {
            #region Sanity checks
            if (!IsParsed) throw new InvalidOperationException(Resources.NotParsed);
            if (AdditionalArgs.Count == 0 || string.IsNullOrEmpty(AdditionalArgs[0])) throw new OptionException(Resources.MissingArguments, "");
            if (AdditionalArgs.Count > 1) throw new OptionException(Resources.TooManyArguments, "");
            #endregion

            Policy.Handler.ShowProgressUI();
            Policy.FeedManager.ImportFeed(
                Feed.Load(AdditionalArgs[0]).Uri,
                new Uri(AdditionalArgs[0]),
                File.ReadAllBytes(AdditionalArgs[0]),
                Policy);
            return 0;
        }
        #endregion
    }
}
