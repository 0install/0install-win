/*
 * Copyright 2010-2014 Bastian Eicher
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
using NanoByte.Common.Storage;
using NanoByte.Common.Utils;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Import a feed from a local file, as if it had been downloaded from the network.
    /// </summary>
    /// <remarks>This is useful when testing a feed file, to avoid uploading it to a remote server in order to download it again. The file must have a trusted digital signature, as when fetching from the network.</remarks>
    [CLSCompliant(false)]
    public sealed class Import : FrontendCommand
    {
        #region Metadata
        /// <summary>The name of this command as used in command-line arguments in lower-case.</summary>
        public new const string Name = "import";

        /// <inheritdoc/>
        protected override string Description { get { return Resources.DescriptionImport; } }

        /// <inheritdoc/>
        protected override string Usage { get { return "FEED-FILE [...]"; } }

        /// <inheritdoc/>
        protected override int AdditionalArgsMin { get { return 1; } }

        /// <inheritdoc/>
        public Import(ICommandHandler handler) : base(handler)
        {
            Options.Add("batch", () => Resources.OptionBatch, _ => Handler.Batch = true);
        }
        #endregion

        /// <inheritdoc/>
        public override int Execute()
        {
            foreach (var file in ArgumentUtils.GetFiles(AdditionalArgs, "*.xml"))
                ImportFile(file.FullName);

            return 0;
        }

        #region Helpers
        /// <summary>
        /// Import a feed from a local file, as if it had been downloaded from the network.
        /// </summary>
        /// <param name="path">The local path of the feed file to import.</param>
        /// <exception cref="InvalidDataException">The feed specifies no <see cref="Feed.Uri"/>.</exception>
        private void ImportFile(string path)
        {
            var feed = XmlStorage.LoadXml<Feed>(path);
            if (feed.Uri == null) throw new InvalidDataException(Resources.ImportNoSource);

            FeedManager.ImportFeed(path, feed.Uri, mirrorUrl: new Uri(path));
        }
        #endregion
    }
}
