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
using System.Collections.Generic;
using System.IO;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Tasks;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands.WinForms.StoreManagementNodes
{
    /// <summary>
    /// Builds a list of <see cref="Node"/>s for <see cref="Feed"/>s and <see cref="Implementation"/>s.
    /// </summary>
    public sealed class NodeListBuilder : TaskBase
    {
        #region Dependencies
        private readonly IStore _store;
        private readonly IFeedCache _feedCache;
        private readonly ITaskHandler _handler;

        /// <summary>
        /// Creates a new list builder
        /// </summary>
        /// <param name="store">Used to list <see cref="Implementation"/>s</param>
        /// <param name="feedCache">Used to load <see cref="Feed"/>s.</param>
        /// <param name="handler">A callback object used when the the user needs to be asked questions or informed about IO tasks.</param>
        public NodeListBuilder(IStore store, IFeedCache feedCache, ITaskHandler handler)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            _handler = handler;
            _store = store;
            _feedCache = feedCache;
        }
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return "Loading"; } }

        /// <inheritdoc/>
        protected override bool UnitsByte { get { return false; } }

        /// <summary>
        /// All generated nodes.
        /// </summary>
        public NamedCollection<Node> Nodes { get; private set; }

        /// <summary>
        /// The total size of all <see cref="Implementation"/>s in bytes.
        /// </summary>
        public long TotalSize { get; private set; }
        #endregion

        private IEnumerable<Feed> _feeds;

        /// <inheritdoc/>
        protected override void Execute()
        {
            Nodes = new NamedCollection<Node>();
            _feeds = _feedCache.GetAll();

            foreach (var feed in _feeds) Add(feed);
            foreach (var digest in _store.ListAll()) Add(digest);
            foreach (string path in _store.ListAllTemp()) Add(path);

            Status = TaskStatus.Complete;
        }

        private void Add(Feed feed)
        {
            Add(new FeedNode(feed, _feedCache, _handler));
        }

        private void Add(ManifestDigest digest)
        {
            try
            {
                Feed feed;
                var implementation = _feeds.GetImplementation(digest, out feed);

                ImplementationNode implementationNode;
                if (feed == null) implementationNode = new OrphanedImplementationNode(digest, _store, _handler);
                else implementationNode = new OwnedImplementationNode(digest, implementation, new FeedNode(feed, _feedCache, _handler), _store, _handler);

                TotalSize += implementationNode.Size;
                Add(implementationNode);
            }
                #region Error handling
            catch (FormatException ex)
            {
                Msg.Inform(null, string.Format("Problem processing the manifest file for '{0}'.\n" + ex.Message, digest), MsgSeverity.Error);
            }
            catch (IOException ex)
            {
                Msg.Inform(null, string.Format("Problem processing '{0}'.\n" + ex.Message, digest), MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(null, string.Format("Problem processing '{0}'.\n" + ex.Message, digest), MsgSeverity.Error);
            }
            #endregion
        }

        private void Add(string path)
        {
            Add(new TempDirectoryNode(path, _store, _handler));
        }

        private void Add(Node entry)
        {
            // Avoid name collisions with suffix
            while (Nodes.Contains(entry.Name)) entry.SuffixCounter++;

            Nodes.Add(entry);
        }
    }
}
