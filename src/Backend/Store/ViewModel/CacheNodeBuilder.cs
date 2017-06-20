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
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.ViewModel
{
    /// <summary>
    /// Builds a list of <see cref="CacheNode"/>s for <see cref="Feed"/>s and <see cref="Implementation"/>s.
    /// </summary>
    public sealed class CacheNodeBuilder : TaskBase
    {
        #region Dependencies
        private readonly IStore _store;
        private readonly IFeedCache _feedCache;

        /// <summary>
        /// Creates a new list builder
        /// </summary>
        /// <param name="store">Used to list <see cref="Implementation"/>s</param>
        /// <param name="feedCache">Used to load <see cref="Feed"/>s.</param>
        public CacheNodeBuilder([NotNull] IStore store, [NotNull] IFeedCache feedCache)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _feedCache = feedCache ?? throw new ArgumentNullException(nameof(feedCache));
        }
        #endregion

        /// <inheritdoc/>
        public override string Name => "Loading";

        /// <inheritdoc/>
        protected override bool UnitsByte => false;

        /// <summary>
        /// All generated nodes.
        /// </summary>
        public NamedCollection<CacheNode> Nodes { get; private set; }

        /// <summary>
        /// The total size of all <see cref="Implementation"/>s in bytes.
        /// </summary>
        public long TotalSize { get; private set; }

        private IEnumerable<Feed> _feeds;

        /// <inheritdoc/>
        protected override void Execute()
        {
            Nodes = new NamedCollection<CacheNode>();
            _feeds = _feedCache.GetAll();

            foreach (var feed in _feeds)
            {
                feed.Normalize(feed.Uri);
                Add(feed);
            }
            foreach (var digest in _store.ListAll()) Add(digest);
            foreach (string path in _store.ListAllTemp()) Add(path);
        }

        private void Add(Feed feed) => Add(new FeedNode(feed, _feedCache));

        private void Add(ManifestDigest digest)
        {
            try
            {
                var implementation = _feeds.GetImplementation(digest, out var feed);

                ImplementationNode implementationNode;
                if (implementation == null) implementationNode = new OrphanedImplementationNode(digest, _store);
                else implementationNode = new OwnedImplementationNode(digest, implementation, new FeedNode(feed, _feedCache), _store);

                TotalSize += implementationNode.Size;
                Add(implementationNode);
            }
                #region Error handling
            catch (FormatException ex)
            {
                Log.Error($"Problem processing the manifest file for '{digest}'.");
                Log.Error(ex);
            }
            catch (IOException ex)
            {
                Log.Error($"Problem processing '{digest}'.");
                Log.Error(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error($"Problem processing '{digest}'.");
                Log.Error(ex);
            }
            #endregion
        }

        private void Add(string path) => Add(new TempDirectoryNode(path, _store));

        private void Add(CacheNode entry)
        {
            // Avoid name collisions by incrementing suffix
            while (Nodes.Contains(entry.Name)) entry.SuffixCounter++;

            Nodes.Add(entry);
        }
    }
}
